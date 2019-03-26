using System.ComponentModel.DataAnnotations;

namespace StudentExercisesAPI.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string Slack { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }
    }
}
