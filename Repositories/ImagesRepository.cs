using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using family_archive_server.Models;
using family_archive_server.RepositoriesDb;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Size = System.Drawing.Size;

namespace family_archive_server.Repositories
{
    public class ImageSizing
    {
        public Size Size { get; set; }
        public string Orientation { get; set; }
    }
    public class ImagesRepository : IImagesRepository
    {
        private readonly IMapper _mapper;
        private readonly IImagesDbRepository _imagesDbRepository;
        private readonly IConfiguration _config;

        public ImagesRepository(IMapper mapper, IConfiguration configuration, IImagesDbRepository imagesDbRepository)
        {
            _config = configuration.GetSection("ImageConfig");
            _mapper = mapper;
            _imagesDbRepository = imagesDbRepository;
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

        private ImageSizing GetThumbnailSize(Image<Rgba32> original, IConfiguration config)
        {
            // Width and height.
            int originalWidth = original.Width;
            int originalHeight = original.Height;
            var imageSizing = new ImageSizing();

            // Compute best factor to scale entire image based on larger dimension.
            double factor;
            if (originalWidth > originalHeight)
            {
                factor = config.GetValue<double>("Width") / originalWidth;
                imageSizing.Orientation = "L";
            }
            else
            {
                factor = config.GetValue<double>("Height") / originalHeight;
                imageSizing.Orientation = "P";
            }

            
            imageSizing.Size = new Size((int)(originalWidth * factor), (int)(originalHeight * factor));

            return imageSizing;
        }

        private string ScaleImage(byte[] originalImage, string fileName, IConfiguration configuration)
        {
            var thumbnail = Image.Load(originalImage);
            
            var imageSizing = GetThumbnailSize(thumbnail, configuration);

            thumbnail.Mutate(x => x
                .Resize(imageSizing.Size.Width, imageSizing.Size.Height));
            var thumbnailFileName = Path.Combine(configuration.GetValue<string>("Path"), fileName);

            thumbnail.Save(thumbnailFileName);

            return imageSizing.Orientation;
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
            imageDb.Orientation = ScaleImage(imageData.Image, imageData.FileName, GetSection(ImageType.Thumbnail));
            var savedImageDb = await _imagesDbRepository.SaveImageToDb(imageDb);
            await _imagesDbRepository.SavePeopleInImageDb(imageData.People, savedImageDb.Id);

        }

       
        public async Task<ImageData> GetImage(string fileName, ImageType imageType)
        {
            var imageData = await _imagesDbRepository.GetImageData(fileName);

            if (imageData == null)
            {
                return null;
            }

            string filename = Path.Combine(GetSection(imageType).GetValue<string>("Path"), imageData.FileName);
            imageData.Image = await File.ReadAllBytesAsync(filename);

            return imageData;
        }

        public async Task<List<ImageDb>> GetImagesForPerson(int personId)
            => await _imagesDbRepository.GetImagesForPerson(personId);

        public Task<List<int>> GetPeopleInImage(int imageId) => _imagesDbRepository.GetPeopleInImage(imageId);

        public Task UpdateImage(ImageDetail imageDetail) => _imagesDbRepository.UpdateImage(imageDetail);
        


        public async Task<ImageDetail> GetImageDetail(int imageId)
            => await _imagesDbRepository.GetImageDetail(imageId);

        public async Task DeleteImage(int imageId)
        {
            var imageDetail = await GetImageDetail(imageId);
 
            DeleteFile(imageDetail.FileName, ImageType.Original);
            DeleteFile(imageDetail.FileName, ImageType.Web);
            DeleteFile(imageDetail.FileName, ImageType.Thumbnail);

            await _imagesDbRepository.DeleteImage(imageId);
        }

        private void DeleteFile(string fileName, ImageType imageType)
        {
            var fullFilename = Path.Combine(GetSection(imageType).GetValue<string>("Path"), fileName);


            // Delete a file by using File class static method...
            if (File.Exists(fullFilename))
            {
                // Use a try block to catch IOExceptions, to
                // handle the case of the file already being
                // opened by another process.
                try
                {
                    File.Delete(fullFilename);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

    }
}
