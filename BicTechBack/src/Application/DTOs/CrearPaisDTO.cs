using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{

    /// <summary>
    /// DTO para crear un nuevo país.
    /// </summary>
    public class CrearPaisDTO
    {
        /// <summary>
        /// Nombre del país.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string Nombre { get; set; }
    }
}
