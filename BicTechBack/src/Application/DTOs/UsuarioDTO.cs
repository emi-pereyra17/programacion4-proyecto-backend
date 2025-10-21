namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa un usuario del sistema.
    /// </summary>
    public class UsuarioDTO
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Rol asignado al usuario.
        /// </summary>
        public string Rol { get; set; }
    }
}