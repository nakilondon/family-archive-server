﻿using System.Threading.Tasks;
using family_archive_server.Models;

namespace family_archive_server.Repositories
{
    public interface IImagesRepository
    {
        Task SaveImage(ImageData imageData);
        Task<ImageData> GetImage(string fileName, ImageType imageType);
    }
}
