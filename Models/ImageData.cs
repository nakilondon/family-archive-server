using System.Collections.Generic;

namespace family_archive_server.Models
{
    public class ImageData
    {
        public string FileName { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public IList<int> People {get;set;}
        public byte[] Image { get; set; }
    }
}
