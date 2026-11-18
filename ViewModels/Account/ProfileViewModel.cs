using System.ComponentModel.DataAnnotations;
using System.Web;

namespace UniManage.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "NIC Number")]
        public string NIC { get; set; }

        [StringLength(200)]
        [Display(Name = "Permanent Address")]
        public string Address { get; set; }

        public string Email { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        public string ProfilePicturePath { get; set; }

        [Display(Name = "Change Profile Picture")]
        public HttpPostedFileBase ProfilePicture { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Invalid Sri Lankan phone number (e.g. 0771234567)")]
        public string PhoneNumber { get; set; }
    }
}
