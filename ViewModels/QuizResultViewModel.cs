namespace progect_DEPI.ViewModels
{
    public class QuizResultViewModel
    {
        public string QuizTitle { get; set; }
        public int TotalQuestions { get; set; }
        public int Score { get; set; }
        public List<QuestionViewModel> Questions { get; set; }
    }

    public class QuestionViewModel
    {
        public string Text { get; set; } 
        public List<AnswerViewModel> Answers { get; set; } 
        public string UserAnswer { get; set; } 
        public string CorrectAnswer { get; set; } 
        public bool IsCorrect => UserAnswer == CorrectAnswer; 
    }

    public class AnswerViewModel
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; } 
        public bool IsSelectedByUser { get; set; }
    }
}
