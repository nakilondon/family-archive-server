using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using family_archive_server.Models;
using family_archive_server.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace family_archive_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FamilyTreeController : ControllerBase
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly IImagesRepository _imagesRepository;

        public FamilyTreeController(IFamilyRepository familyRepository,
            IImagesRepository imagesRepository)
        {
            _familyRepository = familyRepository;
            _imagesRepository = imagesRepository;
        }

        [HttpGet("{id}")]
        public async Task<PersonDetails> Get(int id)
        {
            var returnValues = await _familyRepository.GetDetails(id);

            return returnValues;
        }

        [HttpGet]
        public async Task<IEnumerable<FamilyTreePerson>> Get()
        {
            var returnValues = await _familyRepository.GetFamilyTree();
            return returnValues;
        }

        [HttpGet("list")]
        public async Task<IEnumerable<ListPerson>> GetList()
        {
            return await _familyRepository.GetList();
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Post([FromForm] FIleUpload fileUpload)
        {
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await fileUpload.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var imageData = new ImageData
            {
                FileName = fileUpload.File.FileName,
                Type = fileUpload.File.ContentType,
                Description = fileUpload.Description,
                Location = "",
                Image = fileBytes
            };

            await _imagesRepository.SaveImage(imageData);

            return Ok();
        }

        [HttpGet("img/{fileName}")]
        public async Task<IActionResult> GetImg(string fileName)
        {
            var imageData = await _imagesRepository.GetImage(fileName, ImageType.Web);

            return File(imageData.Image, imageData.Type);
        }

        [HttpGet("thumbnail/{fileName}")]
        public async Task<IActionResult> GetThumbnail(string fileName)
        {
            var imageData = await _imagesRepository.GetImage(fileName, ImageType.Thumbnail);

            return File(imageData.Image, imageData.Type);

        }

        [HttpPut("update")]
        public async  Task<PersonDetails> UpdatePerson([FromBody] PersonDetailsUpdate personDetails)
        {
            await _familyRepository.UpdatePerson(personDetails);
            return await _familyRepository.GetDetails(personDetails.Id);
        }

        [HttpGet("update/{id}")]
        public async Task<PersonDetailsUpdate> GetUpdate(int id)
        {
            var returnValues = await _familyRepository.GetDetailsForUpdate(id);

            return returnValues;
        }
    }
}