namespace ServiceNow.Api.Test;

public class TestConfiguration
{
	public required string ServiceNowAccount { get; set; }
	public required string ServiceNowUsername { get; set; }
	public required string ServiceNowPassword { get; set; }
	public required string? ServiceNowEnvironment { get; set; }
}
