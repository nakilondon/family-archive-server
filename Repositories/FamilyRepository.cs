using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using family_archive_server.Models;
using family_archive_server.Utilities;
//using Google.Protobuf.WellKnownTypes;

namespace family_archive_server.Repositories
{
    public class FamilyRepository : IFamilyRepository   
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;

        public FamilyRepository(IPersonRepository personRepository, IMapper mapper)
        {
            _personRepository = personRepository;
            _mapper = mapper;
        }
        
        public string FindDates(PersonDb personDb)
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

        public async Task<IEnumerable<FamilyTreePerson>> GetFamilyTree()
        {
            var peopleDb= await _personRepository.FindAllPeople();
            var familyTreePeople = new List<FamilyTreePerson>();

            foreach (var personDb in peopleDb)
            {
                var familyTreePerson = new FamilyTreePerson
                {
                    Id = personDb.Value.Id,
                    Title = personDb.Value.PreferredName,
                    Spouses = new List<int>(),
                    Parents = new List<int>(),
                    BirthDate = personDb.Value.BirthRangeStart
                };

                familyTreePerson.Description = FindDates(personDb.Value);
                
                var gender = (Gender)Enum.Parse(typeof(Gender),personDb.Value.Gender);

                if (gender == Gender.Female)
                {
                    familyTreePerson.ItemTitleColor = "#FDD7E4";
                    familyTreePerson.Image = "/img/Female.png";
                }

                if (gender == Gender.Male)
                {
                    familyTreePerson.Image = "/img/Male.png";
                }

                
                if (!string.IsNullOrWhiteSpace(personDb.Value?.Portrait))
                {
                    familyTreePerson.Image = $"familytree/thumbnail/{personDb.Value.Portrait}";
                }

                var spouses = personDb.Value.Relationships.Where(r =>
                    r.Relationship == Relationship.Husband || r.Relationship == Relationship.Wife ||
                    r.Relationship == Relationship.Spouse).ToList();

                foreach (var spouse in spouses)
                {
                    familyTreePerson.Spouses.Add(spouse.PersonId);
                }

                var parents = personDb.Value.Relationships.Where(r =>
                    r.Relationship == Relationship.Father || r.Relationship == Relationship.Mother ||
                    r.Relationship == Relationship.Parent).ToList();

                foreach (var parent in parents)
                {
                    familyTreePerson.Parents.Add(parent.PersonId);
                }
                
                familyTreePeople.Add(familyTreePerson);
            }

            foreach (var person in familyTreePeople)
            {
                if (person.Parents.Any())
                {
                    var siblings = familyTreePeople.FindAll(p => p.Parents.Contains(person.Parents.First()))
                        .OrderBy(p => p.BirthDate);

                    short position = 0;
                    int relativeId = 0;
                    foreach (var sibling in siblings)
                    {
                        if (position == 0)
                        {
                            relativeId = sibling.Id;
                        }
                        else
                        {
                            sibling.RelativeItem = relativeId;
                            sibling.PlacementType = AdviserPlacementType.Right;
                            sibling.Position = position;
                        }

                        position++;
                    }
                }
            }
            return familyTreePeople;
        }

        public async Task<IEnumerable<ListPerson>> GetList()
        {
            var peopleDb = await _personRepository.FindAllPeople();
            var listPeople = new List<ListPerson>();

            foreach (var personDb in peopleDb)
            {
                var person = new ListPerson
                {
                    Id = personDb.Key,
                    Label = personDb.Value.PreferredName + " " + FindDates(personDb.Value)
                };
                
                listPeople.Add(person);
            }

            return listPeople;
        }

        public async Task<PersonDetails> GetDetails(int id)
        {
            var personDb = await _personRepository.FindPerson(id);

            var familyDetails = new List<Family>();
            foreach (var relationship in personDb.Relationships)
            {
                var familyPerson = await _personRepository.FindPerson(relationship.PersonId);
                familyDetails.Add(new Family
                {
                    Id = relationship.PersonId,
                    Name = familyPerson.PreferredName,
                    Relationship = relationship.Relationship.ToString()
                });
            }

            var personDetails = _mapper.Map<PersonDetails>(personDb);

            if(string.IsNullOrEmpty(personDetails.Portrait))
            {
                var gender = (Gender)Enum.Parse(typeof(Gender), personDb.Gender);
                personDetails.Portrait = gender == Gender.Male ? "Male.png" : "Female.png";
            }
            personDetails.Family = familyDetails;

            return personDetails;
        }

        public async Task UpdatePerson(PersonDetails personDetails)
        {
            var personDd = await _personRepository.FindPerson(personDetails.Id); 
            _mapper.Map(personDetails, personDd);
            await _personRepository.UpdatePerson(personDd);
        }
    }
}
