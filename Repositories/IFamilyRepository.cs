using System.Collections.Generic;
using System.Threading.Tasks;
using family_archive_server.Models;

namespace family_archive_server.Repositories
{
    public interface IFamilyRepository
    {
        Task<IEnumerable<FamilyTreePerson>> GetFamilyTree();
        Task<IEnumerable<ListPerson>> GetList();
        Task<PersonDetails> GetDetails(int id);
        Task<PersonDetailsUpdate> GetDetailsForUpdate(int id);
        Task UpdatePerson(PersonDetailsUpdate personDetails);
        Task<int> AddPerson(PersonDetailsUpdate personDetails);
        Task DeletePerson(int id);
    }
}
