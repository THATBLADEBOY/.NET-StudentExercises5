using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercisesAPI.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Language { get; set; }
        public List<Student> StudentsCurrentlyWorking { get; set; } = new List<Student>();
    }
}
