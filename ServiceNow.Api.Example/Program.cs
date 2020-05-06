using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ServiceNow.Api.Example
{
	public class Program
	{
		public async static Task Main(string[] args)
		{
			var account = args[0];
			var username = args[1];
			var password = args[2];

			Console.WriteLine("Lists Windows Servers");

			using var serviceNowClient = new ServiceNowClient(account, username, password, new Options());

			// MANDATORY: The table name can be obtained from this list:
			// https://docs.servicenow.com/bundle/london-platform-administration/page/administer/reference-pages/reference/r_TablesAndClasses.html
			var tableName = "cmdb_ci_win_server";

			// OPTIONAL: The main sysparm_query goes here.  See documention here:
			// https://docs.servicenow.com/bundle/geneva-servicenow-platform/page/integrate/inbound_rest/reference/r_TableAPI-GET.html
			// If you omit this, an unfiltered result will be returned
			var query = "name";

			// OPTIONAL: The fields to bring back.
			// This should be set to constrain the response to ONLY the fields that you are going to process.
			// Doing so will hugely speed up your query.
			var fields = new List<string> { "sys_id", "name" };

			var jObjectResults = await serviceNowClient.GetAllByQueryAsync(
				tableName,
				query,
				fields
				).ConfigureAwait(false);

			var modelResults = jObjectResults.Select(o => o.ToObject<WinServerModel>()).ToList();

			Console.WriteLine("Windows Servers:");
			foreach (var modelResult in modelResults)
			{
				Console.WriteLine($"  - {modelResult.Id}: {modelResult.Name}");
			}
		}
	}

	[DataContract]
	public class WinServerModel
	{
		[DataMember(Name = "sys_id")]
		public string Id { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }
	}
}
