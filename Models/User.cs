namespace progect_DEPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ? Picture  { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string IdentityId { get; set; }


        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Certificate> Certificates { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; }

        
        
    }
}
