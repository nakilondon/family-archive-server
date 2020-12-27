using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using family_archive_server.Models;
using family_archive_server.RepositoriesDb;

namespace family_archive_server.Utilities
{
    public static class PersonUtils
    {
        public static string FindDates(PersonDb personDb)
        {
            string dates = null;

            if (personDb.BirthRangeStart != default || personDb.DeathRangeStart != default)
            {
                dates += " (";
                if (personDb.BirthRangeStart != default)
                {
                    dates += Format.FindDateFromRange(personDb.BirthRangeStart, personDb.BirthRangeEnd);
                }

                if (personDb.DeathRangeStart != default)
                {
                    dates += " - " + Format.FindDateFromRange(personDb.DeathRangeStart, personDb.DeathRangeEnd);
                }

                dates += ")";
            }

            return dates;
        }

        public static ListPerson CreateListPerson(PersonDb personDb)
        {
            return new ListPerson
            {
                Id = personDb.Id,
                Label = personDb.PreferredName + " " + FindDates(personDb)
            };
        }
        public static async Task<List<RelationshipTable>> FindSiblings(PersonDb personDb, IPersonRepository personRepository)
        {
            var siblings = new List<RelationshipTable>();
            foreach (var relationship in personDb.Relationships)
            {
                switch (relationship.Relationship)
                {
                    case Relationship.Father:
                    case Relationship.Mother:
                    case Relationship.Parent:
                        var siblingIds = await personRepository.FindRelationships(relationship.PersonId,
                            relationship.Relationship);
                        foreach (var siblingId in siblingIds)
                        {
                            if (siblingId != personDb.Id && siblings.All(f => f.PersonId != siblingId))
                            {
                                var sibling = await personRepository.FindPerson(siblingId);
                                siblings.Add(new RelationshipTable
                                {
                                    PersonId = siblingId,

                                    Relationship = sibling.Gender == Gender.Male.ToString()
                                        ? Relationship.Brother
                                        : Relationship.Sister
                                });
                            }
                        }
                        break;
                }
            }

            return siblings;
        }
    }
}
