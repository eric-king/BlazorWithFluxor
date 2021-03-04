using System.ComponentModel.DataAnnotations;

namespace BlazorWithFluxor.Shared
{
    public class UserFeedbackModel
    {
        [EmailAddress]
        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        [Required]
        public int Rating { get; set; }
        [MaxLength(100)]
        public string Comment { get; set; }

        public UserFeedbackModel()
        {
            EmailAddress = string.Empty;
            Rating = 1;
            Comment = string.Empty;
        }
    }
}
