using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using ServiceNow.Api;
using ServiceNow.Api.Tables;
using System.Net;
using System.Text;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Unit tests for the <see cref="ServiceNowClient"/> that run against a stub message handler
/// and therefore need no credentials and no network access.
/// </summary>
public class ServiceNowClientStubbedTests
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	/// <summary>
	/// A message handler that returns one fixed response for every request.
	/// </summary>
	private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			=> Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
	}

	[Fact]
	public async Task GetByIdAsync_Table_Type_ReturnsDeserializedItem()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{\"sys_id\":\"abc123\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.GetByIdAsync<WindowsServer>("abc123", Ct);

		result.Should().NotBeNull();
		result!.SysId.Should().Be("abc123");
	}

	[Fact]
	public async Task GetByIdAsync_TableName_Type_ReturnsDeserializedItem()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{\"sys_id\":\"xyz789\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.GetByIdAsync("cmdb_ci_win_server", "xyz789", Ct);

		result.Should().NotBeNull();
		result!["sys_id"]!.ToString().Should().Be("xyz789");
	}

	[Fact]
	public async Task GetMetaForClassAsync_ReturnsMetaData()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{\"name\":\"cmdb_ci_win_server\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.GetMetaForClassAsync("cmdb_ci_win_server", Ct);

		result.Should().NotBeNull();
		result!.Item.Should().NotBeNull();
		result.Item!.Name.Should().Be("cmdb_ci_win_server");
	}

	[Fact]
	public async Task GetPageByQueryAsync_Success_ParsesItems()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":[{\"sys_id\":\"one\"},{\"sys_id\":\"two\"}]}");
		using var client = new ServiceNowClient(handler);

		var page = await client.GetPageByQueryAsync(0, 10, "cmdb_ci_win_server", null, ["sys_id", "sys_created_on"], null, Ct);

		page.Should().NotBeNull();
		page!.Items.Should().HaveCount(2);
		page.Items[0]["sys_id"]!.ToString().Should().Be("one");
	}

	[Fact]
	public async Task CreateAsync_TableName_Type_PostsAndReturnsItem()
	{
		using var handler = new StubHandler(HttpStatusCode.Created, "{\"result\":{\"sys_id\":\"new1\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.CreateAsync("cmdb_ci_win_server", new JObject { ["name"] = "Test" }, Ct);

		result.Should().NotBeNull();
		result!["sys_id"]!.ToString().Should().Be("new1");
	}

	[Fact]
	public async Task UpdateAsync_TableName_Type_PutsAndReturnsItem()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{\"sys_id\":\"upd1\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.UpdateAsync("cmdb_ci_win_server", new JObject { ["sys_id"] = "upd1", ["name"] = "Updated" }, Ct);

		result.Should().NotBeNull();
		result!["sys_id"]!.ToString().Should().Be("upd1");
	}

	[Fact]
	public async Task PatchAsync_TableName_Type_PatchesAndReturnsItem()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{\"sys_id\":\"patch1\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.PatchAsync("cmdb_ci_win_server", new JObject { ["sys_id"] = "patch1", ["name"] = "Patched" }, Ct);

		result.Should().NotBeNull();
		result!["sys_id"]!.ToString().Should().Be("patch1");
	}

	[Fact]
	public async Task DeleteAsync_TableName_Type_SucceedsOn200()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{}");
		using var client = new ServiceNowClient(handler);

		await client.DeleteAsync("cmdb_ci_win_server", "del1", Ct);

		handler.Should().NotBeNull();
	}

	[Fact]
	public async Task UpdateAsync_Table_Type_PutsAndSucceeds()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{}");
		using var client = new ServiceNowClient(handler);

		var server = new WindowsServer { SysId = "srv1", Name = "SRV1" };
		await client.UpdateAsync(server, Ct);

		handler.Should().NotBeNull();
	}

	[Fact]
	public async Task DeleteAsync_Table_Type_SucceedsOn200()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{}");
		using var client = new ServiceNowClient(handler);

		await client.DeleteAsync<WindowsServer>("srv1", Ct);

		handler.Should().NotBeNull();
	}

	[Fact]
	public async Task GetLinkedEntityAsync_ParsesAndReturnsItem()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":{\"sys_id\":\"linked\"}}");
		using var client = new ServiceNowClient(handler);

		var result = await client.GetLinkedEntityAsync("https://test.service-now.com/api/now/table/cmdb_ci_win_server/linked", ["sys_id"], Ct);

		result.Should().NotBeNull();
		result!["sys_id"]!.ToString().Should().Be("linked");
	}

	[Fact]
	public async Task GetAttachmentsAsync_TableName_Type_ReturnsAttachments()
	{
		using var handler = new StubHandler(HttpStatusCode.OK, "{\"result\":[{\"sys_id\":\"att1\",\"file_name\":\"a.txt\"}]}");
		using var client = new ServiceNowClient(handler);

		var result = await client.GetAttachmentsAsync("cmdb_ci_win_server", "srv1", Ct);

		result.Should().NotBeNull();
		result!.Should().HaveCount(1);
		result[0].SysId.Should().Be("att1");
	}

	[Fact]
	public void Constructor_NullAccount_Throws()
	{
		var act = () => new ServiceNowClient(null!, "user", "pass", (Options?)null);

		act.Should().Throw<ArgumentNullException>();
	}
}
