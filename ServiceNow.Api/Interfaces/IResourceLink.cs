namespace ServiceNow.Api.Interfaces;

public interface IResourceLink
{
	string? Link { get; set; } // REST URL for child record
	string? Value { get; set; } // Reference to the child record (sys_id)
}
