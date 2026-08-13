namespace UP.Api.Features.AuthFeature.Responses;

public class LoginResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
