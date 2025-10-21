namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO que representa el resultado de un inicio de sesión exitoso.
    /// </summary>
    public class LoginResultDTO
    {
        /// <summary>
        /// Token JWT generado para el usuario autenticado.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token de actualización (refresh token) para renovar la sesión.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Identificador único del usuario autenticado.
        /// </summary>
        public int UsuarioId { get; set; }

        /// <summary>
        /// Nombre del usuario autenticado.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Correo electrónico del usuario autenticado.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Rol del usuario autenticado (e.g., Admin, User).
        /// </summary>
        public string Rol { get; set; }
    }
}