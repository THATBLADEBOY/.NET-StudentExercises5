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
    public class ExercisesController : ControllerBase
    {
        public SqlConnection Connection
        {
            get
            {
                string connectionSTring = "Server=localhost\\SQLExpress;Database=StudentExercises;Integrated Security=true";
                return new SqlConnection(connectionSTring);
            }
        }

        // GET: api/Exercises
        [HttpGet]
        public List<Exercise> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (include == "student")
                    {
                        cmd.CommandText = @"SELECT s.ID as StudentId, s.FirstName as StudentFirstName, s.LastName as StudentLastName,
                                            s.Slack as StudentSlack, s.CohortId as StudentCohortId, e.[Name] as ExerciseName, e.Id as ExerciseId,
                                            e.Language as ExerciseLanguage, c.name as cohortname, c.Id as cohortId, se.Student as StudentExStudent
                                           FROM Student s INNER JOIN StudentExercise se ON s.Id = se.Student
                                           INNER JOIN Exercise e ON se.Exercise = e.Id
                                            INNER JOIN Cohort c ON s.cohortId = c.id
                                            WHERE @q LIKE e.[Name] OR @q LIKE e.language";
                        cmd.Parameters.Add(new SqlParameter("@q", q));
                        SqlDataReader reader = cmd.ExecuteReader();
                        Dictionary<int, Exercise> exercises = new Dictionary<int, Exercise>();
                        while (reader.Read())
                        {
                            int exerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"));
                            if (!exercises.ContainsKey(exerciseId))
                            {
                                Exercise newExercise = new Exercise
                                {
                                    Id = exerciseId,
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                                };
                                exercises.Add(exerciseId, newExercise);
                            }
                            if (!reader.IsDBNull(reader.GetOrdinal("StudentId")))
                            {
                                Exercise currentExercise = exercises[exerciseId];
                                currentExercise.StudentsCurrentlyWorking.Add(
                                    new Student
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("StudentExStudent")),
                                        FirstName = reader.GetString(reader.GetOrdinal("StudentFirstName")),
                                        LastName = reader.GetString(reader.GetOrdinal("StudentLastName")),
                                        Slack = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                        CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortId")),
                                        Cohort = new Cohort
                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("cohortId")),
                                            Name = reader.GetString(reader.GetOrdinal("cohortname"))
                                        }
                                    }
                                );
                            }


                        }
                        reader.Close();
                        return exercises.Values.ToList(); ;
                    }
                    else
                    {

                        cmd.CommandText = @"SELECT e.id as ExerciseId, e.name as ExerciseName, e.language as ExerciseLanguage
                                        FROM Exercise e
                                        WHERE @q LIKE e.[Name] OR @q LIKE e.language";

                        cmd.Parameters.Add(new SqlParameter("@q", q)); 
                        SqlDataReader reader = cmd.ExecuteReader();

                        List<Exercise> exercises = new List<Exercise>();
                        while (reader.Read())
                        {
                            Exercise exercise = new Exercise
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ExerciseId")),
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage"))
                            };
                            exercises.Add(exercise);
                        }
                        reader.Close();
                        return exercises;
                    }
                }
            }
        }

    }
}