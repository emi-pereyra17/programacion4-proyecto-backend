using System.Collections.Generic;

namespace BicTechBack.src.Core.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public ICollection<Producto> Productos { get; set; }
        public ICollection<CategoriaMarca> CategoriasMarcas { get; set; }
    }
}
