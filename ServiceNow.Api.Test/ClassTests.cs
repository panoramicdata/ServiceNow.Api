using Microsoft.Extensions.Logging;
using ServiceNow.Api.Tables;
using ServiceNow.Api.Test.Extensions;
using System.Threading.Tasks;
using Xunit;

namespace ServiceNow.Api.Test;

public class ClassTests : ServiceNowTest
{
	public ClassTests(ILogger<ClassTests> logger) : base(logger)
	{
	}

	private async Task GetItemsAsync<T>() where T : Table
	{
		// Go and get 10 items for the type we're testing
		var page = await Client.GetPageByQueryAsync<T>(0, 10);
		// Make sure that IF we have items that they have unique SysIds
		Assert.True(page?.Items.AreDistinctBy(i => i.SysId) ?? true);
	}

	[Fact]
	public async Task GetAllServers()
	{
		var allItems = await Client.GetAllByQueryAsync<Server>("firewall_status=Intranet");
		// Check that the total count matches the count of items
		Assert.NotNull(allItems);
	}

	[Fact]
	public async Task Agreement() => await GetItemsAsync<Agreement>();

#pragma warning disable xUnit1004 // Test methods should not be skipped
	[Fact(Skip = "This table doesn't seem to exist in the dev systems")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
	public async Task AmazonWebService() => await GetItemsAsync<AmazonWebService>();

	[Fact]
	public async Task Application() => await GetItemsAsync<Application>();

#pragma warning disable xUnit1004 // Test methods should not be skipped
	[Fact(Skip = "This table doesn't seem to exist in the dev systems")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
	public async Task AzureVirtualMachineInstance() => await GetItemsAsync<AzureVirtualMachineInstance>();

	[Fact]
	public async Task BusinessService() => await GetItemsAsync<BusinessService>();

	[Fact]
	public async Task CmdbCi() => await GetItemsAsync<CmdbCi>();

	[Fact]
	public async Task Company() => await GetItemsAsync<Company>();

	[Fact]
	public async Task Computer() => await GetItemsAsync<Computer>();

	[Fact]
	public async Task CostCenter() => await GetItemsAsync<CostCenter>();

	[Fact]
	public async Task Country() => await GetItemsAsync<Country>();

	[Fact]
	public async Task Department() => await GetItemsAsync<Department>();

	[Fact]
	public async Task Ec2VirtualMachineInstance() => await GetItemsAsync<Ec2VirtualMachineInstance>();

	[Fact]
	public async Task Environment() => await GetItemsAsync<Environment>();

	[Fact]
	public async Task EsxServer() => await GetItemsAsync<EsxServer>();

	[Fact]
	public async Task Hardware() => await GetItemsAsync<Hardware>();

	[Fact]
	public async Task HpuxServer() => await GetItemsAsync<HpuxServer>();

	[Fact]
	public async Task HyperVVirtualMachineInstance() => await GetItemsAsync<HyperVVirtualMachineInstance>();

	[Fact]
	public async Task Incident() => await GetItemsAsync<Incident>();

	[Fact]
	public async Task IpFirewall() => await GetItemsAsync<IpFirewall>();

	[Fact]
	public async Task IpRouter() => await GetItemsAsync<IpRouter>();

	[Fact]
	public async Task IpSwitch() => await GetItemsAsync<IpSwitch>();

	[Fact]
	public async Task KvmVirtualMachineInstance() => await GetItemsAsync<KvmVirtualMachineInstance>();

	[Fact]
	public async Task LinuxServer() => await GetItemsAsync<LinuxServer>();

	[Fact]
	public async Task LoadBalancer() => await GetItemsAsync<LoadBalancer>();

	[Fact]
	public async Task Location() => await GetItemsAsync<Location>();

	[Fact]
	public async Task NetworkGear() => await GetItemsAsync<NetworkGear>();

	[Fact]
	public async Task Printer() => await GetItemsAsync<Printer>();

	[Fact]
	public async Task Relationship() => await GetItemsAsync<Relationship>();

	[Fact]
	public async Task Server() => await GetItemsAsync<Server>();

	[Fact]
	public async Task SolarisServer() => await GetItemsAsync<SolarisServer>();

	[Fact]
	public async Task SolarisVirtualMachineInstance() => await GetItemsAsync<SolarisVirtualMachineInstance>();

	[Fact]
	public async Task UnixServer() => await GetItemsAsync<UnixServer>();

	[Fact]
	public async Task User() => await GetItemsAsync<User>();

	[Fact]
	public async Task VirtualizationServer() => await GetItemsAsync<VirtualizationServer>();

	[Fact]
	public async Task VirtualMachineInstance() => await GetItemsAsync<VirtualMachineInstance>();

	[Fact]
	public async Task WindowsServer() => await GetItemsAsync<WindowsServer>();
}