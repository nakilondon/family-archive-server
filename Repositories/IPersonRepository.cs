using System.Collections.Generic;
using System.Threading.Tasks;

namespace family_archive_server.Repositories
{
    public interface IPersonRepository
    {
        Task AddPerson(PersonDb personDb);
        Task<PersonDb> FindPerson(int id);
        Task AddRelationship(RelationshipDb relationshipDb);
        Task<IDictionary<int, PersonDb>> FindAllPeople();
    }
}
