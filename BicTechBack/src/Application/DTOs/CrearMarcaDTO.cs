using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para crear una nueva marca.
    /// </summary>
    public class CrearMarcaDTO
    {
        /// <summary>
        /// Nombre de la marca.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string Nombre { get; set; }
        /// <summary>
        /// Identificador de país que se agregará al la marca.
        /// </summary>
        [Required(ErrorMessage = "El campo 'PaisId' es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El 'PaisId' debe ser un número positivo.")]
        public int PaisId { get; set; }
    }
}