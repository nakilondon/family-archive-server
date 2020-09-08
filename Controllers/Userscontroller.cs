using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using family_archive_server.Models;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace family_archive_server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyToken(TokenVerifyRequest request)
        {
            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;

            try
            {
                var response = await auth.VerifyIdTokenAsync(request.Token);
                if (response != null)
                    return Accepted();
            }
            catch (FirebaseException ex)
            {
                return BadRequest();
            }

            return BadRequest();
        }

        [HttpGet("secrets")]
        [Authorize]
        public IEnumerable<string> GetSecrets()
        {
            return new List<string>()
            {
                "This is from the secret controller",
                "Seeing this means you are authenticated",
                "You have logged in using your google account from firebase",
                "Have a nice day!!"
            };
        }


        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<UserDetails>>> GetList([FromHeader] string authorization)
        {
            var userRecords = new List<UserDetails>();
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            var securityLevel = Roles.General;
            if (firebaseToken.Claims.ContainsKey("admin") && (bool) firebaseToken.Claims["admin"])
            {
                securityLevel = Roles.Admin;
            }

            var pagedEnumerable = FirebaseAuth.DefaultInstance.ListUsersAsync(null);
            var responses = pagedEnumerable.AsRawResponses().GetEnumerator();
            while (await responses.MoveNext())
            {
                ExportedUserRecords response = responses.Current;

                foreach (ExportedUserRecord user in response.Users)
                {
                    userRecords.Add(new UserDetails
                    {
                        Uid = user.Uid,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        Admin = user?.CustomClaims != null && user.CustomClaims.ContainsKey("admin") &&
                                (bool) user?.CustomClaims["admin"],
                        Edit = user?.CustomClaims != null && user.CustomClaims.ContainsKey("edit") &&
                               (bool) user?.CustomClaims["edit"],
                        Verified = user?.CustomClaims != null && user.CustomClaims.ContainsKey("verified") &&
                                   (bool) user?.CustomClaims["verified"],
                    });
                }
            }

            return Ok(userRecords);

        }


        [HttpDelete("{uid}")]
        public async Task<ActionResult> Delete(string uid, [FromHeader] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (firebaseToken.Claims.ContainsKey("admin") && (bool) firebaseToken.Claims["admin"])
            {
                await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
                return Ok($"Delete {uid} user successful");
            }

            return Unauthorized();
        }

        [HttpPut()]
        public async Task<ActionResult<PersonDetailsUpdate>> Update([FromHeader] string authorization, [FromBody] UserDetails updateDetails)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return Unauthorized();
            }

            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            var firebaseToken = await auth.VerifyIdTokenAsync(authorization);

            if (firebaseToken.Claims.ContainsKey("admin") && (bool) firebaseToken.Claims["admin"])
            {
                var claims = new Dictionary<string, object>()
                {
                    { "admin", updateDetails.Admin },
                    { "edit", updateDetails.Edit },
                    { "verified", updateDetails.Verified }
                };
                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(updateDetails.Uid, claims);
                return Ok($"Update {updateDetails.Uid} user successful");
            }

            return Unauthorized();
        }
    }
}
