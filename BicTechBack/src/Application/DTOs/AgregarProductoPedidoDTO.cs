namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para agregar un producto a un pedido existente.
    /// </summary>
    public class AgregarProductoPedidoDTO
    {
        /// <summary>
        /// Identificador del pedido al que se agregará el producto.
        /// </summary>
        public int PedidoId { get; set; }

        /// <summary>
        /// Identificador del producto que se agregará al pedido.
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto a agregar al pedido.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto al momento de agregarlo.
        /// </summary>
        public decimal Precio { get; set; }
    }
}