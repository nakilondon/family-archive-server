using System.Collections.Generic;

namespace family_archive_server.Models
{
    public class Family
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Relationship { get; set; }

    }

    public class PersonDetails
    {
        public int Id { get; set; }
        public string PreferredName { get; set; }
        public string FullName { get; set; }
        public string Birth { get; set; }
        public string Death { get; set; }
        public string Portrait { get; set; }
        public string Note { get; set; }
        public List<Family> Family { get; set; }
    }
}
