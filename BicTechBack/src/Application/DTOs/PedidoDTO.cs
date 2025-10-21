namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa un pedido realizado por un usuario.
    /// </summary>
    public class PedidoDTO
    {
        /// <summary>
        /// Identificador único del pedido.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del usuario que realizó el pedido.
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Fecha y hora en que se realizó el pedido.
        /// </summary>
        public DateTime FechaPedido { get; set; }

        /// <summary>
        /// Monto total del pedido.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Estado actual del pedido (por ejemplo: Pendiente, Enviado, Entregado).
        /// </summary>
        public string Estado { get; set; }

        /// <summary>
        /// Dirección de envío asociada al pedido.
        /// </summary>
        public string DireccionEnvio { get; set; }

        /// <summary>
        /// Lista de productos y sus detalles incluidos en el pedido.
        /// </summary>
        public List<PedidoDetalleDTO> Productos { get; set; }
    }
}