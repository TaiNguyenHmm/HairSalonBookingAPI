
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace BusinessObjects.Authentication
{
    public class LoginDto
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        [JsonPropertyName("rememberMe")]
        public bool RememberMe { get; set; }

    }
}
