using Application.Models.ExecutiveCompany;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapper;

public class ExecutiveCompanyMapProfile : Profile
{
    public ExecutiveCompanyMapProfile()
    {
        CreateMap<ExecutiveCompany, ExecutiveCompanyDto>()
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
            .ForMember(dest => dest.Address, opt =>
                opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.LicenseNumber, opt =>
                opt.MapFrom(src => src.LicenseNumber))
            .ForMember(dest => dest.LicenseDateOfIssue, opt =>
                opt.MapFrom(src => src.LicenseDateOfIssue))
            .ForMember(dest => dest.BIN, opt =>
                opt.MapFrom(src => src.BIN))
            .ForMember(dest => dest.CompanyName, opt =>
                opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.TownName, opt =>
                opt.MapFrom(src => src.TownName))
            .ForMember(dest => dest.DirectorSurname, opt =>
                opt.MapFrom(src => src.DirectorSurname))
            .ForMember(dest => dest.DirectorPatronymic, opt =>
                opt.MapFrom(src => src.DirectorPatronymic))
            .ForMember(dest => dest.DirectorName, opt =>
                opt.MapFrom(src => src.DirectorName))
            .ReverseMap();

        CreateMap<ExecutiveCompanyDto, UpdateExecutiveCompanyDto>()
            .ForMember(dest => dest.Id, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Address, opt =>
                opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.LicenseNumber, opt =>
                opt.MapFrom(src => src.LicenseNumber))
            .ForMember(dest => dest.LicenseDateOfIssue, opt =>
                opt.MapFrom(src => src.LicenseDateOfIssue))
            .ForMember(dest => dest.BIN, opt =>
                opt.MapFrom(src => src.BIN))
            .ForMember(dest => dest.CompanyName, opt =>
                opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.TownName, opt =>
                opt.MapFrom(src => src.TownName))
            .ForMember(dest => dest.DirectorSurname, opt =>
                opt.MapFrom(src => src.DirectorSurname))
            .ForMember(dest => dest.DirectorPatronymic, opt =>
                opt.MapFrom(src => src.DirectorPatronymic))
            .ForMember(dest => dest.DirectorName, opt =>
                opt.MapFrom(src => src.DirectorName))
            .ReverseMap();


        CreateMap<ExecutiveCompanyDto, CreateExecutiveCompanyDto>()
            .ForMember(dest => dest.Address, opt =>
                opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.LicenseNumber, opt =>
                opt.MapFrom(src => src.LicenseNumber))
            .ForMember(dest => dest.LicenseDateOfIssue, opt =>
                opt.MapFrom(src => src.LicenseDateOfIssue))
            .ForMember(dest => dest.BIN, opt =>
                opt.MapFrom(src => src.BIN))
            .ForMember(dest => dest.CompanyName, opt =>
                opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.TownName, opt =>
                opt.MapFrom(src => src.TownName))
            .ForMember(dest => dest.DirectorSurname, opt =>
                opt.MapFrom(src => src.DirectorSurname))
            .ForMember(dest => dest.DirectorPatronymic, opt =>
                opt.MapFrom(src => src.DirectorPatronymic))
            .ForMember(dest => dest.DirectorName, opt =>
                opt.MapFrom(src => src.DirectorName))
            .ReverseMap();
    }
}