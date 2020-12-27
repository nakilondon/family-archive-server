using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using family_archive_server.Models;
using family_archive_server.Utilities;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace family_archive_server.RepositoriesDb
{

    public class ImagesDbRepository : IImagesDbRepository
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;
        private readonly string _connectionString;

        public ImagesDbRepository(IPersonRepository personRepository, IMapper mapper, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _personRepository = personRepository;
            _mapper = mapper;
        }

        public async Task<ImageDb> SaveImageToDb(ImageDb imageDb)
        {
            var db = new MySqlConnection(_connectionString);

            imageDb.Description ??= " ";
            imageDb.Location ??= " ";
            imageDb.Orientation ??= " ";
            if (imageDb.Id == 0)
            {
                imageDb.Id =
                    await db.QuerySingleAsync<int>(
                        "SELECT Auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Images'");
            }

            await db.ExecuteAsync(@"
INSERT INTO Images(Id, 
FileName, 
Type, 
Location, 
Description, 
Orientation, 
DateRangeStart, 
DateRangeEnd
) VALUES (
@Id, 
@FileName, 
@Type, 
@Location, 
@Description, 
@Orientation, 
@DateRangeStart, 
@DateRangeEnd)", imageDb);

            return imageDb;
        }

        public async Task RemovePeopleInImage(int imageId)
        {
            var db = new MySqlConnection(_connectionString);
            var people = await GetPeopleInImage(imageId);
            if (people.Count > 0)
            {
                await db.ExecuteAsync("DELETE FROM PeopleInImage WHERE ImageId = @imageId", new { imageId });
            }
        }

        public async Task SavePeopleInImageDb(IEnumerable<int> peopleInImage, int imageId)
        {
            var db = new MySqlConnection(_connectionString);
            
            await RemovePeopleInImage(imageId);

            foreach (var personId in peopleInImage)
            {
                var peopleInImageDb = new PeopleInImageDb {ImageId = imageId, PersonId = personId};
                await db.ExecuteAsync("INSERT INTO PeopleInImage(ImageId, PersonId) VALUES (@ImageId, @PersonId)",
                    peopleInImageDb);
            }
        }

        public async Task<ImageData> GetImageData(string fileName)
        {
            var db = new MySqlConnection(_connectionString);
            var imageDb = await db.QueryFirstOrDefaultAsync<ImageDb>("SELECT * FROM Images WHERE FileName = @FileName;", new { FileName = fileName });

            if (imageDb == null)
            {
                return null;
            }

            return _mapper.Map<ImageData>(imageDb);
        }

        public async Task<List<ImageDb>> GetImagesForPerson(int personId)
        {
                var lookup = new Dictionary<int, ImageDb>();


                var db = new MySqlConnection(_connectionString);
                try
                {

                    await db.QueryAsync<PeopleInImageDb, ImageDb, string>(@"
SELECT pI.*, i.*
FROM PeopleInImage pI
INNER JOIN Images i ON pI.ImageId = i.Id
WHERE pI.PersonId = @Id",
                        (pI, i) =>
                        {
                            if (!lookup.TryGetValue(i.Id, out _))
                            {
                                lookup.Add(i.Id, i);
                            }

                            return i.FileName;
                        }, splitOn: "Id",
                        param: new { @Id = personId });
                }
                catch
                {
                    // ignored
                }

                return lookup.Values.ToList();
        }

        public async Task<List<int>> GetPeopleInImage(int imageId)
        {
            var db = new MySqlConnection(_connectionString);
            var peopleInImage = await db.QueryAsync<int>("SELECT PersonId FROM PeopleInImage WHERE ImageId = @ImageId;", new { ImageId = imageId });

            return peopleInImage?.ToList();
        }
        
        public async Task UpdateImage(ImageDetail imageDetail)
        {
            var imageDb = _mapper.Map<ImageDb>(imageDetail);
            var db = new MySqlConnection(_connectionString);

            await db.ExecuteAsync(@"
UPDATE Images SET
Location = @Location, 
Description = @Description,
DateRangeStart = @DateRangeStart, 
DateRangeEnd = @DateRangeEnd
WHERE Id = @Id", imageDb);

            var people = imageDetail.People.Select((i) => i.Id);
            await SavePeopleInImageDb(people, imageDb.Id);
        }

        public async Task<ImageDetail> GetImageDetail(int imageId)
        {
            var db = new MySqlConnection(_connectionString);
            var imageDb = await db.QueryFirstOrDefaultAsync<ImageDb>("SELECT * FROM Images WHERE Id = @Id;", new { Id = imageId });

            if (imageDb == null)
            {
                return null;
            }

            var imageDetail = _mapper.Map<ImageDetail>(imageDb);

            var people = await GetPeopleInImage(imageId);
            
            imageDetail.People = await GetPeopleDetail(people);

            return imageDetail;
        }

        private async Task<IList<ListPerson>> GetPeopleDetail(IEnumerable<int> people)
        {
            var peopleList = new List<ListPerson>();

            foreach (var personId in people)
            {
                var personDb = await _personRepository.FindPerson(personId);
                peopleList.Add(PersonUtils.CreateListPerson(personDb));
            }

            return peopleList;
        }

        public async Task DeleteImage(int imageId)
        {
            var db = new MySqlConnection(_connectionString);

            await RemovePeopleInImage(imageId);

            try
            {
                await db.ExecuteAsync("DELETE FROM Images WHERE Id = @id", new {id = imageId});
            }
            catch
            {
                // ignored
            }
        }
    }
}
