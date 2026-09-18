using System.Text.Json;
using UP.Api.Features.AuthFeature.Models.AuthUser;

namespace UP.Api.Features.AppUserFeature.Models;

public class AppUserModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public JsonDocument View { get; set; } = JsonDocument.Parse("{}");

    public int AuthUserId { get; set; }
    public AuthUserModel AuthUser { get; set; } = null!;
}
