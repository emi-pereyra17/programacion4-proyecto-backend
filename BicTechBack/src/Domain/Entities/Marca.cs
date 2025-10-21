using System.Collections.Generic;

namespace BicTechBack.src.Core.Entities
{
    public class Marca
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int PaisId { get; set; }
        public Pais Pais { get; set; }

        public ICollection<Producto> Productos { get; set; }
        public ICollection<CategoriaMarca> CategoriasMarcas { get; set; }
    }
}
