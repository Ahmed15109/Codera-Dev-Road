namespace progect_DEPI.Models
{
	public class Payment
	{
		public int PaymentId { get; set; }
		public DateTime PaymentDate { get; set; }
		public string PaymentMethod { get; set; }
		public decimal PaymentAmount { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }
	}
}
