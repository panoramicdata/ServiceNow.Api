using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class IncidentTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	[Fact(Skip = "Cannot perform Create, Update or Delete in all test systems")]
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
		var createdIncident = await Client.CreateAsync(incident, CancellationToken);

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
		var reFetchedIncident = await Client.GetByIdAsync<Incident>(createdIncident.SysId, CancellationToken);
		_ = reFetchedIncident.Should().NotBeNull();
		_ = reFetchedIncident!.SysId.Should().Be(createdIncident.SysId);

		// Act
		reFetchedIncident.Comments = "Some new comment text " + testId;
		await Client.UpdateAsync(reFetchedIncident, CancellationToken);
		reFetchedIncident.Comments = "Some other comment text " + testId;
		await Client.UpdateAsync(reFetchedIncident, CancellationToken);

		// Assert
		var incidentAfterUpdate = await Client.GetByIdAsync<Incident>(createdIncident.SysId, CancellationToken);
		_ = incidentAfterUpdate.Should().NotBeNull();
		_ = incidentAfterUpdate!.SysId.Should().Be(createdIncident.SysId);

		// Delete the incident //
		await Client.DeleteAsync<Incident>(createdIncident.SysId, CancellationToken);
	}
}
