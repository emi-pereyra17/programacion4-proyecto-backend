using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo detalle de pedido.
    /// </summary>
    public class CrearPedidoDetalleDTO
    {
        /// <summary>
        /// Identificador del producto que se agregará al pedido.
        /// </summary>
        [Required(ErrorMessage = "El campo 'ProductoId' es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El 'ProductoId' debe ser un número positivo.")]
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad del producto a agregar al pedido.
        /// </summary>
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto al momento de agregarlo al pedido.
        /// </summary>
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }
    }
}