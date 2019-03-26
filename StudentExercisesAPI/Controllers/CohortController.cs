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

        // GET: api/Instructors
        [HttpGet]
        public List<Cohort> GetAllCohorts()
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
                                        LEFT JOIN Instructor i ON i.CohortId = c.Id";
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

     /*   // GET: api/Instructors/5
        [HttpGet("{id}", Name = "GetInstructor")]
        public Instructor Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT i.id, i.firstname, i.lastname,
                                               i.slack, i.cohortId, c.name as cohortname
                                          FROM Instructor i INNER JOIN Cohort c ON i.cohortid = c.id
                                         WHERE i.id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Instructor instructor = null;
                    if (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                            LastName = reader.GetString(reader.GetOrdinal("lastname")),
                            Slack = reader.GetString(reader.GetOrdinal("slack")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortid")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortid")),
                                Name = reader.GetString(reader.GetOrdinal("cohortname"))
                            }
                        };
                    }

                    reader.Close();
                    return instructor;
                }
            }
        }

        // POST: api/Instructors
        [HttpPost]
        public ActionResult Post([FromBody] Instructor newInstructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO instructor (firstname, lastname, slack, cohortid)
                                             OUTPUT INSERTED.Id
                                             VALUES (@firstname, @lastname, @slack, @cohortid)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newInstructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newInstructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slack", newInstructor.Slack));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", newInstructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newInstructor.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, newInstructor);
                }
            }
        }

        // PUT: api/Instructors/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE instructor 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slack = @slack, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@firstname", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slack", instructor.Slack));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", instructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM instructor WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }*/
    }
}