using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations.Schema;

namespace progect_DEPI.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public byte[]? Image { get; set; }
        [NotMapped]
        public IFormFile? formFile { get; set; }

        [NotMapped]
        public string? ImageUrl
        {
            get
            {
                if (Image != null)
                {
                    string base64 = Convert.ToBase64String(Image, 0, Image.Length);
                    return "data:image/png;base64," + base64;
                }
                return string.Empty;
            }
        }

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Quizzes> Quizzes { get; set; } = new List<Quizzes>();

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }

}
