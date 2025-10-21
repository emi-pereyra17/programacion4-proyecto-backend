namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa una categoría de productos.
    /// </summary>
    public class CategoriaDTO
    {
        /// <summary>
        /// Identificador único de la categoría.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la categoría.
        /// </summary>
        public string Nombre { get; set; }
    }
}