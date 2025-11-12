using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Mappings
{
    public class CarritoProfile : Profile
    {
        public CarritoProfile()
        {
            CreateMap<CrearCarritoDTO, Carrito>();

            CreateMap<Carrito, CarritoDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.UsuarioId))
                .ForMember(dest => dest.CarritosDetalles, opt => opt.MapFrom(src => src.CarritosDetalles));
        }
    }
}
