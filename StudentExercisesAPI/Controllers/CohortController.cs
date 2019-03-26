using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentExercisesAPI.Models;

namespace StudentExercisesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CohortsController : ControllerBase
    {
        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercises;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }

        // GET: api/Cohorts
        [HttpGet]
        public List<Cohort> GetAllCohorts(string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.id as CohortId, c.[name] as CohortName, s.Id as StudentId, s.FirstName as StudentFirstName,
                                        s.LastName as StudentLastName, s.Slack as StudentSlack, i.Id as InstructorId, i.FirstName as InstructorFirstName,
                                        i.LastName as InstructorLastName, i.Slack as InstructorSlack
                                        FROM Cohort c LEFT JOIN Student s ON s.CohortId = c.Id 
                                        LEFT JOIN Instructor i ON i.CohortId = c.Id
                                        WHERE c.[name] LIKE @q";
                    cmd.Parameters.Add(new SqlParameter("@q", q));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();
                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort newCohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            };
                            cohorts.Add(cohortId, newCohort);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Cohort currentCohort = cohorts[cohortId];
                            currentCohort.Students.Add(
                                new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    CohortId = cohortId,
                                    Slack = reader.GetString(reader.GetOrdinal("StudentSlack"))
                                }
                            );
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            Cohort currentCohort = cohorts[cohortId];
                            currentCohort.Instructors.Add(
                                new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    CohortId = cohortId,
                                    Slack = reader.GetString(reader.GetOrdinal("InstructorSlack"))
                                }
                            );
                        }
                    }
                    reader.Close();
                    return cohorts.Values.ToList();
                }
            }
        }

        // GET: api/Cohorts/id
        [HttpGet("{id}", Name = "GetCohort")]
        public List<Cohort> Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT c.id as CohortId, c.[name] as CohortName, s.Id as StudentId, s.FirstName as StudentFirstName,
                                        s.LastName as StudentLastName, s.Slack as StudentSlack, i.Id as InstructorId, i.FirstName as InstructorFirstName,
                                        i.LastName as InstructorLastName, i.Slack as InstructorSlack
                                        FROM Cohort c LEFT JOIN Student s ON s.CohortId = c.Id 
                                        LEFT JOIN Instructor i ON i.CohortId = c.Id
                                        WHERE c.id = @id
                                    ";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Dictionary<int, Cohort> cohorts = new Dictionary<int, Cohort>();
                    while (reader.Read())
                    {
                        int cohortId = reader.GetInt32(reader.GetOrdinal("CohortId"));
                        if (!cohorts.ContainsKey(cohortId))
                        {
                            Cohort newCohort = new Cohort
                            {
                                Id = cohortId,
                                Name = reader.GetString(reader.GetOrdinal("CohortName"))
                            };
                            cohorts.Add(cohortId, newCohort);
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                        {
                            Cohort currentCohort = cohorts[cohortId];
                            currentCohort.Students.Add(
                                new Student
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    CohortId = cohortId,
                                    Slack = reader.GetString(reader.GetOrdinal("StudentSlack"))
                                }
                            );
                        }
                        if (!reader.IsDBNull(reader.GetOrdinal("InstructorId")))
                        {
                            Cohort currentCohort = cohorts[cohortId];
                            currentCohort.Instructors.Add(
                                new Instructor
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLastName")),
                                    CohortId = cohortId,
                                    Slack = reader.GetString(reader.GetOrdinal("InstructorSlack"))
                                }
                            );
                        }
                    }
                    reader.Close();
                    return cohorts.Values.ToList();
                }
            }
        }

        // POST: api/Cohorts
        [HttpPost]
        public ActionResult Post([FromBody] Cohort newCohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO cohort (name)
                                             OUTPUT INSERTED.Id
                                             VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", newCohort.Name));
                   

                    int newId = (int)cmd.ExecuteScalar();
                    newCohort.Id = newId;
                    return CreatedAtRoute("GetCohort", new { id = newId }, newCohort);
                }
            }
        }

        // PUT: api/Cohorts/id
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE cohort 
                                           SET name = @name, 
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@firstname", cohort.Name));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: api/ApiWithActions/id
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM cohort WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}