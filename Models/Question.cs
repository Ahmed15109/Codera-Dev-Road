namespace progect_DEPI.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }

        public int QuizId { get; set; }
        public virtual Quizzes Quiz { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
    }

}
