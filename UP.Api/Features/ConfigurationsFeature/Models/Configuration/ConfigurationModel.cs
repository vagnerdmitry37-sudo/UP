using UP.Api.Features.ConfigurationsFeature.Models.Settings;

namespace UP.Api.Features.ConfigurationsFeature.Models.Configuration;

public class ConfigurationModel
{
    public int Id { get; set; }
    public string Theme { get; set; } = "";
    public SettingsModel Settings { get; set; } = new();
}
