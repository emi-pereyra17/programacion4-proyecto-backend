namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa el detalle de un producto dentro de un pedido.
    /// </summary>
    public class PedidoDetalleDTO
    {
        /// <summary>
        /// Identificador único del detalle del pedido.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del pedido al que pertenece este detalle.
        /// </summary>
        public int PedidoId { get; set; }

        /// <summary>
        /// Identificador del producto incluido en el pedido.
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Información del producto incluido en el pedido.
        /// </summary>
        public ProductoDTO Producto { get; set; }

        /// <summary>
        /// Nombre del producto incluido en el pedido.
        /// </summary>
        public string NombreProducto { get; set; }

        /// <summary>
        /// Cantidad del producto en el pedido.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto en el pedido.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Subtotal correspondiente a este producto en el pedido.
        /// </summary>
        public decimal Subtotal { get; set; }
    }
}