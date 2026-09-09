namespace UP.Api.Features.AuthFeature.Requests;

public class ChangePasswordRequest
{
    public required string NewPassword { get; init; }
    public required string CurrentPassword { get; init; }
}
