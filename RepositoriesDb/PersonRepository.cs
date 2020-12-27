using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace family_archive_server.RepositoriesDb
{

    public class PersonRepository : IPersonRepository
    {
        private readonly IMapper _mapper;
        private readonly string _connectionString;

        public PersonRepository(IConfiguration configuration, IMapper mapper)
        {
            _mapper = mapper;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task UpdatePerson(PersonDb personDb)
        {
            var db = new MySqlConnection(_connectionString);
            await db.ExecuteAsync(@"
UPDATE People SET
GedcomID = @GedcomID, 
Gender = @Gender,
PreferredName = @PreferredName, 
GivenNames = @GivenNames,
Surname = @Surname,
BirthRangeStart = @BirthRangeStart,
BirthRangeEnd = @BirthRangeEnd,  
PlaceOfBirth = @PlaceOfBirth,
Dead = @Dead,
DeathRangeStart = @DeathRangeStart,
DeathRangeEnd = @DeathRangeEnd,  
PlaceOfDeath = @PlaceOfDeath,
Note = @Note,
Portrait = @Portrait
WHERE Id = @Id", personDb);

        }

        public async Task<int> AddPerson(PersonDb personDb)
        {

            personDb.NickName ??= "";
            personDb.PlaceOfBirth ??= "";
            personDb.PlaceOfDeath ??= "";
            personDb.Note ??= "";
            personDb.Portrait ??= "";
            personDb.GedcomId ??= "";

            var db = new MySqlConnection(_connectionString);

            var personId = 0;
            try
            {
                personId = await db.QuerySingleAsync<int>("SELECT Auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'People'");
                personDb.Id = personId;
                personDb.GedcomId = personId.ToString();

                await db.ExecuteAsync(@"
INSERT INTO People (
Id,
GedcomId, 
Gender,
PreferredName, 
GivenNames,
Surname,
BirthRangeStart,
BirthRangeEnd, 
PlaceOfBirth,
Dead,
DeathRangeStart,
DeathRangeEnd, 
PlaceOfDeath
) VALUES (
@Id,
@GedcomId, 
@Gender,
@PreferredName, 
@GivenNames,
@Surname,
@BirthRangeStart,
@BirthRangeEnd, 
@PlaceOfBirth,
@Dead,
@DeathRangeStart,
@DeathRangeEnd, 
@PlaceOfDeath)", personDb);
            }
            catch (Exception )
            {
                // ignored
            }

            return personId;
        }

        public async Task<PersonDb> FindPerson(int id)
        {
            var lookup = new Dictionary<int, PersonDb>();
            

            var db = new MySqlConnection(_connectionString);
            try
            {
 
               await db.QueryAsync<PersonTableDb, RelationshipDb, PersonDb> (@"
SELECT p.*, r.*
FROM People p
INNER JOIN Relationship r ON p.Id = r.Person1
WHERE p.Id = @Id",
                   (p, r) =>
                   {
                       if (!lookup.TryGetValue(p.Id, out var personDb))
                       {
                           lookup.Add(p.Id, personDb =  _mapper.Map<PersonDb>(p));
                       }

                       if (personDb.Relationships == null)
                       {
                           personDb.Relationships = new List<RelationshipTable>();
                       }
                       personDb.Relationships.Add(new RelationshipTable {PersonId = r.Person2, Relationship = Enum.Parse<Relationship>(r.RelationShip)});
                       return personDb;
                   }, splitOn: "Person1",
                   param: new {@Id = id});
            }
            catch (Exception)
            {
                // ignored
            }

            return lookup[id];

        }

        public async Task AddRelationship(RelationshipDb relationshipDb)
        {
            var db = new MySqlConnection(_connectionString);
            try
            {
                await db.ExecuteAsync("INSERT INTO Relationship (Person1, Relationship, Person2) VALUES ( @Person1, @Relationship, @Person2)", relationshipDb);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task<IDictionary<int, PersonDb>> FindAllPeople()
        {

            var lookup = new Dictionary<int, PersonDb>();

            var db = new MySqlConnection(_connectionString);
            try
            {

                await db.QueryAsync<PersonTableDb, RelationshipDb, PersonDb>(@"
SELECT p.*, r.*
FROM People p
INNER JOIN Relationship r ON p.Id = r.Person1",
                    (p, r) =>
                    {
                        if (!lookup.TryGetValue(p.Id, out var personDb))
                        {
                            lookup.Add(p.Id, personDb = _mapper.Map<PersonDb>(p));
                        }

                        personDb.Relationships ??= new List<RelationshipTable>();

                        personDb.Relationships.Add(new RelationshipTable
                            {PersonId = r.Person2, Relationship = Enum.Parse<Relationship>(r.RelationShip)});
                        return personDb;
                    }, splitOn: "Person1");
            }
            catch (Exception)
            {
                // ignored
            }

            return lookup;
        }

        public async Task<IEnumerable<int>> FindRelationships(int personId, Relationship relationship)
        {
            var db = new MySqlConnection(_connectionString);
            var result = await db.QueryAsync<int>(@"
SELECT Person1
FROM  Relationship 
WHERE Person2 = @SearchPerson AND
Relationship = @Relationship", new {SearchPerson = personId, Relationship = relationship.ToString()});
            return result;
            
        }

        public async Task RemoveRelationships(int personId)
        {
            var db = new MySqlConnection(_connectionString);
            await db.ExecuteAsync("DELETE FROM  Relationship WHERE Person1 = @id OR Person2 = @id", new{id = personId});
        }

        public async Task DeletePerson(int personId)
        {
            var db = new MySqlConnection(_connectionString);
            await db.ExecuteAsync("DELETE FROM People WHERE Id = @id", new { id = personId });
        }
    }
}
