using System.Collections.Generic;
using System.Threading.Tasks;
using family_archive_server.Models;

namespace family_archive_server.RepositoriesDb
{
    public interface IImagesDbRepository
    {
        Task<ImageDb> SaveImageToDb(ImageDb imageDb);
        Task RemovePeopleInImage(int imageId);
        Task SavePeopleInImageDb(IEnumerable<int> peopleInImage, int imageId);
        Task<ImageData> GetImageData(string fileName);
        Task<List<ImageDb>> GetImagesForPerson(int personId);
        Task<List<int>> GetPeopleInImage(int imageId);
        Task UpdateImage(ImageDetail imageDetail);
        Task<ImageDetail> GetImageDetail(int imageId);
        Task DeleteImage(int imageId);
    }
}
