using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Mappings
{
    public class PedidoDetalleProfile : Profile
    {
        public PedidoDetalleProfile()
        {
            CreateMap<CrearPedidoDetalleDTO, PedidoDetalle>();

            CreateMap<PedidoDetalle, PedidoDetalleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PedidoId, opt => opt.MapFrom(src => src.PedidoId))
                .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
                .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : null))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio))
                .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal));
        }
    }
}
