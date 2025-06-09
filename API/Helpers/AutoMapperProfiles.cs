using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {

        CreateMap<AppUser, MemberDto>()
           .ForMember(d => d.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()))
           .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.Photos.FirstOrDefault(x => x.IsMain)!.Url));
        CreateMap<Photo, PhotoDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PhotoTags.Select(pt => pt.Tag)));
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<string, DateOnly>()
            .ConvertUsing(s => DateOnly.Parse(s));
        CreateMap<Message, MessageDto>().ForMember(d => d.SenderPhotoUrl, o => o.MapFrom(s => s.Sender.Photos.FirstOrDefault
        (x => x.IsMain)!.Url));
        CreateMap<Message, MessageDto>().ForMember(d => d.RecipientPhotoUrl, o => o.MapFrom(s => s.Recipient.Photos.FirstOrDefault
        (x => x.IsMain)!.Url));
        CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);

        CreateMap<Photo, PhotoForApprovalDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.AppUser.UserName))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.PhotoTags.Select(pt => pt.Tag!.Name)));
            
        CreateMap<Tag, TagDto>();
        CreateMap<string, TagDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src));
    }
}
