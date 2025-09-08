using AutoMapper;
using SmartCamera.WebApiDemo.DTOs;
using SmartCamera.WebApiDemo.Models;

namespace SmartCamera.WebApiDemo.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Camera mappings
            CreateMap<Camera, CameraDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<CreateCameraRequest, Camera>()
                .ForMember(dest => dest.StreamUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Events, opt => opt.Ignore())
                .ForMember(dest => dest.Recordings, opt => opt.Ignore());

            // Event mappings
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.CameraName, opt => opt.MapFrom(src => src.Camera.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()));

            // Recording mappings
            CreateMap<Recording, RecordingDto>()
                .ForMember(dest => dest.CameraName, opt => opt.MapFrom(src => src.Camera.Name))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.FileSizeMB, opt => opt.MapFrom(src => Math.Round(src.FileSizeBytes / (1024.0 * 1024), 2)));

            // User mappings  
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        }
    }
}