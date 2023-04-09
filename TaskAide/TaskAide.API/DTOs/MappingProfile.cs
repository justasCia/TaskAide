using AutoMapper;
using NetTopologySuite.Geometries;
using TaskAide.API.DTOs.Auth;
using TaskAide.API.DTOs.Bookings;
using TaskAide.API.DTOs.Geometry;
using TaskAide.API.DTOs.Reviews;
using TaskAide.API.DTOs.Services;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.API.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<Point, PointDto>()
                .ForMember(d => d.PlaceId, opt => opt.Ignore());

            //services
            CreateMap<Service, ServiceDto>();
            CreateMap<ProviderService, ServiceDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(p => p.ServiceId))
                .ForMember(d => d.Name, opt => opt.MapFrom(p => p.Service.Name));

            CreateMap<ServiceDto, Service>();

            //providers
            CreateMap<Provider, UserDto>()
                .ForMember(d => d.FirstName, opt => opt.MapFrom(p => p.User.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(p => p.User.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(p => p.User.Email))
                .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(p => p.User.PhoneNumber));
            CreateMap<Provider, ProviderDto>()
                .ForMember(d => d.FirstName, opt => opt.MapFrom(p => p.User.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(p => p.User.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(p => p.User.Email))
                .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(p => p.User.PhoneNumber))
                .ForMember(d => d.CompanyName, opt => opt.MapFrom(p => p.User.CompanyName))
                .ForMember(d => d.Reviews, opt => opt.MapFrom(p => p.Bookings.Select(b => b.Review).Where(r => r != null)));
            CreateMap<Provider, ProviderWithInformationDto>()
                .ForMember(d => d.FirstName, opt => opt.MapFrom(p => p.User.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(p => p.User.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(p => p.User.Email))
                .ForMember(d => d.Location, opt => opt.MapFrom(p => p.Location))
                .ForMember(d => d.Location, opt => opt.MapFrom(p => new PointDto() { X = p.Location.X, Y = p.Location.Y, PlaceId = p.PlaceId }))
                .ForMember(d => d.CompanyName, opt => opt.MapFrom(p => p.User.CompanyName));

            CreateMap<ProviderInformationDto, Provider>()
                .ForMember(d => d.Location, opt => opt.MapFrom(p => new Point(p.Location.X, p.Location.Y) { SRID = 4326 }))
                .ForMember(d => d.PlaceId, opt => opt.MapFrom(p => p.Location.PlaceId))
                .ForMember(d => d.ProviderServices, opt => opt.Ignore());

            //bookings
            CreateMap<BookingRequestDto, Booking>()
                .ForMember(d => d.Address, opt => opt.MapFrom(b => new Point(b.Address.X, b.Address.Y) { SRID = 4326 }))
                .ForMember(d => d.Services, opt => opt.MapFrom(b => b.Services.Select(bs => new BookingService() { ServiceId = bs.Service.Id })))
                .ForMember(d => d.PlaceId, opt => opt.MapFrom(b => b.Address.PlaceId));
            CreateMap<PostBookingDto, Booking>()
                .ForMember(d => d.Address, opt => opt.MapFrom(b => new Point(b.Address.X, b.Address.Y) { SRID = 4326 }))
                .ForMember(d => d.Services, opt => opt.MapFrom(b => b.Services.Select(bs => new BookingService() { ServiceId = bs.Service.Id })))
                .ForMember(d => d.PlaceId, opt => opt.MapFrom(b => b.Address.PlaceId));
            CreateMap<BookingServiceDto, BookingService>()
                .ForMember(d => d.Service, opt => opt.Ignore())
                .ForMember(d => d.Id, opt => opt.Ignore());
            CreateMap<BookingMaterialPriceDto, BookingMaterialPrice>();
            CreateMap<ReviewDto, Review>();

            CreateMap<Review, ReviewDto>();
            CreateMap<BookingService, ServiceDto>()
                .ForMember(b => b.Name, opt => opt.MapFrom(b => b.Service.Name));
            CreateMap<BookingService, BookingServiceDto>()
                .ForMember(b => b.Service, opt => opt.MapFrom(b => b.Service));
            CreateMap<BookingMaterialPrice, BookingMaterialPriceDto>();
            CreateMap<Booking, BookingDto>()
                .ForMember(d => d.Client, opt => opt.MapFrom(b => b.User))
                .ForMember(d => d.Services, opt => opt.MapFrom(b => b.Services))
                .ForMember(d => d.MaterialPrices, opt => opt.MapFrom(b => b.MaterialPrices))
                .ForMember(d => d.Status, opt => opt.MapFrom(b => b.Status.ToString()))
                .ForMember(d => d.Address, opt => opt.MapFrom(b => new PointDto() { X = b.Address.X, Y = b.Address.Y, PlaceId = b.PlaceId }));
        }
    }
}
