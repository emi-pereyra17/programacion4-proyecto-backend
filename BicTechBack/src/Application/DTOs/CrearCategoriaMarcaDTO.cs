using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para crear una relación entre una categoría y una marca.
    /// </summary>
    public class CrearCategoriaMarcaDTO
    {
        /// <summary>
        /// Identificador de la categoría asociada.
        /// </summary>
        [Required(ErrorMessage = "El ID de categoria es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de categoria debe ser un número positivo")]
        public int CategoriaId { get; set; }

        /// <summary>
        /// Identificador de la marca asociada.
        /// </summary>
        [Required(ErrorMessage = "El ID de marca es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de marca debe ser un número positivo")]
        public int MarcaId { get; set; }
    }
}