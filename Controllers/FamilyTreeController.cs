using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using family_archive_server.Models;
using family_archive_server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        public async Task<IActionResult> Post([FromForm] RawFIleUpload rawFileUpload)
        {
            byte[] fileBytes;
            var fileUpload = JsonConvert.DeserializeObject<FIleUpload>(rawFileUpload.Details);
            //var date = JsonConvert.DeserializeObject<UpdateDate>(fileUpload.Date);

            using (var memoryStream = new MemoryStream())
            {
                await rawFileUpload.File.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            if (await _imagesRepository.GetImage(fileUpload.Name, ImageType.Thumbnail) != null)
            {
                return BadRequest($"{rawFileUpload.File.FileName} already exists");
            }

            var imageData = new ImageData
            {
                FileName = fileUpload.Name,
                Type = rawFileUpload.File.ContentType,
                Description = fileUpload.Description,
                Location = fileUpload.Location,
                Image = fileBytes
               // People = new List<int>()
            };

            imageData.People = fileUpload.People.Select(p => p.Id).ToList();

            await _imagesRepository.SaveImage(imageData);

            return Ok();
        }

        [HttpGet("img/{fileName}")]
        public async Task<IActionResult> GetImg(string fileName)
        {
            var imageData = await _imagesRepository.GetImage(fileName, ImageType.Web);

            return File(imageData.Image, imageData.Type);
        }

        [HttpGet("original/{fileName}")]
        public async Task<IActionResult> GetOriginal(string fileName)
        {
            var imageData = await _imagesRepository.GetImage(fileName, ImageType.Original);

            return File(imageData.Image, imageData.Type);
        }

        [HttpGet("thumbnail/{fileName}")]
        public async Task<IActionResult> GetThumbnail(string fileName)
        {
            var imageData = await _imagesRepository.GetImage(fileName, ImageType.Thumbnail);

            return File(imageData.Image, imageData.Type);

        }

        [HttpPut]
        public async  Task<PersonDetails> UpdatePerson([FromBody] PersonDetailsUpdate personDetails)
        {
            await _familyRepository.UpdatePerson(personDetails);
            return await _familyRepository.GetDetails(personDetails.Id);
        }

        [HttpPost]
        public async Task<PersonDetails> AddPerson([FromBody] PersonDetailsUpdate personDetails)
        {
            var personId = await _familyRepository.AddPerson(personDetails);
            return await _familyRepository.GetDetails(personId);
        }

        [HttpGet("update/{id}")]
        public async Task<PersonDetailsUpdate> GetUpdate(int id)
        {
            var returnValues = await _familyRepository.GetDetailsForUpdate(id);
            return returnValues;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _familyRepository.DeletePerson(id);
            return Ok($"Delete {id} successful");
        }
    }
}