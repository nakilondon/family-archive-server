using System;
using System.Collections.Generic;

namespace family_archive_server.Repositories
{
    public enum Gender
    {
        Male = 0,
        Female = 1,
    }

    public enum Relationship
    {
        Mother = 0,
        Father = 1,
        Parent = 2,
        Wife = 3,
        Husband = 4,
        Spouse = 5,
        Son = 6,
        Daughter = 7,
        Child = 8,
        Sister = 9,
        Brother = 10,
        Sibling = 11
    }

    public class RelationshipTable
    {
        public Relationship Relationship { get; set; }
        public int PersonId { get; set; }
    }

    public class PersonDb
    {
        public int Id { get; set; }
        public string GedcomId { get; set; }
        public string Gender { get; set; }
        public string PreferredName { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string NickName { get; set; }
        public DateTime BirthRangeStart { get; set; }
        public DateTime BirthRangeEnd { get; set; }
        public string PlaceOfBirth { get; set; }
        public bool Dead { get; set; }
        public DateTime DeathRangeStart { get; set; }
        public DateTime DeathRangeEnd { get; set; }
        public string PlaceOfDeath { get; set; }
        public string Note { get; set; }
        public string Portrait { get; set; }
        public IList<RelationshipTable> Relationships { get; set; }
    }

    public class PersonTableDb
    {
        public int Id { get; set; }
        public string GedcomId { get; set; }
        public string Gender { get; set; }
        public string PreferredName { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string NickName { get; set; }
        public DateTime BirthRangeStart { get; set; }
        public DateTime BirthRangeEnd { get; set; }
        public string PlaceOfBirth { get; set; }
        public bool Dead { get; set; }
        public DateTime DeathRangeStart { get; set; }
        public DateTime DeathRangeEnd { get; set; }
        public string PlaceOfDeath { get; set; }
        public string Note { get; set; }
        public string Portrait { get; set; }
    }

    public class RelationshipDb
    {
        public int Person1 { get; set; }
        public string RelationShip { get; set; }
        public int Person2 { get; set; }
    }
}
