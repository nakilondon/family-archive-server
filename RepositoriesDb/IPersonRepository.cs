using System.Collections.Generic;
using System.Threading.Tasks;

namespace family_archive_server.RepositoriesDb
{
    public interface IPersonRepository
    {
        Task<int> AddPerson(PersonDb personDb);
        Task UpdatePerson(PersonDb personDb);
        Task<PersonDb> FindPerson(int id);
        Task AddRelationship(RelationshipDb relationshipDb);
        Task<IDictionary<int, PersonDb>> FindAllPeople();
        Task<IEnumerable<int>> FindRelationships(int personId, Relationship relationship);
        Task RemoveRelationships(int personId);
        Task DeletePerson(int personId);
    }
}
