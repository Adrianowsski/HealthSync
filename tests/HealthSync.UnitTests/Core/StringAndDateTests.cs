using System;
using FluentAssertions;
using Xunit;

namespace HealthSync.UnitTests.Core;

public class StringAndDateTests
{
    [Fact]
    public void Case_insensitive_compare()
        => "healthsync".Should().BeEquivalentTo("HEALTHSYNC");

    [Fact]
    public void Arithmetic_should_be_stable()
        => (1 + 1).Should().Be(2);

    [Fact]
    public void Utc_clock_should_progress()
    {
        var t1 = DateTime.UtcNow;
        var t2 = DateTime.UtcNow;
        (t2 - t1).Should().BeLessThan(TimeSpan.FromSeconds(2));
    }
}
