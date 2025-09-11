using FluentAssertions;
using HealthSync.Shared.Models;
using Xunit;

namespace HealthSync.Shared.Tests.Unit;

public class PatientProfileTests
{
    [Fact]
    public void Defaults_should_initialize_strings_as_empty_not_null()
    {
        var p = new PatientProfile();

        new[] { p.FirstName, p.LastName, p.PESEL, p.Address, p.PhoneNumber }
            .Should().AllSatisfy(s =>
            {
                s.Should().NotBeNull();
                s.Should().Be(string.Empty);
            });
    }

    [Fact]
    public void Assigning_basic_data_should_persist_values()
    {
        var p = new PatientProfile
        {
            FirstName = "Jan",
            LastName = "Nowak",
            PESEL = "90010112345",
            Address = "ul. Sezamkowa 1",
            PhoneNumber = "555-123-456"
        };

        p.FirstName.Should().Be("Jan");
        p.LastName.Should().Be("Nowak");
        p.PESEL.Should().Be("90010112345");
        p.Address.Should().Be("ul. Sezamkowa 1");
        p.PhoneNumber.Should().Be("555-123-456");
    }
}
