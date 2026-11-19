using AwesomeAssertions;
using System.Net;
using System.Net.Http.Headers;
using Xunit;

namespace ServiceNow.Api.Test;

/// <summary>
/// Credential-free: requests are captured by a handler and never reach a ServiceNow instance.
/// </summary>
public class AuthenticationHeaderConstructorTests
{
	private static CancellationToken Ct => TestContext.Current.CancellationToken;

	private sealed class CapturingHandler : HttpMessageHandler
	{
		public AuthenticationHeaderValue? Authorization { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Authorization = request.Headers.Authorization;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("{\"result\":{\"sys_id\":\"abc\"}}")
			});
		}
	}

	private static async Task<AuthenticationHeaderValue?> CaptureAuthorizationAsync(Func<Options, ServiceNowClient> create)
	{
		using var handler = new CapturingHandler();
		using var client = create(new Options { HttpMessageHandler = handler });

		await client.GetByIdAsync("incident", "abc", Ct);

		return handler.Authorization;
	}

	[Fact]
	public async Task AuthenticationHeaderConstructor_SendsSuppliedHeader()
	{
		var header = await CaptureAuthorizationAsync(options => new ServiceNowClient("acme", new AuthenticationHeaderValue("Bearer", "token-123"), options));

		header.Should().NotBeNull();
		header!.Scheme.Should().Be("Bearer");
		header.Parameter.Should().Be("token-123");
	}

	[Fact]
	public async Task UsernamePasswordConstructor_StillSendsBasicHeader()
	{
		var header = await CaptureAuthorizationAsync(options => new ServiceNowClient("acme", "user", "pass", options));

		header.Should().NotBeNull();
		header!.Scheme.Should().Be("Basic");
		header.Parameter.Should().Be(Convert.ToBase64String("user:pass"u8.ToArray()));
	}

	[Fact]
	public void AuthenticationHeaderConstructor_NullHeader_Throws()
	{
		var act = () => new ServiceNowClient("acme", (AuthenticationHeaderValue)null!, new Options());

		act.Should().Throw<ArgumentNullException>().WithParameterName("authenticationHeader");
	}

	[Fact]
	public void AuthenticationHeaderConstructor_NullAccount_Throws()
	{
		var act = () => new ServiceNowClient(null!, new AuthenticationHeaderValue("Bearer", "t"), new Options());

		act.Should().Throw<ArgumentNullException>().WithParameterName("account");
	}

	[Theory]
	[InlineData(null, "user", "pass", "account")]
	[InlineData("acme", null, "pass", "username")]
	[InlineData("acme", "user", null, "password")]
	public void UsernamePasswordConstructor_NullArgument_ThrowsNamingTheArgument(string? account, string? username, string? password, string expectedParameter)
	{
		var act = () => new ServiceNowClient(account!, username!, password!, new Options());

		act.Should().Throw<ArgumentNullException>().WithParameterName(expectedParameter);
	}
}
