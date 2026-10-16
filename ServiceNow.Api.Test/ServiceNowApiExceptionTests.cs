using AwesomeAssertions;
using ServiceNow.Api.Exceptions;
using Xunit;

namespace ServiceNow.Api.Test;

public class ServiceNowApiExceptionTests
{
	[Fact]
	public void ParameterlessConstructor_CreatesException()
	{
		var exception = new ServiceNowApiException();

		exception.Should().BeOfType<ServiceNowApiException>();
		exception.InnerException.Should().BeNull();
	}

	[Fact]
	public void MessageOnlyConstructor_SetsMessage()
	{
		var exception = new ServiceNowApiException("Something went wrong");

		exception.Message.Should().Be("Something went wrong");
		exception.InnerException.Should().BeNull();
	}

	[Fact]
	public void MessageAndInnerExceptionConstructor_SetsBoth()
	{
		var inner = new InvalidOperationException("root cause");
		var exception = new ServiceNowApiException("wrapper", inner);

		exception.Message.Should().Be("wrapper");
		exception.InnerException.Should().BeSameAs(inner);
	}
}
