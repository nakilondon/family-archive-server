using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using family_archive_server.Models;
using family_archive_server.RepositoriesDb;
using family_archive_server.Utilities;

namespace family_archive_server.Repositories
{
    public class FamilyRepository : IFamilyRepository   
    {
        private readonly IPersonRepository _personRepository;
        private readonly IMapper _mapper;
        private readonly IImagesRepository _imagesRepository;

        public FamilyRepository(IPersonRepository personRepository, IMapper mapper, IImagesRepository imagesRepository)
        {
            _personRepository = personRepository;
            _mapper = mapper;
            _imagesRepository = imagesRepository;
        }
        

        public async Task<IEnumerable<FamilyTreePerson>> GetFamilyTree(Roles roles)
        {
            var peopleDb= await _personRepository.FindAllPeople();
            var familyTreePeople = new List<FamilyTreePerson>();

            foreach (var personDb in peopleDb)
            {
                if (roles == Roles.General && !personDb.Value.Dead)
                {
                    continue;
                }

                var familyTreePerson = new FamilyTreePerson
                {
                    Id = personDb.Value.Id,
                    Title = personDb.Value.PreferredName,
                    Spouses = new List<int>(),
                    Parents = new List<int>(),
                    BirthDate = personDb.Value.BirthRangeStart
                };

                familyTreePerson.Description = PersonUtils.FindDates(personDb.Value);
                
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
                    familyTreePerson.Image = $"picture/thumbnail/{personDb.Value.Portrait}";
                }

                var spouses = personDb.Value.Relationships.Where(r =>
                    r.Relationship == Relationship.Husband || r.Relationship == Relationship.Wife ||
                    r.Relationship == Relationship.Spouse).ToList();

                foreach (var spouse in spouses)
                {
                    if (roles != Roles.General || peopleDb[spouse.PersonId].Dead)
                    {
                        familyTreePerson.Spouses.Add(spouse.PersonId);
                    }
                }

                var parents = personDb.Value.Relationships.Where(r =>
                    r.Relationship == Relationship.Father || r.Relationship == Relationship.Mother ||
                    r.Relationship == Relationship.Parent).ToList();

                foreach (var parent in parents)
                {
                    if (roles != Roles.General || peopleDb[parent.PersonId].Dead)
                    {
                        familyTreePerson.Parents.Add(parent.PersonId);
                    }
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

        public async Task<IEnumerable<ListPerson>> GetList(Roles roles)
        {
            var peopleDb = await _personRepository.FindAllPeople();
            var listPeople = new List<ListPerson>();

            foreach (var personDb in peopleDb)
            {
                if (roles != Roles.General || personDb.Value.Dead)
                {
                    listPeople.Add(PersonUtils.CreateListPerson(personDb.Value));
                }
            }

            return listPeople;
        }

        public async Task<PersonDetails> GetDetails(Roles roles, int id)
        {
            var personDb = await _personRepository.FindPerson(id);

            var family = await PersonUtils.FindSiblings(personDb, _personRepository);
            family.AddRange(personDb.Relationships);
            var familyDetails = new List<Family>();

            foreach (var familyMember in family.OrderBy(f => f.Relationship))
            {
                var familyMemberDetails = await _personRepository.FindPerson(familyMember.PersonId);

                if (roles != Roles.General || familyMemberDetails.Dead)
                {
                    familyDetails.Add(new Family
                    {
                        Id = familyMember.PersonId,
                        Name = familyMemberDetails.PreferredName,
                        Relationship = familyMember.Relationship.ToString()
                    });
                }
            }

            var personDetails = _mapper.Map<PersonDetails>(personDb);

            if(string.IsNullOrEmpty(personDetails.Portrait))
            {
                var gender = (Gender)Enum.Parse(typeof(Gender), personDb.Gender);
                personDetails.Portrait = gender == Gender.Male ? "Male.png" : "Female.png";
            }

            personDetails.Family = familyDetails;
            var images = await _imagesRepository.GetImagesForPerson(id);
            personDetails.Images = new List<ImageDetails>();

            foreach (var image in images)
            {
                var peopleInImage = await _imagesRepository.GetPeopleInImage(image.Id);
                
                string caption = peopleInImage.Count > 0 ? "People: \n" : "";
                
                foreach (var personInImage in peopleInImage)
                {
                    var personDbImage = await _personRepository.FindPerson(personInImage);
                    caption += $"\t{personDbImage.PreferredName}\n";
                }

                if (!string.IsNullOrEmpty(image.Description))
                {
                    caption += $"Description: {image.Description}\n";
                }

                if (!string.IsNullOrEmpty(image.Location))
                {
                    caption += $"Location: {image.Location}\n";
                }

                personDetails.Images.Add(new ImageDetails { Id = image.Id, FileName = image.FileName, Orientation = image.Orientation, Caption = caption });
            }

            return personDetails;
        }

        public async Task<PersonDetailsUpdate> GetDetailsForUpdate(int id)
        {
            var personDb = await _personRepository.FindPerson(id);

            var familyDetails = new List<FamilyUpdateInternal>();
            foreach (var relationship in personDb.Relationships)
            {
                var familyPerson = await _personRepository.FindPerson(relationship.PersonId);
                familyDetails.Add(new FamilyUpdateInternal
                {
                    Id = relationship.PersonId,
                    Label = familyPerson.PreferredName + " " + PersonUtils.FindDates(familyPerson),
                    Relationship = relationship.Relationship
                });
            }

            var personDetails = _mapper.Map<PersonDetailsUpdate>(personDb);

            personDetails.Spouses = familyDetails
                .Where(r => r.Relationship == Relationship.Wife || r.Relationship == Relationship.Husband ||
                            r.Relationship == Relationship.Spouse)
                .Select(r => new ListPerson{Id = r.Id, Label = r.Label})
                .ToList();

            personDetails.Parents = familyDetails
                .Where(r => r.Relationship == Relationship.Mother || r.Relationship == Relationship.Father ||
                            r.Relationship == Relationship.Parent)
                .Select(r => new ListPerson { Id = r.Id, Label = r.Label })
                .ToList();

            personDetails.Children = familyDetails
                .Where(r => r.Relationship == Relationship.Daughter || r.Relationship == Relationship.Son ||
                            r.Relationship == Relationship.Child)
                .Select(r => new ListPerson { Id = r.Id, Label = r.Label })
                .ToList();

            if (string.IsNullOrEmpty(personDetails.Portrait))
            {
                var gender = (Gender)Enum.Parse(typeof(Gender), personDb.Gender);
                personDetails.Portrait = gender == Gender.Male ? "Male.png" : "Female.png";
            }

            return personDetails;
        }

        private async Task CreateRelationships(PersonDetailsUpdate personDetails)
        {
            if (personDetails.Id != 0)
            {
                await _personRepository.RemoveRelationships(personDetails.Id);
            }

            foreach (var parent in personDetails.Parents)
            {
                var parentDetails = await _personRepository.FindPerson(parent.Id);
                await _personRepository.AddRelationship(new RelationshipDb
                {
                    Person1 = personDetails.Id,
                    RelationShip = parentDetails.Gender == Gender.Female.ToString()
                        ? Relationship.Mother.ToString()
                        : Relationship.Father.ToString(),
                    Person2 = parent.Id
                });
                await _personRepository.AddRelationship(new RelationshipDb()
                {
                    Person1 = parent.Id,
                    RelationShip = personDetails.Gender == Gender.Female.ToString()
                        ? Relationship.Daughter.ToString()
                        : Relationship.Son.ToString(),
                    Person2 = personDetails.Id
                });
            }

            foreach (var spouse in personDetails.Spouses)
            {
                var spouseDetails = await _personRepository.FindPerson(spouse.Id);
                await _personRepository.AddRelationship(new RelationshipDb
                {
                    Person1 = personDetails.Id,
                    RelationShip = spouseDetails.Gender == Gender.Female.ToString()
                        ? Relationship.Wife.ToString()
                        : Relationship.Husband.ToString(),
                    Person2 = spouse.Id
                });
                await _personRepository.AddRelationship(new RelationshipDb()
                {
                    Person1 = spouse.Id,
                    RelationShip = personDetails.Gender == Gender.Female.ToString()
                        ? Relationship.Wife.ToString()
                        : Relationship.Husband.ToString(),
                    Person2 = personDetails.Id
                });
            }

            foreach (var child in personDetails.Children) 
            {
                var childDetails = await _personRepository.FindPerson(child.Id);

                await _personRepository.AddRelationship(new RelationshipDb()
                {
                    Person1 = personDetails.Id,
                    RelationShip = childDetails.Gender == Gender.Female.ToString()
                        ? Relationship.Daughter.ToString()
                        : Relationship.Son.ToString(),
                    Person2 = child.Id
                });

                await _personRepository.AddRelationship(new RelationshipDb
                {
                    Person1 = child.Id,
                    RelationShip = personDetails.Gender == Gender.Female.ToString()
                        ? Relationship.Mother.ToString()
                        : Relationship.Father.ToString(),
                    Person2 = personDetails.Id
                });
            }
        }

        public async Task UpdatePerson(PersonDetailsUpdate personDetails)
        {
            var personDd = await _personRepository.FindPerson(personDetails.Id); 
            _mapper.Map(personDetails, personDd);
            await CreateRelationships(personDetails);
            await _personRepository.UpdatePerson(personDd);
        }

        public async Task<int> AddPerson(PersonDetailsUpdate personDetails)
        {
            var personDb = _mapper.Map<PersonDb>(personDetails);
            var personId = await _personRepository.AddPerson(personDb);
            personDetails.Id = personId;
            await CreateRelationships(personDetails);
            return personId;
        }

        public async Task DeletePerson(int id)
        {
            await _personRepository.RemoveRelationships(id);
            await _personRepository.DeletePerson(id);
        }
    }
}
