namespace family_archive_server.Models
{
    public class ImageDb
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }

    public class PeopleInImageDb
    {
        public int ImageId { get; set; }
        public int PersonId { get; set; }
    }
}
