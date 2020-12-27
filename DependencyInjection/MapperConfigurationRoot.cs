using AutoMapper;
using family_archive_server.Models;
using family_archive_server.RepositoriesDb;
using family_archive_server.Utilities;
using LightInject;

namespace family_archive_server.DependencyInjection
{
    public class MapperConfigurationRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {

            IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ImageData, ImageDb>()
                    .ForMember(dest => dest.Location,
                        opt => opt.MapFrom(src => src.Location.Description))
                    .ForMember(dest => dest.PlaceId,
                        opt => opt.MapFrom(src => src.Location.PlaceId))
                    .ForMember(dest => dest.DateRangeStart,
                        opt => opt.MapFrom(src => Format.FindStartDateFromUpdateDate(src.Date)))
                    .ForMember(dest => dest.DateRangeEnd,
                        opt => opt.MapFrom(src => Format.FindEndDateFromUpdateDate(src.Date)));
                   

                cfg.CreateMap<ImageDb, ImageData>()
                    .ForMember(dest => dest.Date,
                        opt => opt.MapFrom(src => Format.FindUpdateDate(src.DateRangeStart, src.DateRangeEnd)))
                    .ForMember(dest => dest.Location, 
                        opt => opt.MapFrom(src => new Location { Description = src.Location, PlaceId = src.PlaceId }));

                cfg.CreateMap<ImageDetail, ImageDb>()
                    .ForMember(dest => dest.DateRangeStart,
                        opt => opt.MapFrom(src => Format.FindStartDateFromUpdateDate(src.Date)))
                    .ForMember(dest => dest.DateRangeEnd,
                        opt => opt.MapFrom(src => Format.FindEndDateFromUpdateDate(src.Date)));

                cfg.CreateMap<ImageDb, ImageDetail>()
                    .ForMember(dest => dest.Date,
                        opt => opt.MapFrom(src => Format.FindUpdateDate(src.DateRangeStart, src.DateRangeEnd)))
                    .ForMember(dest => dest.DisplayDate,
                        opt => opt.MapFrom(src => Format.FindDateFromRange(src.DateRangeStart, src.DateRangeEnd)))
                    .ForMember(dest => dest.Location,
                        opt => opt.MapFrom(src => new Location { Description = src.Location, PlaceId = src.PlaceId }));

                cfg.CreateMap<ImageDetail, ImageData>();

                cfg.CreateMap<ImageData, ImageDetail>();

                cfg.CreateMap<PersonDb, PersonDetails>()
                    .ForMember(dest => dest.Gender,
                        opt => opt.MapFrom(src => src.Gender.ToString()));

                cfg.CreateMap<PersonTableDb, PersonDb>()
                    .ForMember(dest => dest.Gender,
                        opt => opt.MapFrom(src => src.Gender == "Male" ? Gender.Male : Gender.Female ));

                cfg.CreateMap<PersonDb, PersonDetails>()
                    .ForMember(dest => dest.Birth,
                        opt => opt.MapFrom(src => Format.FindDateFromRange(src.BirthRangeStart, src.BirthRangeEnd)))
                    .ForMember(dest => dest.Death,
                        opt => opt.MapFrom(src => Format.FindDateFromRange(src.DeathRangeStart, src.DeathRangeEnd)));

                cfg.CreateMap<PersonDetails, PersonDb>();

                cfg.CreateMap<PersonDb, PersonDetailsUpdate>()
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Dead ? "Dead" : "Living"))
                    .ForMember(dest => dest.Birth,
                        opt => opt.MapFrom(src => Format.FindUpdateDate(src.BirthRangeStart, src.BirthRangeEnd)))
                    .ForMember(dest => dest.Death,
                        opt => opt.MapFrom(src => Format.FindUpdateDate(src.DeathRangeStart, src.DeathRangeEnd)));
                
                cfg.CreateMap<PersonDetailsUpdate, PersonDb>()
                    .ForMember(dest => dest.Dead, opt => opt.MapFrom(src => src.Status == "Dead"))
                    .ForMember(dest => dest.BirthRangeStart,
                        opt => opt.MapFrom(src => Format.FindStartDateFromUpdateDate(src.Birth)))
                    .ForMember(dest => dest.BirthRangeEnd,
                        opt => opt.MapFrom(src => Format.FindEndDateFromUpdateDate(src.Birth)))
                    .ForMember(dest => dest.DeathRangeStart,
                        opt => opt.MapFrom(src => Format.FindStartDateFromUpdateDate(src.Death)))
                    .ForMember(dest => dest.DeathRangeEnd,
                        opt => opt.MapFrom(src => Format.FindEndDateFromUpdateDate(src.Death)));
            }));

            serviceRegistry.RegisterInstance(mapper);
        }
    }
}
