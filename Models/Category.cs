using System.ComponentModel.DataAnnotations.Schema;

namespace progect_DEPI.Models
{
	public class Category
	{
		public int CategoryId { get; set; }
		public string CategoryName { get; set; }
		public string Description { get; set; }
		public int LessonsCount { get; set; }
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

        public virtual ICollection<Course> Courses { get; set; }
	}
}
