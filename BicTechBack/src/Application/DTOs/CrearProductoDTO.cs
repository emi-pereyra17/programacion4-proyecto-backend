using System.ComponentModel.DataAnnotations;

namespace BicTechBack.src.Core.DTOs
{
    /// <summary>
    /// DTO para crear un nuevo producto.
    /// </summary>
    public class CrearProductoDTO
    {
        /// <summary>
        /// Nombre del producto.
        /// </summary>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres")]
        public string Nombre { get; set; }

        /// <summary>
        /// Precio del producto.
        /// </summary>
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal Precio { get; set; }

        /// <summary>
        /// Descripción del producto.
        /// </summary>
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        public string Descripcion { get; set; }

        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto.
        /// </summary>
        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        /// <summary>
        /// Identificador de la marca a la que pertenece el producto.
        /// </summary>
        [Required(ErrorMessage = "La marca es obligatoria")]
        public int MarcaId { get; set; }

        /// <summary>
        /// Stock disponible del producto.
        /// </summary>
        [Required(ErrorMessage = "El stock es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        /// <summary>
        /// URL de la imagen del producto.
        /// </summary>
        [Required(ErrorMessage = "La URL de la imagen es obligatoria")]
        [Url(ErrorMessage = "La URL de la imagen no es válida")]
        public string ImagenUrl { get; set; }
    }
}
