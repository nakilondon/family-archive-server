using AutoMapper;
using family_archive_server.Models;
using family_archive_server.Repositories;
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
                cfg.CreateMap<ImageData, ImageDb>();
                cfg.CreateMap<ImageDb, ImageData>();
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


            }));

            serviceRegistry.RegisterInstance(mapper);
        }
    }
}
