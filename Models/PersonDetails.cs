using System.Collections.Generic;

namespace family_archive_server.Models
{
    public class Family
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Relationship { get; set; }

    }

    public class ImageDetails
    {
        public string FileName { get; set; }
        public string Caption { get; set; }
    }

    public class PersonDetails
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public string PreferredName { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string NickName { get; set; }
        public string Birth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Death { get; set; }
        public string PlaceOfDeath { get; set; }
        public string Portrait { get; set; }
        public string Note { get; set; }
        public List<Family> Family { get; set; }
        public List<ImageDetails> Images { get; set; }

    }
}
