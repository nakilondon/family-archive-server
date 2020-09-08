using System;
using System.Collections.Generic;
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
        public async Task<ActionResult<PersonDetails>> Get(int id, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            var securityLevel = Roles.General;
            if (firebaseToken.Claims.ContainsKey("edit") && (bool)firebaseToken.Claims["edit"])
            {
                securityLevel = Roles.Admin;
            }

            var returnValues = await _familyRepository.GetDetails(securityLevel, id);

            return Ok(returnValues);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyTreePerson>>> Get([FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            var securityLevel = Roles.General;
            if (firebaseToken.Claims.ContainsKey("edit") && (bool)firebaseToken.Claims["edit"])
            {
                securityLevel = Roles.Admin;
            }
            var returnValues = await _familyRepository.GetFamilyTree(securityLevel);
            foreach (var person in returnValues)
            {
                Console.WriteLine($"Description: {person.Description}, id: {person.Id}");
            }
            
            return Ok(returnValues);
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<ListPerson>>> GetList([FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            var securityLevel = Roles.General;
            if (firebaseToken.Claims.ContainsKey("edit") && (bool)firebaseToken.Claims["edit"])
            {
                securityLevel = Roles.Admin;
            }
            return Ok(await _familyRepository.GetList(securityLevel));
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Post([FromForm] RawFIleUpload rawFileUpload)
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
        public async  Task<ActionResult<PersonDetails>> UpdatePerson([FromBody] PersonDetailsUpdate personDetails, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (firebaseToken.Claims.ContainsKey("edit") && (bool)firebaseToken.Claims["edit"])
            {
                await _familyRepository.UpdatePerson(personDetails);
                return Ok(await _familyRepository.GetDetails(Roles.Admin, personDetails.Id));
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<ActionResult<PersonDetails>> AddPerson([FromBody] PersonDetailsUpdate personDetails, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (firebaseToken.Claims.ContainsKey("edit") && (bool)firebaseToken.Claims["edit"])
            {
                var personId = await _familyRepository.AddPerson(personDetails);
                return Ok(await _familyRepository.GetDetails(Roles.Admin, personId));
            }

            return Unauthorized();

        }

        [HttpGet("update/{id}")]
        public async Task<ActionResult<PersonDetailsUpdate>> GetUpdate(int id, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            try
            {
                var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
                var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

                if (firebaseToken.Claims.ContainsKey("edit") && (bool) firebaseToken.Claims["edit"])
                {
                    var returnValues = await _familyRepository.GetDetailsForUpdate(id);
                    return Ok(returnValues);
                }
            }
            catch
            {
                return Unauthorized();
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

            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (firebaseToken.Claims.ContainsKey("edit") && (bool) firebaseToken.Claims["edit"])
            {
                await _familyRepository.DeletePerson(id);
                return Ok($"Delete {id} successful");
            }

            return Unauthorized();
        }
    }
}