namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa la relación entre una categoría y una marca.
    /// </summary>
    public class CategoriaMarcaDTO
    {
        /// <summary>
        /// Identificador único de la relación categoría-marca.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador de la categoría asociada.
        /// </summary>
        public int CategoriaId { get; set; }

        /// <summary>
        /// Identificador de la marca asociada.
        /// </summary>
        public int MarcaId { get; set; }
    }
}