using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using family_archive_server.Models;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Size = System.Drawing.Size;

namespace family_archive_server.Repositories
{
    public class ImagesRepository : IImagesRepository
    {
        private readonly IMapper _mapper;
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public ImagesRepository(IMapper mapper, IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _config = configuration.GetSection("ImageConfig");
            _mapper = mapper;
        }

        private IConfiguration GetSection(ImageType imageType)
        {
            return imageType switch
            {
                ImageType.Original => _config.GetSection("Original"),
                ImageType.Web => _config.GetSection("Web"),
                _ => _config.GetSection("Thumbnail")
            };
        }

        private  Size GetThumbnailSize(Image<Rgba32> original, IConfiguration config)
        {
            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = config.GetValue<double>("Width") / originalWidth;
            }
            else
            {
                factor = config.GetValue<double>("Height") / originalHeight;
            }

            // Return thumbnail size.
            return new Size((int)(originalWidth * factor), (int)(originalHeight * factor));
        }

        private void ScaleImage(byte[] originalImage, string fileName, IConfiguration configuration)
        {
            var thumbnail = Image.Load(originalImage);
            
            var size = GetThumbnailSize(thumbnail, configuration);

            thumbnail.Mutate(x => x
                .Resize(size.Width, size.Height));
            var thumbnailFileName = Path.Combine(configuration.GetValue<string>("Path"), fileName);

            thumbnail.Save(thumbnailFileName);
        }

        public async Task SaveImage(ImageData imageData)
        {
            var imageDb = _mapper.Map<ImageDb>(imageData);
            var originalFilename = Path.Combine(GetSection(ImageType.Original).GetValue<string>("Path"), imageData.FileName);

            using (FileStream sourceStream = File.Open(originalFilename, FileMode.OpenOrCreate))
            {
                sourceStream.Seek(0, SeekOrigin.End);
                await sourceStream.WriteAsync(imageData.Image, 0, imageData.Image.Length);
            }

            //var originalImage = Image.FromStream(new MemoryStream(imageData.Image));
            ScaleImage(imageData.Image, imageData.FileName, GetSection(ImageType.Web));
            ScaleImage(imageData.Image, imageData.FileName, GetSection(ImageType.Thumbnail));

            var db = new MySqlConnection(_connectionString);

            imageDb.Description ??= " ";
            imageDb.Location ??= " ";
            imageDb.Id = await db.QuerySingleAsync<int>("SELECT Auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Images'");

            await db.ExecuteAsync("INSERT INTO Images(Id, FileName, Type, Location, Description) VALUES (@Id, @FileName, @Type, @Location, @Description)", imageDb);

            foreach (var personId in imageData.People)
            {
                var peopleInImageDb = new PeopleInImageDb { ImageId = imageDb.Id, PersonId = personId };
                await db.ExecuteAsync("INSERT INTO PeopleInImage(ImageId, PersonId) VALUES (@ImageId, @PersonId)", peopleInImageDb);
            }

        }
        
        public async Task<ImageData> GetImage(string fileName, ImageType imageType)
        {
            var db = new MySqlConnection(_connectionString);
            var imageDb = await db.QueryFirstOrDefaultAsync<ImageData>("SELECT * FROM Images WHERE FileName = @FileName;", new { FileName = fileName });

            if (imageDb == null)
            {
                return null;
            }

            var imageData = _mapper.Map<ImageData>(imageDb);

            string filename = Path.Combine(GetSection(imageType).GetValue<string>("Path"), imageData.FileName);
            imageData.Image = await File.ReadAllBytesAsync(filename);

            return imageData;
        }

        public async Task<List<ImageDb>> GetImagesForPerson(int personId)
        {
                var lookup = new Dictionary<int, ImageDb>();


                var db = new MySqlConnection(_connectionString);
                try
                {

                    var result = await db.QueryAsync<PeopleInImageDb, ImageDb, string>(@"
SELECT pI.*, i.*
FROM PeopleInImage pI
INNER JOIN Images i ON pI.ImageId = i.Id
WHERE pI.PersonId = @Id",
                        (pI, i) =>
                        {
                            ImageDb imageDb;
                            if (!lookup.TryGetValue(i.Id, out imageDb))
                            {
                                lookup.Add(i.Id, i);
                            }

                            return i.FileName;
                        }, splitOn: "Id",
                        param: new { @Id = personId });
                }
                catch (Exception e)
                { }

                return lookup.Values.ToList();

            
        }

        public async Task<List<int>> GetPeopleInImage(int imageId)
        {
            var db = new MySqlConnection(_connectionString);
            var peopleInImage = await db.QueryAsync<int>("SELECT PersonId FROM PeopleInImage WHERE ImageId = @ImageId;", new { ImageId = imageId });

            if (peopleInImage == null)
            {
                return null;
            }

            return peopleInImage.ToList();
        }
    }
}
