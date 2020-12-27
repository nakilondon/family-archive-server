using System.IO;
using System.Linq;
using System.Threading.Tasks;
using family_archive_server.Models;
using family_archive_server.Repositories;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace family_archive_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PictureController : ControllerBase
    {
        private readonly IImagesRepository _imagesRepository;

        public PictureController(IImagesRepository imagesRepository)
        {
            _imagesRepository = imagesRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ImageData>> Get(int id, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (fireBaseToken.Claims.ContainsKey("verified") && (bool)fireBaseToken.Claims["verified"])
            {
                var returnValues = await _imagesRepository.GetImageDetail(id);

                return Ok(returnValues);
            }

            return Unauthorized();

        }

 
        [HttpPost("Upload")]
        public async Task<ActionResult> Post([FromForm] RawFIleUpload rawFileUpload)
        {
            byte[] fileBytes;
            var fileUpload = JsonConvert.DeserializeObject<FIleUpload>(rawFileUpload.Details);

            await using (var memoryStream = new MemoryStream())
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
            };

            imageData.People = fileUpload.People.Select(p => p.Id).ToList();

            await _imagesRepository.SaveImage(imageData);

            return Ok("Image saved");
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
        public async Task<IActionResult> UpdateImage([FromBody] ImageDetail imageDetail, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (fireBaseToken.Claims.ContainsKey("edit") && (bool)fireBaseToken.Claims["edit"])
            {
                await _imagesRepository.UpdateImage(imageDetail);
                return Ok($"Update {imageDetail.Id} successful");
            }

            return Unauthorized();
        }



        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (fireBaseToken.Claims.ContainsKey("edit") && (bool) fireBaseToken.Claims["edit"])
            {
                await _imagesRepository.DeleteImage(id);
                return Ok($"Delete {id} successful");
            }

            return Unauthorized();
        }
    }
}