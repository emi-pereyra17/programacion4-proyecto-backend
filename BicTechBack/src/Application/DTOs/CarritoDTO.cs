namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa un carrito de compras de un usuario.
    /// </summary>
    public class CarritoDTO
    {
        /// <summary>
        /// Identificador único del carrito.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador del usuario propietario del carrito.
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Lista de productos y sus detalles contenidos en el carrito.
        /// </summary>
        public List<CarritoDetalleDTO> Productos { get; set; } = new();
    }
}