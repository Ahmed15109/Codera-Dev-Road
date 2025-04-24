using Microsoft.AspNetCore.Mvc.Rendering;
using progect_DEPI.Models;

namespace progect_DEPI.ViewModels
{
    public class AddLessonViewModel
    {
        public int? LessonId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int OrderNumber { get; set; }
        public string Level { get; set; }

        public int CourseId { get; set; }
        public List<SelectListItem> Courses { get; set; } // جديدة
    }

}
