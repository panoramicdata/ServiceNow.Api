using FluentAssertions;
using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
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
			var createdIncident = await Client.CreateAsync(incident).ConfigureAwait(false);

			// Assert
			createdIncident.Should().NotBeNull();
			createdIncident.Description.Should().Be("Incident description: " + testId);
			createdIncident.ShortDescription.Should().Be("Incident short description: " + testId);
			createdIncident.SysId.Should().NotBeNullOrEmpty();
			createdIncident.Number.Should().NotBeNullOrEmpty();

			Logger.LogInformation("SysId: " + createdIncident.SysId);
			Logger.LogInformation("Number: " + createdIncident.Number);

			// Update an incident //

			// Arrange
			var reFetchedIncident = await Client.GetByIdAsync<Incident>(createdIncident.SysId).ConfigureAwait(false);
			reFetchedIncident.Should().NotBeNull();
			reFetchedIncident!.SysId.Should().Be(createdIncident.SysId);

			// Act
			reFetchedIncident.Comments = "Some new comment text " + testId;
			await Client.UpdateAsync(reFetchedIncident).ConfigureAwait(false);
			reFetchedIncident.Comments = "Some other comment text " + testId;
			await Client.UpdateAsync(reFetchedIncident).ConfigureAwait(false);

			// Assert
			var incidentAfterUpdate = await Client.GetByIdAsync<Incident>(createdIncident.SysId).ConfigureAwait(false);
			incidentAfterUpdate.Should().NotBeNull();
			incidentAfterUpdate!.SysId.Should().Be(createdIncident.SysId);

			// Delete the incident //
			await Client.DeleteAsync<Incident>(createdIncident.SysId).ConfigureAwait(false);
		}
	}
}
