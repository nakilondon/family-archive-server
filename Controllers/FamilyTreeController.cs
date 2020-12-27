using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using family_archive_server.Models;
using family_archive_server.Repositories;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;

namespace family_archive_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FamilyTreeController : ControllerBase
    {
        private readonly IFamilyRepository _familyRepository;
        

        public FamilyTreeController(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
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

        [HttpPut]
        public async  Task<ActionResult<PersonDetails>> UpdatePerson([FromBody] PersonDetailsUpdate personDetails, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (fireBaseToken.Claims.ContainsKey("edit") && (bool)fireBaseToken.Claims["edit"])
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

            var auth = FirebaseAuth.DefaultInstance;
            var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (fireBaseToken.Claims.ContainsKey("edit") && (bool)fireBaseToken.Claims["edit"])
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
                var auth = FirebaseAuth.DefaultInstance;
                var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

                if (fireBaseToken.Claims.ContainsKey("edit") && (bool) fireBaseToken.Claims["edit"])
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

            var auth = FirebaseAuth.DefaultInstance;
            var fireBaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (fireBaseToken.Claims.ContainsKey("edit") && (bool) fireBaseToken.Claims["edit"])
            {
                await _familyRepository.DeletePerson(id);
                return Ok($"Delete {id} successful");
            }

            return Unauthorized();
        }
    }
}