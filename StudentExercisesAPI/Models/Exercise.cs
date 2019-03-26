using System.Collections.Generic;

namespace StudentExercisesAPI.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public List<Student> StudentsCurrentlyWorking { get; set; } = new List<Student>();
    }
}
