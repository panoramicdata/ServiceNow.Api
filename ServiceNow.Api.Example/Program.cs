using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceNow.Api.Example
{
	public static class Program
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
			const string tableName = "cmdb_ci_win_server";

			// OPTIONAL: The main sysparm_query goes here.  See documention here:
			// https://docs.servicenow.com/bundle/geneva-servicenow-platform/page/integrate/inbound_rest/reference/r_TableAPI-GET.html
			// If you omit this, an unfiltered result will be returned
			const string query = "name";

			// OPTIONAL: The fields to bring back.
			// This should be set to constrain the response to ONLY the fields that you are going to process.
			// Doing so will hugely speed up your query.
			var fields = new List<string> { "sys_id", "name" };

			var jObjectResults = await serviceNowClient.GetAllByQueryAsync(
				tableName,
				query,
				fields
				).ConfigureAwait(false);

			var modelResults = jObjectResults.ConvertAll(o => o.ToObject<WinServerModel>());

			Console.WriteLine("Windows Servers:");
			foreach (var modelResult in modelResults)
			{
				Console.WriteLine($"  - {modelResult.Id}: {modelResult.Name}");
			}
		}
	}
}
