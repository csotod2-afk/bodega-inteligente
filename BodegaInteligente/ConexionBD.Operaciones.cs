using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace BodegaInteligente
{
    /// <summary>
    /// Parte de ConexionBD para los comandos del lenguaje (REGISTRAR, ASIGNAR, ...).
    /// Mismo estilo que ConexionBD.cs: cada método abre su conexión, atrapa MySqlException
    /// y devuelve bool + mensaje para la consola. Todo con parámetros (@x), nunca
    /// concatenando texto del usuario en el SQL.
    /// </summary>
    public partial class ConexionBD
    {
        // Unidades por tamaño; debe coincidir con la vista vista_estado_espacios.
        private const string SqlUnidades =
            "CASE tamano WHEN 'GRANDE' THEN 6 WHEN 'MEDIANO' THEN 3 WHEN 'PEQUENO' THEN 2 ELSE 0 END";

        // ---------------- Consultas (IConsultasBodega) ----------------

        public bool BuscarPaquete(string idPaquete, out PaqueteInfo paquete, out string error)
        {
            paquete = null;
            error = null;
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    const string sql =
                        "SELECT id_paquete, tipo, tamano, estado, id_espacio, retiro_autorizado " +
                        "FROM paquetes WHERE id_paquete = @id";
                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@id", idPaquete);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                paquete = new PaqueteInfo
                                {
                                    IdPaquete = r["id_paquete"].ToString(),
                                    Tipo = r["tipo"].ToString(),
                                    Tamano = r["tamano"].ToString(),
                                    Estado = r["estado"].ToString(),
                                    IdEspacio = r["id_espacio"] == DBNull.Value ? null : r["id_espacio"].ToString(),
                                    RetiroAutorizado = Convert.ToBoolean(r["retiro_autorizado"])
                                };
                            }
                        }
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                error = "No se pudo consultar el paquete: " + ex.Message;
                return false;
            }
        }

        public bool BuscarEspacio(string idEspacio, out EspacioEstado espacio, out string error)
        {
            espacio = null;
            error = null;
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    const string sql =
                        "SELECT id_espacio, zona, soporta_fragil, capacidad_total, unidades_ocupadas, estado " +
                        "FROM vista_estado_espacios WHERE id_espacio = @id";
                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@id", idEspacio);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                espacio = new EspacioEstado
                                {
                                    IdEspacio = r["id_espacio"].ToString(),
                                    Zona = r["zona"].ToString(),
                                    SoportaFragil = Convert.ToBoolean(r["soporta_fragil"]),
                                    CapacidadTotal = Convert.ToInt32(r["capacidad_total"]),
                                    UnidadesOcupadas = Convert.ToInt32(r["unidades_ocupadas"]),
                                    Estado = r["estado"].ToString()
                                };
                            }
                        }
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                error = "No se pudo consultar el espacio: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Una alarma está activa si lo último que dice el historial de ese paquete, entre
        /// ALARMA y APAGAR_ALARMA, es ALARMA.
        /// </summary>
        public bool AlarmaActiva(string idPaquete, out bool activa, out string error)
        {
            activa = false;
            error = null;
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    const string sql =
                        "SELECT accion FROM historial WHERE id_paquete = @id " +
                        "AND accion IN ('ALARMA', 'APAGAR_ALARMA') ORDER BY id_historial DESC LIMIT 1";
                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@id", idPaquete);
                        object r = cmd.ExecuteScalar();
                        activa = r != null && r.ToString() == "ALARMA";
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                error = "No se pudo consultar la alarma: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Unidades ocupadas (ALMACENADO) más las reservadas por ASIGNAR (REGISTRADO con espacio),
        /// para no asignar dos paquetes al mismo hueco antes de almacenarlos.
        /// </summary>
        public bool UnidadesComprometidas(string idEspacio, out int unidades, out string error)
        {
            unidades = 0;
            error = null;
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    string sql =
                        "SELECT COALESCE(SUM(" + SqlUnidades + "), 0) FROM paquetes " +
                        "WHERE id_espacio = @id AND estado IN ('REGISTRADO', 'ALMACENADO')";
                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@id", idEspacio);
                        unidades = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                error = "No se pudo calcular la ocupación del espacio: " + ex.Message;
                return false;
            }
        }

        // ---------------- Ejecución de comandos ya validados ----------------

        /// <summary>
        /// Ejecuta una instrucción que Semantico.Validar aceptó (acciones semánticas).
        /// Aplica el cambio y deja la bitácora en historial dentro de la misma transacción.
        /// espacioAsignado solo se usa en ASIGNAR: es el espacio que eligió el Semántico.
        /// Para CONSULTAR el mensaje puede traer varias líneas separadas por '\n'.
        /// </summary>
        public bool Ejecutar(Instruccion ins, string espacioAsignado, out string mensaje)
        {
            try
            {
                // Estado del paquete antes del cambio (para saber su espacio en los mensajes).
                string idPaquete = null;
                PaqueteInfo antes = null;
                string ignorado;
                if (ins.Comando == "REGISTRAR")
                {
                    string tipoR, tamR;
                    Semantico.LeerRegistro(ins, out idPaquete, out tipoR, out tamR);
                }
                else if (ins.Argumentos.Count > 0)
                {
                    idPaquete = Semantico.Normalizar(ins.Argumentos[0].Valor);
                    if (ins.Comando != "CONSULTAR") BuscarPaquete(idPaquete, out antes, out ignorado);
                }
                string espacioAntes = antes != null ? antes.IdEspacio : null;

                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    using (var tx = cn.BeginTransaction())
                    {
                        switch (ins.Comando)
                        {
                            case "REGISTRAR":
                                {
                                    string id, tipo, tamano;
                                    Semantico.LeerRegistro(ins, out id, out tipo, out tamano);
                                    EjecutarSql(cn, tx,
                                        "INSERT INTO paquetes (id_paquete, tipo, tamano) VALUES (@p, @t, @m)",
                                        "@p", id, "@t", tipo, "@m", tamano);
                                    Bitacora(cn, tx, id, "REGISTRAR", tipo + ", " + tamano);
                                    mensaje = "Paquete " + id + " registrado (" + tipo + ", " + tamano + ").";
                                    break;
                                }
                            case "ASIGNAR":
                                {
                                    EjecutarSql(cn, tx, "UPDATE paquetes SET id_espacio = @e WHERE id_paquete = @p",
                                        "@e", espacioAsignado, "@p", idPaquete);
                                    Bitacora(cn, tx, idPaquete, "ASIGNAR", "Espacio " + espacioAsignado);
                                    mensaje = "Paquete " + idPaquete + " asignado automáticamente al espacio " + espacioAsignado + ".";
                                    break;
                                }
                            case "ALMACENAR":
                                {
                                    EjecutarSql(cn, tx, "UPDATE paquetes SET estado = 'ALMACENADO' WHERE id_paquete = @p",
                                        "@p", idPaquete);
                                    Bitacora(cn, tx, idPaquete, "ALMACENAR", "Espacio " + espacioAntes);
                                    // Requisito del PDF: al almacenar se activa la seguridad del paquete.
                                    Bitacora(cn, tx, idPaquete, "SEGURIDAD_ACTIVADA", "Espacio " + espacioAntes);
                                    mensaje = "Paquete " + idPaquete + " almacenado en " + espacioAntes +
                                              ". Seguridad activada: no se puede retirar sin AUTORIZAR_RETIRO.";
                                    break;
                                }
                            case "AUTORIZAR_RETIRO":
                                {
                                    // Sigue ALMACENADO (ocupando lugar en el mapa) hasta que se haga RETIRAR.
                                    EjecutarSql(cn, tx, "UPDATE paquetes SET retiro_autorizado = 1 WHERE id_paquete = @p",
                                        "@p", idPaquete);
                                    Bitacora(cn, tx, idPaquete, "AUTORIZAR_RETIRO", "Espacio " + espacioAntes);
                                    mensaje = "Retiro del paquete " + idPaquete + " autorizado.";
                                    break;
                                }
                            case "RETIRAR":
                                {
                                    // Libera el espacio automáticamente: RETIRADO y sin espacio.
                                    EjecutarSql(cn, tx,
                                        "UPDATE paquetes SET estado = 'RETIRADO', id_espacio = NULL, retiro_autorizado = 0 " +
                                        "WHERE id_paquete = @p", "@p", idPaquete);
                                    Bitacora(cn, tx, idPaquete, "RETIRAR", "Espacio " + espacioAntes + " liberado");
                                    mensaje = "Paquete " + idPaquete + " retirado. Espacio " + espacioAntes + " liberado.";
                                    break;
                                }
                            case "APAGAR_ALARMA":
                                {
                                    Bitacora(cn, tx, idPaquete, "APAGAR_ALARMA", "Espacio " + espacioAntes);
                                    mensaje = "Alarma del paquete " + idPaquete + " (espacio " + espacioAntes + ") apagada.";
                                    break;
                                }
                            case "CONSULTAR":
                                {
                                    mensaje = null; // solo lectura: se arma después del commit
                                    break;
                                }
                            default:
                                tx.Rollback();
                                mensaje = "Comando sin ejecución definida: " + ins.Comando;
                                return false;
                        }
                        tx.Commit();
                        if (ins.Comando == "CONSULTAR")
                            mensaje = ins.Argumentos.Count == 0 ? ResumenBodega() : Describir(idPaquete);
                        return true;
                    }
                }
            }
            catch (MySqlException ex)
            {
                mensaje = "Error de base de datos al ejecutar " + ins.Comando + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>Deja una ALARMA en el historial (p. ej. retiro no autorizado).</summary>
        public void RegistrarAlarma(string idPaquete, string detalle)
        {
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    Bitacora(cn, null, idPaquete, "ALARMA", detalle);
                }
            }
            catch (MySqlException)
            {
                // Si la BD falla aquí, el aviso ya salió en la consola; no se vuelve a lanzar.
            }
        }

        // CONSULTAR(): estado de toda la bodega, una línea por espacio (separadas por '\n').
        private string ResumenBodega()
        {
            List<EspacioEstado> espacios;
            string err;
            if (!ObtenerEstadoEspacios(out espacios, out err)) return err;

            var sb = new StringBuilder("Estado de la bodega (" + espacios.Count + " espacios):");
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    foreach (EspacioEstado e in espacios)
                    {
                        sb.Append("\n  " + e.IdEspacio + " (zona " + e.Zona + (e.SoportaFragil ? ", frágiles" : "") + "): " +
                                  e.Estado + " " + e.UnidadesOcupadas + "/" + e.CapacidadTotal);

                        const string sql =
                            "SELECT id_paquete, tamano, estado FROM paquetes WHERE id_espacio = @e " +
                            "AND estado IN ('REGISTRADO', 'ALMACENADO') ORDER BY id_paquete";
                        using (var cmd = new MySqlCommand(sql, cn))
                        {
                            cmd.Parameters.AddWithValue("@e", e.IdEspacio);
                            using (var r = cmd.ExecuteReader())
                            {
                                var lista = new List<string>();
                                while (r.Read())
                                    lista.Add(r["id_paquete"] + " " + r["tamano"] +
                                              (r["estado"].ToString() == "REGISTRADO" ? " (asignado)" : ""));
                                if (lista.Count > 0) sb.Append(" - " + string.Join(", ", lista));
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                sb.Append("\n  No se pudo leer el detalle de paquetes: " + ex.Message);
            }
            return sb.ToString();
        }

        // Texto de CONSULTAR(id): busca primero como paquete y luego como espacio.
        private string Describir(string id)
        {
            PaqueteInfo p; EspacioEstado e; string err;
            if (BuscarPaquete(id, out p, out err) && p != null)
                return "Paquete " + p.IdPaquete + ": " + p.Tipo + ", " + p.Tamano + ", estado " + p.Estado +
                       (p.IdEspacio != null ? ", espacio " + p.IdEspacio : ", sin espacio") +
                       (p.RetiroAutorizado ? ", retiro autorizado" : "") + ".";
            if (BuscarEspacio(id, out e, out err) && e != null)
                return "Espacio " + e.IdEspacio + " (zona " + e.Zona + "): " + e.Estado + ", " +
                       e.UnidadesOcupadas + "/" + e.CapacidadTotal + " unidades" +
                       (e.SoportaFragil ? ", apto para frágiles" : "") + ".";
            return err ?? "Sin resultados para '" + id + "'.";
        }

        // ---------------- Auxiliares ----------------

        // Ejecuta un INSERT/UPDATE con pares (nombre, valor) como parámetros.
        private static void EjecutarSql(MySqlConnection cn, MySqlTransaction tx, string sql, params string[] paresParametro)
        {
            using (var cmd = new MySqlCommand(sql, cn, tx))
            {
                for (int i = 0; i + 1 < paresParametro.Length; i += 2)
                    cmd.Parameters.AddWithValue(paresParametro[i], paresParametro[i + 1]);
                cmd.ExecuteNonQuery();
            }
        }

        private static void Bitacora(MySqlConnection cn, MySqlTransaction tx, string idPaquete, string accion, string detalle)
        {
            // historial.detalle es VARCHAR(200): recortar evita que un mensaje largo (p. ej. una alarma) falle.
            if (detalle != null && detalle.Length > 200) detalle = detalle.Substring(0, 200);
            using (var cmd = new MySqlCommand(
                "INSERT INTO historial (id_paquete, accion, detalle) VALUES (@p, @a, @d)", cn, tx))
            {
                cmd.Parameters.AddWithValue("@p", (object)idPaquete ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@a", accion);
                cmd.Parameters.AddWithValue("@d", (object)detalle ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }
}