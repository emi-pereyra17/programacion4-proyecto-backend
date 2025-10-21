using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo detalle en un carrito de compras.
    /// </summary>
    public class CrearCarritoDetalleDTO
    {
        /// <summary>
        /// Identificador del carrito al que se agregará el producto.
        /// </summary>
        [Required(ErrorMessage = "El campo CarritoId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El CarritoId debe ser un número positivo.")]
        public int CarritoId { get; set; }

        /// <summary>
        /// Identificador del producto que se agregará al carrito.
        /// </summary>
        [Required(ErrorMessage = "El campo ProductoId es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ProductoId debe ser un número positivo.")]
        public int ProductoId { get; set; }
    }
}