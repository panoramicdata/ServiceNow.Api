using System;

namespace ServiceNow.Api.DiagCli.Exceptions;

[Serializable]
public class ConfigurationException : Exception
{
	public ConfigurationException()
	{
	}

	public ConfigurationException(string message) : base(message)
	{
	}

	public ConfigurationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}