using System;
using System.ComponentModel.DataAnnotations;

namespace UniManage.ViewModels.Admin
{
    public class AddUserViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Account Role")]
        public string Role { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "NIC Number")]
        public string NIC { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Invalid Sri Lankan phone number (e.g. 0771234567)")]
        public string PhoneNumber { get; set; }
    }

    public class UserListViewModel
    {
        public System.Collections.Generic.List<UniManage.Models.ApplicationUser> Users { get; set; } = new System.Collections.Generic.List<UniManage.Models.ApplicationUser>();
        public string SearchTerm { get; set; }
        public string Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class EditUserViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Account Role")]
        public string Role { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "NIC Number")]
        public string NIC { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password (Leave blank to keep current)")]
        public string NewPassword { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Invalid Sri Lankan phone number (e.g. 0771234567)")]
        public string PhoneNumber { get; set; }
    }
}
