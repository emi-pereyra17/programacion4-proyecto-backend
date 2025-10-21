namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa el detalle de un producto dentro de un carrito de compras.
    /// </summary>
    public class CarritoDetalleDTO
    {
        /// <summary>
        /// Identificador único del detalle del carrito.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del carrito al que pertenece este detalle.
        /// </summary>
        public int CarritoId { get; set; }

        /// <summary>
        /// Identificador del producto agregado al carrito.
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto en el carrito.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Producto asociado a este detalle del carrito.
        /// </summary>
        public ProductoDTO Producto { get; set; }
    }
}