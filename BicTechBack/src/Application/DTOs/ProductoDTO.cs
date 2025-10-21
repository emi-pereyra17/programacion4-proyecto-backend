namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa un producto disponible en el sistema.
    /// </summary>
    public class ProductoDTO
    {
        /// <summary>
        /// Identificador único del producto.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del producto.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Precio del producto.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Descripción del producto.
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Id de la categoría a la que pertenece el producto.
        /// </summary>
        public string CategoriaId { get; set; }

        /// <summary>
        /// Id de la marca a la que pertenece el producto.
        /// </summary>
        public string MarcaId { get; set; }

        /// <summary>
        /// Stock disponible del producto.
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Url de la imagen del producto.
        /// </summary>
        public string ImagenUrl { get; set; }
    }
}