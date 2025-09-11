using FluentAssertions;
using HealthSync.Shared.Models;
using Xunit;

namespace HealthSync.Shared.Tests.Unit;

public class PatientProfileTests
{
    [Fact]
    public void Defaults_should_initialize_strings_as_empty()
    {
        var p = new PatientProfile();
        new[] { p.FirstName, p.LastName, p.PESEL, p.Address, p.PhoneNumber }
            .Should().AllSatisfy(s => { s.Should().NotBeNull(); s.Should().Be(string.Empty); });
    }
}
