using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace BodegaInteligente
{
    /// <summary>
    /// Acceso a MySQL (base bodega_db, ver db/bodega_db.sql).
    /// Cada método abre su propia conexión, atrapa MySqlException y devuelve
    /// un booleano más un mensaje listo para mostrar en la consola del Form1;
    /// así la GUI nunca se cae si la base de datos no está disponible.
    /// </summary>
    public class ConexionBD
    {
        /// <summary>Cadena de conexión por defecto. Ajusta usuario y contraseña a tu MySQL.</summary>
        public const string CadenaPorDefecto =
            "Server=localhost;Port=3306;Database=bodega_db;Uid=root;Pwd=;";

        private readonly string _cadenaConexion;

        public ConexionBD() : this(CadenaPorDefecto) { }

        public ConexionBD(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        /// <summary>
        /// Abre una conexión y ejecuta SELECT 1. Devuelve true si MySQL respondió 1.
        /// En "mensaje" queda el resultado o el motivo del fallo (para mostrarlo en la GUI).
        /// </summary>
        public bool ProbarConexion(out string mensaje)
        {
            try
            {
                using (var conexion = new MySqlConnection(_cadenaConexion))
                {
                    conexion.Open();
                    using (var comando = new MySqlCommand("SELECT 1", conexion))
                    {
                        object resultado = comando.ExecuteScalar();
                        bool ok = Convert.ToInt32(resultado) == 1;
                        mensaje = ok
                            ? "Conexión a MySQL correcta (SELECT 1 = 1). Servidor " + conexion.ServerVersion
                            : "MySQL respondió algo inesperado: " + resultado;
                        return ok;
                    }
                }
            }
            catch (MySqlException ex)
            {
                mensaje = "No se pudo conectar a MySQL: " + ex.Message;
                return false;
            }
        }

        /// <summary>Lee el estado de todos los espacios (vista vista_estado_espacios) para pintar el mapa.</summary>
        public bool ObtenerEstadoEspacios(out List<EspacioEstado> espacios, out string mensaje)
        {
            espacios = new List<EspacioEstado>();
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    const string sql =
                        "SELECT id_espacio, zona, soporta_fragil, capacidad_total, unidades_ocupadas, estado " +
                        "FROM vista_estado_espacios ORDER BY id_espacio";

                    using (var cmd = new MySqlCommand(sql, cn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            espacios.Add(new EspacioEstado
                            {
                                IdEspacio = reader["id_espacio"].ToString(),
                                Zona = reader["zona"].ToString(),
                                SoportaFragil = Convert.ToBoolean(reader["soporta_fragil"]),
                                CapacidadTotal = Convert.ToInt32(reader["capacidad_total"]),
                                UnidadesOcupadas = Convert.ToInt32(reader["unidades_ocupadas"]),
                                Estado = reader["estado"].ToString()
                            });
                        }
                    }
                }
                mensaje = espacios.Count + " espacio(s) cargado(s) desde MySQL.";
                return true;
            }
            catch (MySqlException ex)
            {
                mensaje = "No se pudo leer el mapa de la bodega: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Agrega un espacio nuevo con id autogenerado (E01, E02, ...), zona 'D' genérica,
        /// sin frágiles y capacidad 6. Pensado para el botón "Agregar espacio" de la demo.
        /// </summary>
        public bool AgregarEspacio(out string mensaje)
        {
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();
                    string siguienteId = CalcularSiguienteId(cn);

                    const string sql =
                        "INSERT INTO espacios (id_espacio, zona, soporta_fragil, capacidad_total) " +
                        "VALUES (@id, 'D', 0, 6)";
                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@id", siguienteId);
                        cmd.ExecuteNonQuery();
                    }
                    mensaje = "Se agregó el espacio " + siguienteId + ".";
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                mensaje = "No se pudo agregar el espacio: " + ex.Message;
                return false;
            }
        }

        /// <summary>Calcula el siguiente id (E01, E02, ...) a partir del mayor existente.</summary>
        private string CalcularSiguienteId(MySqlConnection cn)
        {
            const string sql = "SELECT id_espacio FROM espacios ORDER BY id_espacio DESC LIMIT 1";
            using (var cmd = new MySqlCommand(sql, cn))
            {
                object resultado = cmd.ExecuteScalar();
                int siguienteNumero = 1;
                if (resultado != null)
                {
                    string ultimoId = resultado.ToString();           // ej. "E06"
                    string parteNumerica = ultimoId.Length > 1 ? ultimoId.Substring(1) : "0";
                    int numero;
                    if (int.TryParse(parteNumerica, out numero))
                        siguienteNumero = numero + 1;
                }
                return "E" + siguienteNumero.ToString("D2");
            }
        }

        /// <summary>
        /// Elimina el espacio con mayor id, pero solo si está LIBRE (0 unidades ocupadas);
        /// evita borrar un espacio que todavía tiene paquetes almacenados.
        /// </summary>
        public bool EliminarUltimoEspacioLibre(out string mensaje)
        {
            try
            {
                using (var cn = new MySqlConnection(_cadenaConexion))
                {
                    cn.Open();

                    const string sqlUltimo =
                        "SELECT id_espacio, unidades_ocupadas FROM vista_estado_espacios " +
                        "ORDER BY id_espacio DESC LIMIT 1";

                    string idEspacio = null;
                    int ocupadas = 0;
                    using (var cmd = new MySqlCommand(sqlUltimo, cn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            idEspacio = reader["id_espacio"].ToString();
                            ocupadas = Convert.ToInt32(reader["unidades_ocupadas"]);
                        }
                    }

                    if (idEspacio == null)
                    {
                        mensaje = "No hay espacios para eliminar.";
                        return false;
                    }
                    if (ocupadas > 0)
                    {
                        mensaje = "No se puede eliminar " + idEspacio + " porque no está libre.";
                        return false;
                    }

                    using (var cmdDel = new MySqlCommand("DELETE FROM espacios WHERE id_espacio = @id", cn))
                    {
                        cmdDel.Parameters.AddWithValue("@id", idEspacio);
                        cmdDel.ExecuteNonQuery();
                    }
                    mensaje = "Se eliminó el espacio " + idEspacio + ".";
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                mensaje = "No se pudo eliminar el espacio: " + ex.Message;
                return false;
            }
        }
    }
}
