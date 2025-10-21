using System.Collections.Generic;

namespace BicTechBack.src.Core.Entities
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public int MarcaId { get; set; }
        public Marca Marca { get; set; }
        public int Stock { get; set; }
        public string ImagenUrl { get; set; }

        public ICollection<CarritoDetalle> CarritosDetalles { get; set; }

        public ICollection<PedidoDetalle> PedidosDetalles { get; set; }
    }
}
