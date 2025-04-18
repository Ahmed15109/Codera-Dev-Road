using System.ComponentModel.DataAnnotations;

namespace progect_DEPI.ViewModels
{
	
	

public class RegisterViewModel
	{
		[Required]
		public string name { get; set; }
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "Passwords do not match.")]
		public string ConfirmPassword { get; set; }
	}

}

