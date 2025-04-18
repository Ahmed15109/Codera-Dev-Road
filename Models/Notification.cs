namespace progect_DEPI.Models
{
	public class Notification
	{
		public int NotificationId { get; set; }
		public string Message { get; set; }
		public bool IsRead { get; set; }
		public DateTime CreatedAt { get; set; }


		public int UserId { get; set; }
		public virtual User User { get; set; }
	}
}
