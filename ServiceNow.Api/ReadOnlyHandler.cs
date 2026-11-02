namespace ServiceNow.Api;

/// <summary>
/// Refuses every request other than GET, HEAD or OPTIONS while <see cref="Options.IsReadOnly"/> is set.
/// The option is read on every request.
/// </summary>
internal sealed class ReadOnlyHandler(Options options, HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (options.IsReadOnly
			&& request.Method != HttpMethod.Get
			&& request.Method != HttpMethod.Head
			&& request.Method != HttpMethod.Options)
		{
			throw new InvalidOperationException($"The client is read-only (IsReadOnly is true): {request.Method} {request.RequestUri?.AbsolutePath} is not allowed.");
		}

		return base.SendAsync(request, cancellationToken);
	}
}
