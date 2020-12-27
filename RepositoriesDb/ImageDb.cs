using System;

namespace family_archive_server.RepositoriesDb
{
    public class ImageDb
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public DateTime DateRangeStart { get; set; }
        public DateTime DateRangeEnd { get; set; }
        public string Orientation { get; set; }
        public string Location { get; set; }
        public string PlaceId { get; set; }
        public string Description { get; set; }
    }

    public class PeopleInImageDb
    {
        public int ImageId { get; set; }
        public int PersonId { get; set; }
    }
}
