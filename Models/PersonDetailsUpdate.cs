﻿using System.Collections.Generic;
using family_archive_server.Repositories;

namespace family_archive_server.Models
{
    public class UpdateDate
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class FamilyUpdateInternal
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public Relationship Relationship { get; set; }
    }

    public class FamilyUpdate
    {
        public int Id { get; set; }
        public string Label { get; set; }
    }

    public class PersonDetailsUpdate
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public string PreferredName { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string NickName { get; set; }
        public UpdateDate Birth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Status { get; set; }
        public UpdateDate Death { get; set; }
        public string PlaceOfDeath { get; set; }
        public string Portrait { get; set; }
        public string Note { get; set; }
        public List<ListPerson> Spouses { get; set; }
        public List<ListPerson> Parents { get; set; }
        public List<ListPerson> Children { get; set; }

    }
}
