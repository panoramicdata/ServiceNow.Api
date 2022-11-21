using ServiceNow.Api.Tables;
using ServiceNow.Api.Test.Extensions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ServiceNow.Api.Test
{
	public class ClassTests : ServiceNowTest
	{
		public ClassTests(ITestOutputHelper output) : base(output)
		{
		}

		private async Task GetItemsAsync<T>() where T : Table
		{
			// Go and get 10 items for the type we're testing
			var page = await Client.GetPageByQueryAsync<T>(0, 10).ConfigureAwait(false);
			// Make sure that IF we have items that they have unique SysIds
			Assert.True(page?.Items.AreDistinctBy(i => i.SysId) ?? true);
		}

		[Fact]
		public async Task GetAllServers()
		{
			var allItems = await Client.GetAllByQueryAsync<Server>("firewall_status=Intranet").ConfigureAwait(false);
			// Check that the total count matches the count of items
			Assert.NotNull(allItems);
		}

		[Fact]
		public async Task Agreement() => await GetItemsAsync<Agreement>().ConfigureAwait(false);

#pragma warning disable xUnit1004 // Test methods should not be skipped
		[Fact(Skip = "This table doesn't seem to exist in the dev systems")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
		public async Task AmazonWebService() => await GetItemsAsync<AmazonWebService>().ConfigureAwait(false);

		[Fact]
		public async Task Application() => await GetItemsAsync<Application>().ConfigureAwait(false);

#pragma warning disable xUnit1004 // Test methods should not be skipped
		[Fact(Skip = "This table doesn't seem to exist in the dev systems")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
		public async Task AzureVirtualMachineInstance() => await GetItemsAsync<AzureVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async Task BusinessService() => await GetItemsAsync<BusinessService>().ConfigureAwait(false);

		[Fact]
		public async Task CmdbCi() => await GetItemsAsync<CmdbCi>().ConfigureAwait(false);

		[Fact]
		public async Task Company() => await GetItemsAsync<Company>().ConfigureAwait(false);

		[Fact]
		public async Task Computer() => await GetItemsAsync<Computer>().ConfigureAwait(false);

		[Fact]
		public async Task CostCenter() => await GetItemsAsync<CostCenter>().ConfigureAwait(false);

		[Fact]
		public async Task Country() => await GetItemsAsync<Country>().ConfigureAwait(false);

		[Fact]
		public async Task Department() => await GetItemsAsync<Department>().ConfigureAwait(false);

		[Fact]
		public async Task Ec2VirtualMachineInstance() => await GetItemsAsync<Ec2VirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async Task Environment() => await GetItemsAsync<Environment>().ConfigureAwait(false);

		[Fact]
		public async Task EsxServer() => await GetItemsAsync<EsxServer>().ConfigureAwait(false);

		[Fact]
		public async Task Hardware() => await GetItemsAsync<Hardware>().ConfigureAwait(false);

		[Fact]
		public async Task HpuxServer() => await GetItemsAsync<HpuxServer>().ConfigureAwait(false);

		[Fact]
		public async Task HyperVVirtualMachineInstance() => await GetItemsAsync<HyperVVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async Task Incident() => await GetItemsAsync<Incident>().ConfigureAwait(false);

		[Fact]
		public async Task IpFirewall() => await GetItemsAsync<IpFirewall>().ConfigureAwait(false);

		[Fact]
		public async Task IpRouter() => await GetItemsAsync<IpRouter>().ConfigureAwait(false);

		[Fact]
		public async Task IpSwitch() => await GetItemsAsync<IpSwitch>().ConfigureAwait(false);

		[Fact]
		public async Task KvmVirtualMachineInstance() => await GetItemsAsync<KvmVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async Task LinuxServer() => await GetItemsAsync<LinuxServer>().ConfigureAwait(false);

		[Fact]
		public async Task LoadBalancer() => await GetItemsAsync<LoadBalancer>().ConfigureAwait(false);

		[Fact]
		public async Task Location() => await GetItemsAsync<Location>().ConfigureAwait(false);

		[Fact]
		public async Task NetworkGear() => await GetItemsAsync<NetworkGear>().ConfigureAwait(false);

		[Fact]
		public async Task Printer() => await GetItemsAsync<Printer>().ConfigureAwait(false);

		[Fact]
		public async Task Relationship() => await GetItemsAsync<Relationship>().ConfigureAwait(false);

		[Fact]
		public async Task Server() => await GetItemsAsync<Server>().ConfigureAwait(false);

		[Fact]
		public async Task SolarisServer() => await GetItemsAsync<SolarisServer>().ConfigureAwait(false);

		[Fact]
		public async Task SolarisVirtualMachineInstance() => await GetItemsAsync<SolarisVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async Task UnixServer() => await GetItemsAsync<UnixServer>().ConfigureAwait(false);

		[Fact]
		public async Task User() => await GetItemsAsync<User>().ConfigureAwait(false);

		[Fact]
		public async Task VirtualizationServer() => await GetItemsAsync<VirtualizationServer>().ConfigureAwait(false);

		[Fact]
		public async Task VirtualMachineInstance() => await GetItemsAsync<VirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async Task WindowsServer() => await GetItemsAsync<WindowsServer>().ConfigureAwait(false);
	}
}