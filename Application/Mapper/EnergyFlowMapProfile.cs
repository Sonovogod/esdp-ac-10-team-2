using Application.Models.EnergyResult;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapper;

public class EnergyFlowMapProfile : Profile
{
    public EnergyFlowMapProfile()
    {
        CreateMap<EnergyResult, EnergyResultDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Created, opt =>
                opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.LastModified, opt =>
                opt.MapFrom(src => src.LastModified))
            .ForMember(dest => dest.LastModifiedBy, opt =>
                opt.MapFrom(src => src.LastModifiedBy))
            .ForMember(dest => dest.CreatedBy, opt =>
                opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.Distance, opt =>
                opt.MapFrom(src => src.Distance))
            .ForMember(dest => dest.Value, opt =>
                opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.AntennaTranslator, opt =>
                opt.MapFrom(src => src.AntennaTranslator))
            .ForMember(dest => dest.AntennaTranslatorId, opt =>
                opt.MapFrom(src => src.AntennaTranslatorId))
            .ReverseMap();
    }
}