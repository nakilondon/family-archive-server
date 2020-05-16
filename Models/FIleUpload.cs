using Microsoft.AspNetCore.Http;

namespace family_archive_server.Models
{
    public class FIleUpload
    {
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
