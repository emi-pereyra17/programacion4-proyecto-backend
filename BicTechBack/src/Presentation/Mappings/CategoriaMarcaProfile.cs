using AutoMapper;
using BicTechBack.src.Core.DTOs;
using BicTechBack.src.Core.Entities;

namespace BicTechBack.src.Core.Mappings
{
    public class CategoriaMarcaProfile : Profile
    {
        public CategoriaMarcaProfile()
        {
            CreateMap<CrearCategoriaMarcaDTO, CategoriaMarca>();

            CreateMap<CategoriaMarca, CategoriaMarcaDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoriaId, opt => opt.MapFrom(src => src.CategoriaId))
                .ForMember(dest => dest.MarcaId, opt => opt.MapFrom(src => src.MarcaId));
        }
    }
}
