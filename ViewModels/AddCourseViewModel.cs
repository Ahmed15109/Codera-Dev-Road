using System.ComponentModel.DataAnnotations.Schema;

namespace progect_DEPI.ViewModels
{
    public class AddCourseViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public int CategoryId { get; set; }
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

    }
}
