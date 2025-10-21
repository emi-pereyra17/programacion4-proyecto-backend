using System;
using System.Collections.Generic;

namespace BicTechBack.src.Core.Entities
{
    public class Carrito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public DateTime ActualizadoEn { get; set; } = DateTime.Now;
        public ICollection<CarritoDetalle> CarritosDetalles { get; set; }
    }
}
