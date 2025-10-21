using System.Collections.Generic;

namespace BicTechBack.src.Core.Entities
{
    public class Pais
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public ICollection<Marca> Marcas { get; set; }
    }
}
