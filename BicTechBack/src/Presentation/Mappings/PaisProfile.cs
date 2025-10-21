using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Mappings
{
    public class PaisProfile : Profile
    {
        public PaisProfile()
        {
            CreateMap<CrearPaisDTO, Pais>();

            CreateMap<Pais, PaisDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre));
        }
    }
}
