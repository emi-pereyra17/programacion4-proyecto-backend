namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa una marca de productos.
    /// </summary>
    public class MarcaDTO
    {
        /// <summary>
        /// Identificador único de la marca.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la marca.
        /// </summary>
        public string Nombre { get; set; }
        /// <summary>
        /// Nombre del país al que pertenece la marca.
        /// </summary>
        public string Pais { get; set; }
    }
}