using System.Text.Json;

namespace UP.Api.Features.UserViewFeature.Models;

public class UserView
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public JsonDocument View { get; set; } = JsonDocument.Parse("{}");
}
