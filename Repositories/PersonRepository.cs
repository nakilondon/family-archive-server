using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace family_archive_server.Repositories
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

        public async Task AddPerson(PersonDb personDb)
        {

            if (personDb.NickName == null)
                personDb.NickName = "";

            if (personDb.PlaceOfBirth == null)
                personDb.PlaceOfBirth = "";

            if (personDb.PlaceOfDeath == null)
                personDb.PlaceOfDeath = "";

            if (personDb.Note == null)
                personDb.Note = "";

            if (personDb.Portrait == null)
                personDb.Portrait = "";


            var db = new MySqlConnection(_connectionString);
            try
            {
                await db.ExecuteAsync(@"
INSERT INTO People (
GedcomID, 
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
@GedcomID, 
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
            } catch (Exception e)
            {}
        }

        public async Task<PersonDb> FindPerson(int id)
        {
            var lookup = new Dictionary<int, PersonDb>();

            var db = new MySqlConnection(_connectionString);
            try
            {
 
               var result = await db.QueryAsync<PersonTableDb, RelationshipDb, PersonDb>(@"
SELECT p.*, r.*
FROM People p
INNER JOIN Relationship r ON p.Id = r.Person1 WHERE p.Id = @Id",
                   (p, r) =>
                   {
                       PersonDb personDb;
                       if (!lookup.TryGetValue(p.Id, out personDb))
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
            } catch (Exception e)
            {}

            return lookup[id];

        }

        public async Task AddRelationship(RelationshipDb relationshipDb)
        {
            var db = new MySqlConnection(_connectionString);
            try
            {
                var response = await db.ExecuteAsync("INSERT INTO Relationship (Person1, Relationship, Person2) VALUES ( @Person1, @Relationship, @Person2)", relationshipDb);
            }
            catch (Exception e)
            { }

        }

        public async Task<IDictionary<int, PersonDb>> FindAllPeople()
        {

            var lookup = new Dictionary<int, PersonDb>();

            var db = new MySqlConnection(_connectionString);
            try
            {

                var result = await db.QueryAsync<PersonTableDb, RelationshipDb, PersonDb>(@"
SELECT p.*, r.*
FROM People p
INNER JOIN Relationship r ON p.Id = r.Person1",
                    (p, r) =>
                    {
                        PersonDb personDb;
                        if (!lookup.TryGetValue(p.Id, out personDb))
                        {
                            lookup.Add(p.Id, personDb = _mapper.Map<PersonDb>(p));
                        }

                        if (personDb.Relationships == null)
                        {
                            personDb.Relationships = new List<RelationshipTable>();
                        }

                        personDb.Relationships.Add(new RelationshipTable
                            {PersonId = r.Person2, Relationship = Enum.Parse<Relationship>(r.RelationShip)});
                        return personDb;
                    }, splitOn: "Person1");
            }
            catch (Exception e)
            {
            }

            return lookup;
        }

        public async Task<IEnumerable<int>> FindRelationships(int personId, Relationship relationship)
        {
            var db = new MySqlConnection(_connectionString);
            try
            {

                var result = await db.QueryAsync<int>(@"
SELECT Person1
FROM  Relationship 
WHERE Person2 = @SearchPerson AND
Relationship = @Relationship", new {SearchPerson = personId, Relationship = relationship.ToString()});
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task RemoveRelationships(int personId)
        {
            var db = new MySqlConnection(_connectionString);
            try
            {

                await db.ExecuteAsync("DELETE FROM  Relationship WHERE Person1 = @id OR Person2 = @id", new{id = personId});
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
