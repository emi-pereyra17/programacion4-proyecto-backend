﻿namespace BicTechBack.src.Core.Entities
{
    public class CarritoDetalle
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public Carrito Carrito { get; set; }
        public int ProductoId { get; set; }
        public Producto Producto { get; set; }
        public int Cantidad { get; set; } = 1;

    }
}
