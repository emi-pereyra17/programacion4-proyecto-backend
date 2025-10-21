using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Mappings
{
    public class CarritoDetalleProfile : Profile
    {
        public CarritoDetalleProfile()
        {
            CreateMap<CrearCarritoDetalleDTO, CarritoDetalle>();

            CreateMap<CarritoDetalle, CarritoDetalleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CarritoId, opt => opt.MapFrom(src => src.CarritoId))
                .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.Producto, opt => opt.MapFrom(src => src.Producto));
        }
    }
}
