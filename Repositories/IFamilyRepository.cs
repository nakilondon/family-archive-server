using System.Collections.Generic;
using System.Threading.Tasks;
using family_archive_server.Models;

namespace family_archive_server.Repositories
{
    public interface IFamilyRepository
    {
        Task<IEnumerable<FamilyTreePerson>> GetFamilyTree(Roles roles);
        Task<IEnumerable<ListPerson>> GetList(Roles roles);
        Task<PersonDetails> GetDetails(Roles roles, int id);
        Task<PersonDetailsUpdate> GetDetailsForUpdate(int id);
        Task UpdatePerson(PersonDetailsUpdate personDetails);
        Task<int> AddPerson(PersonDetailsUpdate personDetails);
        Task DeletePerson(int id);
    }
}
