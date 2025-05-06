using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace progect_DEPI.Models
{
    public class Quizzes
    {
        [Key]  
        public int QuizId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
    }
}
