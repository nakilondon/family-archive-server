using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using family_archive_server.Repositories;

namespace family_archive_server.Utilities
{
    public static class PersonUtils
    {
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
