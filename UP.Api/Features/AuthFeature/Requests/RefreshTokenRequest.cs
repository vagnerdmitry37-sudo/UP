using System.ComponentModel.DataAnnotations;

namespace UP.Api.Features.AuthFeature.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string RefreshToken { get; set; }
    }
}
