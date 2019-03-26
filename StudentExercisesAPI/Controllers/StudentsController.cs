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
    public class StudentsController : ControllerBase
    {
        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercises;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }

        // GET: api/Students
        [HttpGet]
        public List<Student> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "exercise")
                    {
                        cmd.CommandText = @"SELECT s.ID as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName,
                                            s.Slack as StudentSlack, s.CohortId as StudentCohortId, e.[Name] as ExerciseName, e.Id as ExerciseId,
                                            e.Language as ExerciseLanguage, c.name as cohortname, c.Id as cohortId
                                           FROM Student s INNER JOIN StudentExercise se ON s.Id = se.Student
                                           INNER JOIN Exercise e ON se.Exercise = e.Id
                                            INNER JOIN Cohort c ON s.cohortId = c.id
                                            WHERE s.FirstName LIKE @q OR s.lastname LIKE @q OR s.slack LIKE @q"; 
                        cmd.Parameters.Add(new SqlParameter("@q", q));
                        SqlDataReader reader = cmd.ExecuteReader();
                        Dictionary<int, Student> students = new Dictionary<int, Student>();
                        while (reader.Read())
                        {
                            int studentId = reader.GetInt32(reader.GetOrdinal("StudentId"));
                            if (!students.ContainsKey(studentId))
                            {
                                Student newStudent = new Student
                                {
                                    Id = studentId,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                    CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                                    Slack = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    Cohort = new Cohort
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                        Name = reader.GetString(reader.GetOrdinal("cohortname"))
                                    }
                                };
                                students.Add(studentId, newStudent);
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("ExerciseId")))
                            {
                                Student currentStudent = students[studentId];
                                currentStudent.Exercises.Add(
                                    new Exercise
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                                        Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                        Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                                    }
                                );
                            }
                            

                        }
                        reader.Close();
                        return students.Values.ToList(); ;
                    }
                    else
                    {

                        cmd.CommandText = @"SELECT s.id, s.firstname, s.lastname, s.slack, s.cohortId, c.name as cohortname
                                        FROM Student s INNER JOIN Cohort c ON s.cohortId = c.id
                                        WHERE s.FirstName LIKE @q OR s.lastname LIKE @q OR s.slack LIKE @q";
                        cmd.Parameters.Add(new SqlParameter("@q", q));
                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Student> students = new List<Student>();
                        while (reader.Read())
                        {
                            Student student = new Student
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                                LastName = reader.GetString(reader.GetOrdinal("lastname")),
                                Slack = reader.GetString(reader.GetOrdinal("slack")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                    Name = reader.GetString(reader.GetOrdinal("cohortname"))
                                }
                            };
                            students.Add(student);
                        }
                        reader.Close();
                        return students;
                    }
                }
            }
        }

        // GET: api/Students/5
        [HttpGet("{id}", Name = "GetStudent")]
        public Student Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT s.id, s.firstname, s.lastname,
                                s.slack, s.cohortId, c.name as cohortname
                                FROM Student s INNER JOIN Cohort c ON s.cohortID = c.id
                                WHERE s.id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;
                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                            LastName = reader.GetString(reader.GetOrdinal("lastname")),
                            Slack = reader.GetString(reader.GetOrdinal("slack")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                Name = reader.GetString(reader.GetOrdinal("cohortname"))
                            }
                        };
                    }
                    reader.Close();
                    return student;
                }
            }
        }

        // POST: api/Students
        [HttpPost]
        public ActionResult Post([FromBody] Student newStudent)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO student (firstname, lastname, slack, cohortid)
                                    OUTPUT INSERTED.Id
                                    VALUES (@firstname, @lastname, @slack, @cohortid)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", newStudent.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", newStudent.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slack", newStudent.Slack));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", newStudent.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    newStudent.Id = newId;
                    return CreatedAtRoute("GetInstructor", new { id = newId }, newStudent);
                }
            }
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE student 
                                           SET firstname = @firstname, 
                                               lastname = @lastname,
                                               slack = @slack, 
                                               cohortid = @cohortid
                                         WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@firstname", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slack", student.Slack));
                    cmd.Parameters.Add(new SqlParameter("@cohortid", student.CohortId));
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
                    cmd.CommandText = "DELETE FROM student WHERE id = @id;";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
