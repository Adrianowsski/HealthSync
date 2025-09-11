using System;
using FluentAssertions;
using HealthSync.Shared.Models;
using Xunit;

namespace HealthSync.Shared.Tests.Unit;

public class AppointmentTests
{
    [Fact]
    public void Defaults_should_set_status_scheduled_and_empty_prescriptions()
    {
        var a = new Appointment();

        a.Status.Should().Be("scheduled");
        a.Prescriptions.Should().NotBeNull();
        a.Prescriptions.Should().BeEmpty();
    }

    [Fact]
    public void DisplayName_should_combine_patient_name_and_date()
    {
        var a = new Appointment
        {
            AppointmentDate = new DateTime(2025, 1, 20, 14, 30, 0),
            PatientProfile = new PatientProfile { FirstName = "Anna", LastName = "Kowalska" }
        };

        var expectedDate = a.AppointmentDate.ToString("g");
        a.DisplayName.Should().Contain("Anna")
                      .And.Contain("Kowalska")
                      .And.EndWith(expectedDate);
    }
}
