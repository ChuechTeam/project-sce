using System.ComponentModel.DataAnnotations;

namespace Chuech.ProjectSce.Identity.Quickstart.Account
{
    public class RegisterViewModel
    {
        public string? ReturnUrl { get; set; }
        
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}