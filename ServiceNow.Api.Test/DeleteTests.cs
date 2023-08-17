using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test;

public class DeleteTests : ServiceNowTest
{
	public DeleteTests(ITestOutputHelper output) : base(output, "appsettings.edinburghAirport.json", new Options { ValidateCountItemsReturned = true, ValidateCountItemsReturnedTolerance = 0, PageSize = 1000 })
	{
	}

	[Fact]
	public async Task Delete_FromList_Succeeds()
	{
		foreach (var sysIdToPhrase in IncidentSysIds)
		{
			var sysId = sysIdToPhrase.Key;
			var phrase = sysIdToPhrase.Value;

			var incidents = await Client
				.GetAllByQueryAsync("incident", $"sys_id={sysId}", default)
				.ConfigureAwait(false);

			_ = incidents.SingleOrDefault() ?? throw new Exception($"Incident {sysId} not found");

			List<JObject> workNotes;
			try
			{
				workNotes = await Client
					.GetAllByQueryAsync("sys_journal_field", $"element=work_notes^name=incident^sys_created_on>2022-05-13^sys_created_by.name==ConnectMagic^valueSTARTSWITHAutoTask Time Entry: ^element_id={sysId}", default)
					.ConfigureAwait(false);
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
					await Client.DeleteAsync("sys_journal_field", sysIdToDelete, default).ConfigureAwait(false);
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
	}
	;
}