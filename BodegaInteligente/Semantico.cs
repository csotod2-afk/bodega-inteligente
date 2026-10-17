using System;
using System.Collections.Generic;

namespace BodegaInteligente
{
    /// <summary>Datos de un paquete tal como están en la tabla paquetes.</summary>
    public class PaqueteInfo
    {
        public string IdPaquete { get; set; }
        public string Tipo { get; set; }        // NORMAL | FRAGIL
        public string Tamano { get; set; }      // PEQUENO | MEDIANO | GRANDE
        public string Estado { get; set; }      // REGISTRADO | ALMACENADO | AUTORIZADO_RETIRO | RETIRADO
        public string IdEspacio { get; set; }   // null si no tiene espacio asignado
        public bool RetiroAutorizado { get; set; }
    }

    /// <summary>
    /// Resultado de validar una instrucción.
    /// Ok = true  -> se puede ejecutar contra la BD.
    /// Ok = false -> Mensaje explica la regla de negocio violada.
    /// EsAlarma   -> la violación es grave (p. ej. retiro no autorizado): además de avisar
    ///               en la consola, el ensamble debe dejarla en el historial.
    /// </summary>
    public class ResultadoSemantico
    {
        public bool Ok { get; private set; }
        public bool EsAlarma { get; private set; }
        public string Mensaje { get; private set; }

        /// <summary>Solo para ASIGNAR: el espacio que el Semántico eligió automáticamente.</summary>
        public string EspacioAsignado { get; private set; }

        private ResultadoSemantico() { }

        public static ResultadoSemantico Valido(string espacioAsignado = null)
        {
            return new ResultadoSemantico { Ok = true, EspacioAsignado = espacioAsignado };
        }

        public static ResultadoSemantico Error(string mensaje)
        {
            return new ResultadoSemantico { Ok = false, Mensaje = mensaje };
        }

        public static ResultadoSemantico Alarma(string mensaje)
        {
            return new ResultadoSemantico { Ok = false, EsAlarma = true, Mensaje = mensaje };
        }
    }

    /// <summary>
    /// Lo mínimo que el analizador semántico necesita saber de la bodega.
    /// ConexionBD lo implementa (ConexionBD.Operaciones.cs); así Semantico se puede
    /// probar con datos falsos sin tener MySQL levantado.
    /// Cada método devuelve false si la BD falló (y deja el motivo en "error").
    /// </summary>
    public interface IConsultasBodega
    {
        /// <summary>ok=true y paquete=null significa "consulta correcta, el paquete no existe".</summary>
        bool BuscarPaquete(string idPaquete, out PaqueteInfo paquete, out string error);

        /// <summary>ok=true y espacio=null significa "consulta correcta, el espacio no existe".</summary>
        bool BuscarEspacio(string idEspacio, out EspacioEstado espacio, out string error);

        /// <summary>Todos los espacios con su estado (para elegir dónde asignar).</summary>
        bool ObtenerEstadoEspacios(out List<EspacioEstado> espacios, out string mensaje);

        /// <summary>true si el paquete tiene una alarma encendida (la última alarma no fue apagada).</summary>
        bool AlarmaActiva(string idPaquete, out bool activa, out string error);

        /// <summary>
        /// Unidades que ya ocupan o tienen reservadas los paquetes de ese espacio
        /// (estados REGISTRADO con espacio asignado y ALMACENADO).
        /// </summary>
        bool UnidadesComprometidas(string idEspacio, out int unidades, out string error);
    }

    /// <summary>
    /// Analizador semántico: valida las reglas de negocio de la bodega sobre una
    /// instrucción que el Parser ya aceptó. No modifica la BD, solo la consulta.
    ///
    /// Comandos (PDF del curso, sección 7):
    ///   REGISTRAR(tipo, id, tamaño)  ASIGNAR(id)  ALMACENAR(id)  AUTORIZAR_RETIRO(id)
    ///   RETIRAR(id)  APAGAR_ALARMA(id)  CONSULTAR()
    /// Ciclo de vida de un paquete:
    ///   REGISTRAR -> ASIGNAR (el sistema elige el espacio) -> ALMACENAR -> AUTORIZAR_RETIRO -> RETIRAR
    /// </summary>
    public class Semantico
    {
        public const int CapacidadPorEspacio = 6;

        private readonly IConsultasBodega _bd;

        public Semantico(IConsultasBodega bd)
        {
            _bd = bd;
        }

