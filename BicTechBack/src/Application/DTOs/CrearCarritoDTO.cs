using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo carrito de compras para un usuario.
    /// </summary>
    public class CrearCarritoDTO
    {
        /// <summary>
        /// Identificador del usuario propietario del carrito.
        /// </summary>
        [Required(ErrorMessage = "El campo 'UsuarioId' es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo 'UsuarioId' debe ser un número entero positivo.")]
        public int UsuarioId { get; set; }
    }
}