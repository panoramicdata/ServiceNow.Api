using FluentAssertions;
using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test;

public class IncidentTests : ServiceNowTest
{
	public IncidentTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
	{
	}

	[Fact]
	public async Task IncidentCrud()
	{
		// Create an incident //

		// Arrange
		var testId = Guid.NewGuid().ToString();
		var incident = new Incident
		{
			Description = "Incident description: " + testId,
			ShortDescription = "Incident short description: " + testId,
		};

		// Act
		var createdIncident = await Client.CreateAsync(incident, System.Threading.CancellationToken.None).ConfigureAwait(false);

		// Assert
		_ = createdIncident.Should().NotBeNull();
		_ = createdIncident.Description.Should().Be("Incident description: " + testId);
		_ = createdIncident.ShortDescription.Should().Be("Incident short description: " + testId);
		_ = createdIncident.SysId.Should().NotBeNullOrEmpty();
		_ = createdIncident.Number.Should().NotBeNullOrEmpty();

		Logger.LogInformation("SysId: {SysId}", createdIncident.SysId);
		Logger.LogInformation("Number: {Number}", createdIncident.Number);

		// Update an incident //

		// Arrange
		var reFetchedIncident = await Client.GetByIdAsync<Incident>(createdIncident.SysId).ConfigureAwait(false);
		_ = reFetchedIncident.Should().NotBeNull();
		_ = reFetchedIncident!.SysId.Should().Be(createdIncident.SysId);

		// Act
		reFetchedIncident.Comments = "Some new comment text " + testId;
		await Client.UpdateAsync(reFetchedIncident).ConfigureAwait(false);
		reFetchedIncident.Comments = "Some other comment text " + testId;
		await Client.UpdateAsync(reFetchedIncident).ConfigureAwait(false);

		// Assert
		var incidentAfterUpdate = await Client.GetByIdAsync<Incident>(createdIncident.SysId).ConfigureAwait(false);
		_ = incidentAfterUpdate.Should().NotBeNull();
		_ = incidentAfterUpdate!.SysId.Should().Be(createdIncident.SysId);

		// Delete the incident //
		await Client.DeleteAsync<Incident>(createdIncident.SysId).ConfigureAwait(false);
	}
}
