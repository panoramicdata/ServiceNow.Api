using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Tests for delete operations.  These tests are designed to be run against a test system where it is safe to perform deletes, and they will attempt to delete specific records based on their sys_id.  The tests will log the progress of the deletes, and any errors that occur during the process.  Note that these tests are skipped by default, as they cannot be run against all test systems without potentially causing issues.  If you want to run these tests, you should ensure that you have a safe test environment and that the records being deleted are appropriate for deletion in that environment.
/// </summary>
/// <param name="iTestOutputHelper">The xUnit output helper for the current test.</param>
/// <param name="fixture">The shared test fixture.</param>
public class DeleteTests(ITestOutputHelper iTestOutputHelper, Fixture fixture) : ServiceNowTest(iTestOutputHelper, fixture)
{
	/// <summary>
	/// Tests deleting records from a list of sys_ids.  For each sys_id, the test will attempt to retrieve the corresponding incident record and its related work notes, filter the work notes based on a specific phrase, and then delete all but the first relevant work note.  The test will log the progress of the deletes and any errors that occur during the process.
	/// </summary>
	[Fact(Skip = "Cannot perform deletes in all test systems")]
	public async Task Delete_FromList_Succeeds()
	{
		foreach (var sysIdToPhrase in IncidentSysIds)
		{
			var sysId = sysIdToPhrase.Key;
			var phrase = sysIdToPhrase.Value;

			var incidents = await Client
				.GetAllByQueryAsync("incident", $"sys_id={sysId}", cancellationToken: CancellationToken)
				;

			var incident = incidents.SingleOrDefault() ?? throw new InvalidOperationException($"Incident {sysId} not found");

			List<JObject> workNotes;
			try
			{
				workNotes = await Client
					.GetAllByQueryAsync("sys_journal_field", $"element=work_notes^name=incident^sys_created_on>2022-05-13^sys_created_by.name==ConnectMagic^valueSTARTSWITHAutoTask Time Entry: ^element_id={sysId}", cancellationToken: CancellationToken)
					;
			}
			catch (Exception e)
			{
				Logger.LogError(e, "Failed to get work notes for {SysId}: {Message}", sysId, e.Message);
				throw;
			}

			var relevantWorkNotes = workNotes.Where(x => x["value"]?.Value<string>()?.Contains(phrase) ?? false).ToList();

			var index = 0;
			var count = relevantWorkNotes.Count;
			foreach (var relevantWorkNote in relevantWorkNotes.Skip(1))
			{
				try
				{
					Logger.LogInformation("{Index} of {Count}", ++index, count);
					var sysIdToDelete = relevantWorkNote["sys_id"]?.Value<string>() ?? throw new InvalidOperationException("No sys_id");
					await Client.DeleteAsync("sys_journal_field", sysIdToDelete, CancellationToken);
					Logger.LogInformation("Done.");
				}
				catch (Exception e)
				{
					Logger.LogError(e, "Failed to delete {SysId}: {Message}", sysId, e.Message);
				}
			}
		}
	}

	private static Dictionary<string, string> IncidentSysIds { get; set; } = new()
	{
		{ "ee679ffa9701e110c65770771153af23", "Gi3/0/24 E105/411 * cem 01d connected 1087 a-full a-100 10/100/1000BaseTX" },
		{ "2b39a31c1b4ae5500cb88551f54bcbf9", "E040AB05 is patched to G103-a-fe-c9k3-1 Gi1/0/17" },
	};
}