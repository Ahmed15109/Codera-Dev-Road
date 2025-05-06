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
        public string? ImageUrl { get; set; }
    }
}
