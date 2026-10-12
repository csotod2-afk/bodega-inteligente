namespace BodegaInteligente
{
    /// <summary>
    /// Un renglón de la vista vista_estado_espacios (db/bodega_db.sql):
    /// cuánto ocupa cada espacio y su estado para el mapa (LIBRE, PARCIAL, LLENO).
    /// </summary>
    public class EspacioEstado
    {
        public string IdEspacio { get; set; }
        public string Zona { get; set; }
        public bool SoportaFragil { get; set; }
        public int CapacidadTotal { get; set; }
        public int UnidadesOcupadas { get; set; }
        public string Estado { get; set; }
    }
}
