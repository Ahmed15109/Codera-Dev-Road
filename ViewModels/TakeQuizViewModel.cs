namespace progect_DEPI.ViewModels
{
    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public List<QuestionWithAnswers> Questions { get; set; }

        public class QuestionWithAnswers
        {
            public int QuestionId { get; set; }
            public string Text { get; set; }
            public List<AnswerChoice> Answers { get; set; }
        }

        public class AnswerChoice
        {
            public int AnswerId { get; set; }
            public string Text { get; set; }
        }

        public List<AnswerSubmission> Submissions { get; set; } = new();

        public class AnswerSubmission
        {
            public int QuestionId { get; set; }
            public int SelectedAnswerId { get; set; }
        }
    }
}
