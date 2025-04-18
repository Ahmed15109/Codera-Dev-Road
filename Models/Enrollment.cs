namespace progect_DEPI.Models
{
	public class Enrollment
	{
		public int EnrollmentId { get; set; }
		public DateTime EnrolledAt { get; set; }
		public string Status { get; set; }
		public string PaymentStatus { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }


		public int CourseId { get; set; }
		public virtual Course Course { get; set; }
	}
}
