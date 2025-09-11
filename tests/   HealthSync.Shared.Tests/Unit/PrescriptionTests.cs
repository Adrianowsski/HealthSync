using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentAssertions;
using HealthSync.Shared.Models;
using Xunit;

namespace HealthSync.Shared.Tests.Unit;

public class PrescriptionTests
{
    [Fact]
    public void AccessCode_should_be_6_uppercase_alphanumeric_and_unique_per_instance()
    {
        var p1 = new Prescription();
        var p2 = new Prescription();

        p1.AccessCode.Should().HaveLength(6)
                     .And.MatchRegex("^[A-Z0-9]{6}$");
        p2.AccessCode.Should().HaveLength(6)
                     .And.MatchRegex("^[A-Z0-9]{6}$");

        p1.AccessCode.Should().NotBe(p2.AccessCode);
    }

    [Fact]
    public void IssuedAt_should_be_close_to_UtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var p = new Prescription();
        var after = DateTime.UtcNow.AddSeconds(2);

        p.IssuedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Validation_should_fail_when_required_strings_are_missing()
    {
        // Uwaga: [Required] na int nie złapie 0 — to celowo pokazuje ewentualną lukę walidacyjną w modelu
        var p = new Prescription(); // stringi są puste => powinno zwrócić błędy dla pól tekstowych

        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(p);
        var isValid = Validator.TryValidateObject(p, ctx, results, validateAllProperties: true);

        isValid.Should().BeFalse();
        results.Select(r => r.MemberNames.FirstOrDefault())
               .Should().Contain(new[]
               {
                   nameof(Prescription.MedicationName),
                   nameof(Prescription.Dosage),
                   nameof(Prescription.Duration)
               });
    }
}
