using System.Collections.Generic;

namespace family_archive_server.Models
{
    public class ImageDetail
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public UpdateDate Date { get; set; }
        public string DisplayDate { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }
        public IList<ListPerson> People {get;set;}
        
    }
}
