namespace progect_DEPI.ViewModels
{
    public class AddQuizViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public TimeSpan Time { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        //public int UserId { get; set; }
        public int CourseId { get; set; }
    }
}
