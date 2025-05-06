using Microsoft.AspNetCore.Mvc.Rendering;

namespace progect_DEPI.ViewModels
{
    public class AddQuestionViewModel
    {
        public string Text { get; set; }

        public int QuizId { get; set; }

        public List<SelectListItem>? Quizzes { get; set; }

        public List<string> Answers { get; set; } = new List<string> { "", "", "", "" };

        public int CorrectAnswerIndex { get; set; }
    }
}
