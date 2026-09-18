using FluentAssertions;
using System.Text.Json;
using UP.Api.Services;

public class JsonMergePatchServiceTests
{
    private readonly JsonMergePatchService _service = new();

    [Fact]
    public void Apply_ShouldMergeNestedObjects()
    {
        var source = JsonDocument.Parse("""
        {
            "theme": "dark",
            "menu": {
                "collapsed": false,
                "items": ["home", "users"]
            }
        }
        """);

        var patch = JsonDocument.Parse("""
        {
            "menu": {
                "collapsed": true
            }
        }
        """);

        var result = _service.Apply(source, patch);

        result.RootElement.GetProperty("theme").GetString().Should().Be("dark");
        result.RootElement.GetProperty("menu").GetProperty("collapsed").GetBoolean().Should().BeTrue();
        result.RootElement.GetProperty("menu").GetProperty("items").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void Apply_ShouldAddNewProperty()
    {
        var source = JsonDocument.Parse("""
        {
            "theme": "dark"
        }
        """);

        var patch = JsonDocument.Parse("""
        {
            "sidebar": {
                "collapsed": true
            }
        }
        """);

        var result = _service.Apply(source, patch);

        result.RootElement.GetProperty("theme").GetString().Should().Be("dark");
        result.RootElement.GetProperty("sidebar").GetProperty("collapsed").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Apply_ShouldRemoveProperty_WhenPatchValueIsNull()
    {
        var source = JsonDocument.Parse("""
        {
            "theme": "dark",
            "sidebar": {
                "collapsed": true
            }
        }
        """);

        var patch = JsonDocument.Parse("""
        {
            "theme": null
        }
        """);

        var result = _service.Apply(source, patch);

        result.RootElement.TryGetProperty("theme", out _).Should().BeFalse();
        result.RootElement.GetProperty("sidebar").GetProperty("collapsed").GetBoolean().Should().BeTrue();
    }
}