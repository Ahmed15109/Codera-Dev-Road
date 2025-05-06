namespace progect_DEPI.Models
{
    public class QuizResult
    {
        public int QuizResultId { get; set; }

        public int QuizId { get; set; }
        public virtual Quizzes Quiz { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public int Score { get; set; }
        public DateTime TakenAt { get; set; }
    }

}
