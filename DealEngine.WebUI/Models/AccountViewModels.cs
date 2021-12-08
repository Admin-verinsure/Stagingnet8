#region Using

using System;
using System.ComponentModel.DataAnnotations;
using DealEngine.Domain.Entities;

#endregion

namespace DealEngine.WebUI.Models
{
    public class AccountLoginModel : BaseViewModel
    {
        
        [Required]
        [DataType(DataType.Text)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
        public string DevicePrint { get; set; }
        public string DomainString { get; set; }
        public string CallbackDomainString { get; set; }
    }

    public class AccountResetPasswordModel : BaseViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class AccountChangePasswordModel : BaseViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordConfirm { get; set; }
    }

    public class AccountRegistrationModel : BaseViewModel
	{
		[Required]
		[DataType(DataType.Text)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [EmailAddress]
        [Compare("Email")]
        public string EmailConfirm { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordConfirm { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string FirstName { get; set; }

		[Required]
		[DataType(DataType.Text)]
		public string LastName { get; set; }
        
    }

    public class AccountErrorViewModel : BaseViewModel
    {
        //public string errorMessage { get; set; }
        //public string errorCode { get; set; }
        //public string reason { get; set; }
        public string message1 { get; set; }
        public string message2 { get; set; }
        public string message3 { get; set; }
        public string message4 { get; set; }
        public string tryAgainURL { get; set; }
        public string backURL { get; set; }
        public string backButtonText { get; set; }
        public string tryAgainButtonText { get; set; }
    }
}
