namespace progect_DEPI.ViewModels
{
    public class QuizResultViewModel
    {
        public string QuizTitle { get; set; }
        public int TotalQuestions { get; set; }
        public int Score { get; set; }
        public List<QuestionViewModel> Questions { get; set; } // List of question results
    }

    public class QuestionViewModel
    {
        public string Text { get; set; } 
        public List<AnswerViewModel> Answers { get; set; } // All answer choices
        public string UserAnswer { get; set; } // What the user chose
        public string CorrectAnswer { get; set; } // Correct answer text
        public bool IsCorrect => UserAnswer == CorrectAnswer; // Comparison
    }

    public class AnswerViewModel
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; } 
        public bool IsSelectedByUser { get; set; }
    }
}