        /// <summary>Unidades que ocupa cada tamaño (coincide con la vista vista_estado_espacios).</summary>
        public static int UnidadesDe(string tamano)
        {
            switch (tamano)
            {
                case "GRANDE": return 6;
                case "MEDIANO": return 3;
                case "PEQUENO": return 2;
                default: return 0;
            }
        }

        public ResultadoSemantico Validar(Instruccion ins)
        {
            switch (ins.Comando)
            {
                case "REGISTRAR": return ValidarRegistrar(ins);
                case "ASIGNAR": return ValidarAsignar(ins);
                case "ALMACENAR": return ValidarAlmacenar(ins);
                case "AUTORIZAR_RETIRO": return ValidarAutorizarRetiro(ins);
                case "RETIRAR": return ValidarRetirar(ins);
                case "CONSULTAR": return ValidarConsultar(ins);
                case "APAGAR_ALARMA": return ValidarApagarAlarma(ins);
                default:
                    return ResultadoSemantico.Error(Donde(ins) + "Comando desconocido '" + ins.Comando + "'.");
            }
        }

        // ------------------------------------------------------------------
        // Reglas por comando
        // ------------------------------------------------------------------

        // REGISTRAR(tipo, id, tamaño), p. ej. REGISTRAR("FRAGIL", "P01", "MEDIANO").
        // Por comodidad también se acepta REGISTRAR("P01", "FRAGIL", "MEDIANO"), el orden
        // del ejemplo del README (ver LeerRegistro).
        private ResultadoSemantico ValidarRegistrar(Instruccion ins)
        {
            ResultadoSemantico err = PedirTexto(ins, 3, "REGISTRAR(tipo, id, tamaño)");
            if (err != null) return err;

            string id, tipo, tamano;
            if (!LeerRegistro(ins, out id, out tipo, out tamano))
                return ResultadoSemantico.Error(Donde(ins) + "No se encontró el tipo de paquete. Formato: " +
                                                "REGISTRAR(tipo, id, tamaño) con tipo NORMAL o FRAGIL.");
            if (tamano != "PEQUENO" && tamano != "MEDIANO" && tamano != "GRANDE")
            {
                Token a = ins.Argumentos[2];
                return ResultadoSemantico.Error("[fila " + a.Fila + ", col " + a.Columna + "] Tamaño inválido '" +
                                                a.Valor + "'. Valores permitidos: PEQUENO, MEDIANO, GRANDE.");
            }
            if (id.Length > 10) // columna VARCHAR(10)
                return ResultadoSemantico.Error(Donde(ins) + "El id '" + id + "' es demasiado largo (máximo 10 caracteres).");

            PaqueteInfo existente;
            string errorBD;
            if (!_bd.BuscarPaquete(id, out existente, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (existente != null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + id + "' ya está registrado.");

            return ResultadoSemantico.Valido();
        }

        /// <summary>
        /// Interpreta los 3 argumentos de REGISTRAR (ya validados como texto) y los devuelve normalizados.
        /// El tamaño siempre va al final; el tipo (NORMAL/FRAGIL) puede ir primero (orden oficial)
        /// o segundo (orden del README). Devuelve false si ninguno de los dos es un tipo válido.
        /// La usan tanto Validar como ConexionBD.Ejecutar, para que ambos lean igual.
        /// </summary>
        public static bool LeerRegistro(Instruccion ins, out string id, out string tipo, out string tamano)
        {
            id = tipo = tamano = null;
            if (ins.Argumentos.Count != 3) return false;
            string a = Texto(ins, 0), b = Texto(ins, 1), c = Texto(ins, 2);
            if (EsTipo(a)) { tipo = a; id = b; }
            else if (EsTipo(b)) { id = a; tipo = b; }
            else return false;
            tamano = c;
            return true;
        }

        private static bool EsTipo(string s) { return s == "NORMAL" || s == "FRAGIL"; }

        // ASIGNAR(id): el sistema elige el espacio adecuado (asignación automática).
        // Reglas: un frágil solo va a espacios que soportan frágiles; debe caber por unidades
        // (1 Grande / 2 Medianos / 3 Pequeños). Entre los que sirven prefiere el más lleno,
        // para dejar espacios libres; un paquete NORMAL deja los espacios de frágiles como último recurso.
        private ResultadoSemantico ValidarAsignar(Instruccion ins)
        {
            ResultadoSemantico err = PedirTexto(ins, 1, "ASIGNAR(id)");
            if (err != null) return err;

            string idPaquete = Texto(ins, 0), errorBD;
            PaqueteInfo p;
            if (!_bd.BuscarPaquete(idPaquete, out p, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (p == null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete + "' no existe. Regístralo primero con REGISTRAR.");
            if (p.Estado != "REGISTRADO")
                return ResultadoSemantico.Error(Donde(ins) + "Solo se puede asignar un paquete en estado REGISTRADO; '" +
                                                idPaquete + "' está " + p.Estado + ".");
            if (p.IdEspacio != null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete + "' ya tiene el espacio " +
                                                p.IdEspacio + " asignado.");

            List<EspacioEstado> espacios;
            if (!_bd.ObtenerEstadoEspacios(out espacios, out errorBD)) return ResultadoSemantico.Error(errorBD);

            int necesarias = UnidadesDe(p.Tamano);
            EspacioEstado elegido = null;
            int elegidoComprometidas = -1;
            bool elegidoPenaliza = false;
            bool hayEspacioParaFragil = false;

            foreach (EspacioEstado e in espacios)
            {
                if (p.Tipo == "FRAGIL" && !e.SoportaFragil) continue;
                hayEspacioParaFragil = true;

                int comprometidas;
                if (!_bd.UnidadesComprometidas(e.IdEspacio, out comprometidas, out errorBD)) return ResultadoSemantico.Error(errorBD);
                if (e.CapacidadTotal - comprometidas < necesarias) continue;

                bool penaliza = (p.Tipo == "NORMAL" && e.SoportaFragil);
                bool mejor = elegido == null
                          || (elegidoPenaliza && !penaliza)
                          || (elegidoPenaliza == penaliza && comprometidas > elegidoComprometidas);
                if (mejor)
                {
                    elegido = e;
                    elegidoComprometidas = comprometidas;
                    elegidoPenaliza = penaliza;
                }
            }

            if (elegido == null)
            {
                if (p.Tipo == "FRAGIL" && !hayEspacioParaFragil)
                    return ResultadoSemantico.Error(Donde(ins) + "No existe ningún espacio que soporte paquetes frágiles.");
                return ResultadoSemantico.Error(Donde(ins) + "No hay espacio disponible para el paquete '" + idPaquete + "' (" +
                                                p.Tipo + ", " + p.Tamano + ", " + necesarias + " unidades" +
                                                (p.Tipo == "FRAGIL" ? "; solo caben en espacios para frágiles" : "") + ").");
            }
            return ResultadoSemantico.Valido(elegido.IdEspacio);
        }

        // ALMACENAR("P01"): confirma que el paquete ya está físicamente en su espacio asignado.
        private ResultadoSemantico ValidarAlmacenar(Instruccion ins)
        {
            ResultadoSemantico err = PedirTexto(ins, 1, "ALMACENAR(id_paquete)");
            if (err != null) return err;

            string idPaquete = Texto(ins, 0), errorBD;
            PaqueteInfo p;
            if (!_bd.BuscarPaquete(idPaquete, out p, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (p == null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete + "' no existe.");
            if (p.Estado != "REGISTRADO")
                return ResultadoSemantico.Error(Donde(ins) + "Solo se puede almacenar un paquete REGISTRADO; '" +
                                                idPaquete + "' está " + p.Estado + ".");
            if (p.IdEspacio == null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete +
                                                "' no tiene espacio asignado. Usa ASIGNAR primero.");
            return ResultadoSemantico.Valido();
        }

        // AUTORIZAR_RETIRO("P01")
        private ResultadoSemantico ValidarAutorizarRetiro(Instruccion ins)
        {
            ResultadoSemantico err = PedirTexto(ins, 1, "AUTORIZAR_RETIRO(id_paquete)");
            if (err != null) return err;

            string idPaquete = Texto(ins, 0), errorBD;
            PaqueteInfo p;
            if (!_bd.BuscarPaquete(idPaquete, out p, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (p == null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete + "' no existe.");
            if (p.Estado != "ALMACENADO")
                return ResultadoSemantico.Error(Donde(ins) + "Solo se puede autorizar el retiro de un paquete ALMACENADO; '" +
                                                idPaquete + "' está " + p.Estado + ".");
            if (p.RetiroAutorizado)
                return ResultadoSemantico.Error(Donde(ins) + "El retiro de '" + idPaquete + "' ya estaba autorizado.");
            return ResultadoSemantico.Valido();
        }

        // RETIRAR("P01"): sin autorización previa es una ALARMA (regla de docs/DER.md).
        private ResultadoSemantico ValidarRetirar(Instruccion ins)
        {
            ResultadoSemantico err = PedirTexto(ins, 1, "RETIRAR(id_paquete)");
            if (err != null) return err;

            string idPaquete = Texto(ins, 0), errorBD;
            PaqueteInfo p;
            if (!_bd.BuscarPaquete(idPaquete, out p, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (p == null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete + "' no existe.");
            if (p.Estado == "RETIRADO")
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + idPaquete + "' ya fue retirado.");
            if (p.Estado != "ALMACENADO")
                return ResultadoSemantico.Error(Donde(ins) + "No se puede retirar '" + idPaquete +
                                                "': no está almacenado (estado " + p.Estado + ").");
            if (!p.RetiroAutorizado)
                return ResultadoSemantico.Alarma("¡ALARMA! Retiro NO autorizado del paquete '" + idPaquete +
                                                 "' en el espacio " + (p.IdEspacio ?? "?") +
                                                 ". Usa AUTORIZAR_RETIRO antes de RETIRAR.");
            return ResultadoSemantico.Valido();
        }

        // CONSULTAR(): estado de toda la bodega. Extra: CONSULTAR("P01") o CONSULTAR("E01")
        // informa de un paquete o un espacio en particular.
        private ResultadoSemantico ValidarConsultar(Instruccion ins)
        {
            if (ins.Argumentos.Count == 0) return ResultadoSemantico.Valido();

            ResultadoSemantico err = PedirTexto(ins, 1, "CONSULTAR() o CONSULTAR(id)");
            if (err != null) return err;

            string id = Texto(ins, 0), errorBD;
            PaqueteInfo p;
            if (!_bd.BuscarPaquete(id, out p, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (p != null) return ResultadoSemantico.Valido();

            EspacioEstado e;
            if (!_bd.BuscarEspacio(id, out e, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (e != null) return ResultadoSemantico.Valido();

            return ResultadoSemantico.Error(Donde(ins) + "No existe ningún paquete ni espacio con id '" + id + "'.");
        }

        // APAGAR_ALARMA(id): solo si ese paquete tiene una alarma encendida.
        private ResultadoSemantico ValidarApagarAlarma(Instruccion ins)
        {
            ResultadoSemantico err = PedirTexto(ins, 1, "APAGAR_ALARMA(id)");
            if (err != null) return err;

            string id = Texto(ins, 0), errorBD;
            PaqueteInfo p;
            if (!_bd.BuscarPaquete(id, out p, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (p == null)
                return ResultadoSemantico.Error(Donde(ins) + "El paquete '" + id + "' no existe.");

            bool activa;
            if (!_bd.AlarmaActiva(id, out activa, out errorBD)) return ResultadoSemantico.Error(errorBD);
            if (!activa)
                return ResultadoSemantico.Error(Donde(ins) + "No hay ninguna alarma activa para el paquete '" + id + "'.");
            return ResultadoSemantico.Valido();
        }

        // ------------------------------------------------------------------
        // Utilidades de validación de argumentos
        // ------------------------------------------------------------------

        private static string Donde(Instruccion ins)
        {
            return "[fila " + ins.Fila + ", col " + ins.Columna + "] ";
        }

        /// <summary>Texto del argumento i, normalizado (mayúsculas, Ñ -> N, sin espacios en los bordes).</summary>
        private static string Texto(Instruccion ins, int i)
        {
            return Normalizar(ins.Argumentos[i].Valor);
        }

        public static string Normalizar(string s)
        {
            return (s ?? "").Trim().ToUpperInvariant().Replace('Ñ', 'N');
        }

        /// <summary>
        /// Verifica cantidad de argumentos y que todos sean texto (CADENA o ID).
        /// Devuelve null si está bien, o el error.
        /// </summary>
        private ResultadoSemantico PedirTexto(Instruccion ins, int cantidad, string firma)
        {
            if (ins.Argumentos.Count != cantidad)
                return ResultadoSemantico.Error(Donde(ins) + ins.Comando + " espera " + cantidad + " argumento(s): " +
                                                firma + ", pero recibió " + ins.Argumentos.Count + ".");
            for (int i = 0; i < ins.Argumentos.Count; i++)
            {
                Token a = ins.Argumentos[i];
                if (a.Tipo != TipoToken.CADENA && a.Tipo != TipoToken.ID)
                    return ResultadoSemantico.Error("[fila " + a.Fila + ", col " + a.Columna + "] El argumento " + (i + 1) +
                                                    " de " + ins.Comando + " debe ser texto (\"P01\" o P01), no " + a.Tipo + ".");
                if (Normalizar(a.Valor).Length == 0)
                    return ResultadoSemantico.Error("[fila " + a.Fila + ", col " + a.Columna + "] El argumento " + (i + 1) +
                                                    " de " + ins.Comando + " está vacío.");
            }
            return null;
        }
    }
}