# ServiceNow.Api

[![Nuget](https://img.shields.io/nuget/v/ServiceNow.Api)](https://www.nuget.org/packages/ServiceNow.Api/)
[![Nuget](https://img.shields.io/nuget/dt/ServiceNow.Api)](https://www.nuget.org/packages/ServiceNow.Api/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/24dcf171dd8540a192635ed46eeb1ea3)](https://www.codacy.com/gh/panoramicdata/ServiceNow.Api/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=panoramicdata/ServiceNow.Api&amp;utm_campaign=Badge_Grade)

# Usage

To create a simple command line app that uses the ServiceNow REST API:

1. Create a .NET Core 7.0 Console project in Visual Studio
2. Ensure that you have specified &lt;LangVersion&gt;latest&lt;/LangVersion&gt; in the csproj file, e.g.:
```` xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>net7.0</TargetFramework>
		<LangVersion>latest</LangVersion>
		<Nullable>enable</Nullable>
		<AnalysisMode>Recommended</AnalysisMode>
		<AnalysisLevel>latest-recommended</AnalysisLevel>
	</PropertyGroup>

	<ItemGroup>
		<Folder Include="Properties\" />
	</ItemGroup>

	<ItemGroup>
		<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
		<PackageReference Include="ServiceNow.Api" Version="1.2.*" />
	</ItemGroup>

</Project>

````
3. Edit Program.cs to be similar to the following:

```` C#
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ServiceNow.Api.Example;

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

		// OPTIONAL: The main sysparm_query goes here.  See documentation here:
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

[DataContract]
public class WinServerModel
{
	[DataMember(Name = "sys_id")]
	public string Id { get; set; }

	[DataMember(Name = "name")]
	public string Name { get; set; }
}
````
