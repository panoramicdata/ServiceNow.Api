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
		public async void GetAllServers()
		{
			var allItems = await Client.GetAllByQueryAsync<Server>("firewall_status=Intranet").ConfigureAwait(false);
			// Check that the total count matches the count of items
			Assert.NotNull(allItems);
		}

		[Fact]
		public async void Agreement() => await GetItemsAsync<Agreement>().ConfigureAwait(false);

#pragma warning disable xUnit1004 // Test methods should not be skipped
		[Fact(Skip = "This table doesn't seem to exist in the dev systems")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
		public async void AmazonWebService() => await GetItemsAsync<AmazonWebService>().ConfigureAwait(false);

		[Fact]
		public async void Application() => await GetItemsAsync<Application>().ConfigureAwait(false);

#pragma warning disable xUnit1004 // Test methods should not be skipped
		[Fact(Skip = "This table doesn't seem to exist in the dev systems")]
#pragma warning restore xUnit1004 // Test methods should not be skipped
		public async void AzureVirtualMachineInstance() => await GetItemsAsync<AzureVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async void BusinessService() => await GetItemsAsync<BusinessService>().ConfigureAwait(false);

		[Fact]
		public async void CmdbCi() => await GetItemsAsync<CmdbCi>().ConfigureAwait(false);

		[Fact]
		public async void Company() => await GetItemsAsync<Company>().ConfigureAwait(false);

		[Fact]
		public async void Computer() => await GetItemsAsync<Computer>().ConfigureAwait(false);

		[Fact]
		public async void CostCenter() => await GetItemsAsync<CostCenter>().ConfigureAwait(false);

		[Fact]
		public async void Country() => await GetItemsAsync<Country>().ConfigureAwait(false);

		[Fact]
		public async void Department() => await GetItemsAsync<Department>().ConfigureAwait(false);

		[Fact]
		public async void Ec2VirtualMachineInstance() => await GetItemsAsync<Ec2VirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async void Environment() => await GetItemsAsync<Environment>().ConfigureAwait(false);

		[Fact]
		public async void EsxServer() => await GetItemsAsync<EsxServer>().ConfigureAwait(false);

		[Fact]
		public async void Hardware() => await GetItemsAsync<Hardware>().ConfigureAwait(false);

		[Fact]
		public async void HpuxServer() => await GetItemsAsync<HpuxServer>().ConfigureAwait(false);

		[Fact]
		public async void HyperVVirtualMachineInstance() => await GetItemsAsync<HyperVVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async void Incident() => await GetItemsAsync<Incident>().ConfigureAwait(false);

		[Fact]
		public async void IpFirewall() => await GetItemsAsync<IpFirewall>().ConfigureAwait(false);

		[Fact]
		public async void IpRouter() => await GetItemsAsync<IpRouter>().ConfigureAwait(false);

		[Fact]
		public async void IpSwitch() => await GetItemsAsync<IpSwitch>().ConfigureAwait(false);

		[Fact]
		public async void KvmVirtualMachineInstance() => await GetItemsAsync<KvmVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async void LinuxServer() => await GetItemsAsync<LinuxServer>().ConfigureAwait(false);

		[Fact]
		public async void LoadBalancer() => await GetItemsAsync<LoadBalancer>().ConfigureAwait(false);

		[Fact]
		public async void Location() => await GetItemsAsync<Location>().ConfigureAwait(false);

		[Fact]
		public async void NetworkGear() => await GetItemsAsync<NetworkGear>().ConfigureAwait(false);

		[Fact]
		public async void Printer() => await GetItemsAsync<Printer>().ConfigureAwait(false);

		[Fact]
		public async void Relationship() => await GetItemsAsync<Relationship>().ConfigureAwait(false);

		[Fact]
		public async void Server() => await GetItemsAsync<Server>().ConfigureAwait(false);

		[Fact]
		public async void SolarisServer() => await GetItemsAsync<SolarisServer>().ConfigureAwait(false);

		[Fact]
		public async void SolarisVirtualMachineInstance() => await GetItemsAsync<SolarisVirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async void UnixServer() => await GetItemsAsync<UnixServer>().ConfigureAwait(false);

		[Fact]
		public async void User() => await GetItemsAsync<User>().ConfigureAwait(false);

		[Fact]
		public async void VirtualizationServer() => await GetItemsAsync<VirtualizationServer>().ConfigureAwait(false);

		[Fact]
		public async void VirtualMachineInstance() => await GetItemsAsync<VirtualMachineInstance>().ConfigureAwait(false);

		[Fact]
		public async void WindowsServer() => await GetItemsAsync<WindowsServer>().ConfigureAwait(false);
	}
}