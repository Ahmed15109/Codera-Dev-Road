namespace progect_DEPI.Models
{
	
	public class Certificate
	{
		public int CertificateId { get; set; }
	public string Content { get; set; }
	public DateTime CreatedAt { get; set; }

	public int UserId { get; set; }
	public virtual User User { get; set; }
  }
}
