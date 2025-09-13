using FluentAssertions;
using Xunit;

namespace HealthSync.UnitTests.Core;

public class StatusMappingTests
{
    private static string GetStatusColor(string status) => status switch
    {
        "Scheduled" => "primary",
        "Confirmed" => "success",
        "Cancelled" => "danger",
        _           => "secondary"
    };

    [Theory]
    [InlineData("Scheduled","primary")]
    [InlineData("Confirmed","success")]
    [InlineData("Cancelled","danger")]
    [InlineData("Other","secondary")]
    public void Mapping_should_match_spec(string input, string expected)
        => GetStatusColor(input).Should().Be(expected);
}
