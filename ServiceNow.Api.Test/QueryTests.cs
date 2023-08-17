﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test;

public class QueryTests : ServiceNowTest
{
	public QueryTests(ITestOutputHelper output) : base(output, "appsettings.ntt.dev.json", new Options { ValidateCountItemsReturned = true, ValidateCountItemsReturnedTolerance = 0, PageSize = 1000 })
	{
	}

	[Fact]
	public async Task InternalPagingTestAsync()
	{
		// This test fails if not ordering by ORDERBYsys_id
		const string query = "u_nameISNOTEMPTY";
		var fieldList = new List<string> { "sys_id", "sys_updated_on", "u_name", "u_value", "sys_created_on" };
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync("u_ci_property", query, fieldList, extraQueryString, default).ConfigureAwait(false);
		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.True(result[0].ContainsKey("sys_id"));

		// There should only be the properties requested, even though sys_id and sys_created_on are used internally for paging
		// (sys_created_on is used unless the paging field has been set in options passed to the client, which would be used instead)
		_ = result[0].Properties().Select(p => p.Name).ToList();

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation("Found {DupesCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count,
			result.Count,
			unique.Count);

		Assert.Empty(dupes);
	}

	[Fact]
	public async Task PagingTestAsync()
	{
		// This test fails if not ordering by ORDERBYsys_id
		const string? query = null;
		var fieldList = new List<string>();
		const string? extraQueryString = null;

		var result = await Client.GetAllByQueryAsync("u_ci_property", query, fieldList, extraQueryString, default).ConfigureAwait(false);
		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.True(result[0].ContainsKey("sys_id"));
		// Expecting the u_value field to be present as we didn't limit the fields to be retrieved
		Assert.True(result[0].ContainsKey("u_value"));

		// Check for dupes
		var dupes = result.GroupBy(ci => ci["sys_id"]).Where(g => g.Count() > 1).Select(g => new { Id = g.First()["sys_id"], Count = g.Count() }).ToList();

		var unique = result.GroupBy(ci => ci["sys_id"]).Select(ci => ci.First()).ToList();

		Logger.LogInformation("Found {DupesCount} dupes - total retrieved = {ResultCount} - unique = {UniqueCount}",
			dupes.Count, result.Count, unique.Count);

		Assert.Empty(dupes);
	}

	[Fact]
	public async Task Incident()
	{
		var page = await Client.GetPageByQueryAsync<Incident>(0, 10).ConfigureAwait(false);
		_ = page.Should().NotBeNull();
		_ = page.Items.Should().NotBeNullOrEmpty();
		var firstItem = page.Items[0];

		// Re-fetch using SysId
		var refetchById = await Client.GetByIdAsync<Incident>(firstItem.SysId).ConfigureAwait(false);
		_ = refetchById.Should().NotBeNull();
		_ = firstItem.SysId.Should().Equals(refetchById!.SysId);

		// Re-fetch using SysId
		var refetchByQuery = (await Client.GetPageByQueryAsync<Incident>(0, 1, $"sys_id={firstItem.SysId}").ConfigureAwait(false))?.Items.FirstOrDefault();
		_ = refetchByQuery.Should().NotBeNull();
		_ = firstItem.SysId.Should().Equals(refetchByQuery!.SysId);
	}

	[Fact]
	public async Task GetLinkedEntity_Succeeds()
	{
		var server = (await Client.GetAllByQueryAsync("cmdb_ci_win_server", null, new List<string> { "sys_id", "name", "company" }).ConfigureAwait(false)).FirstOrDefault();
		_ = server.Should().NotBeNull();
		_ = server!["company"].Should().NotBeNull();
		_ = server!["company"]!["link"].Should().NotBeNull();

		var companyLink = server!["company"]!["link"]!.ToString();

		var company = await Client.GetLinkedEntityAsync(companyLink, new List<string> { "name" }).ConfigureAwait(false);

		_ = company.Should().NotBeNull();
		_ = company!["name"].Should().NotBeNull();
	}

	[Fact]
	public async Task GetRelationshipByChild_Succeeds()
	{
		var firstTen = await Client
			.GetAllByQueryAsync("cmdb_rel_ci", extraQueryString: "sysparm_limit=10")
			.ConfigureAwait(false);

		_ = firstTen.Should().NotBeNullOrEmpty();

		var first = firstTen[0];
		var firstChild = first["child"];
		_ = firstChild.Should().NotBeNull();

		var childSysId = firstChild!["value"];
		_ = childSysId.Should().NotBeNull();

		var list = await Client
			.GetAllByQueryAsync("cmdb_rel_ci", $"child={childSysId}")
			.ConfigureAwait(false);

		var relationship = list.FirstOrDefault();
		_ = relationship.Should().NotBeNull();
	}
}