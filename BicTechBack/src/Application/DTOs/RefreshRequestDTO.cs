namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para solicitar la renovación de un token de autenticación.
    /// </summary>
    public class RefreshRequestDTO
    {
        /// <summary>
        /// Token JWT actual que se desea renovar.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Token de actualización (refresh token) asociado al usuario.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}