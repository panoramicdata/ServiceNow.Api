using Newtonsoft.Json;
using ServiceNow.Api.Interfaces;

namespace ServiceNow.Api.Converters;

/// <summary>
/// Custom Json converter for the ResourceLink type
/// </summary>
internal class ResourceLinkConverter : JsonConverter
{
	public override bool CanConvert(Type objectType) => typeof(IResourceLink).IsAssignableFrom(objectType);

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.StartObject || !reader.Read())
		{
			return null;
		}

		if (Activator.CreateInstance(objectType) is not IResourceLink resourceLink)
		{
			return null;
		}

		(resourceLink.Link, resourceLink.Value) = ReadLinkAndValue(reader);
		return resourceLink;
	}

	private static (string? Link, string? Value) ReadLinkAndValue(JsonReader reader)
	{
		string? link = null;
		string? value = null;
		while (reader.TokenType != JsonToken.EndObject)
		{
			var tokenName = reader.Value?.ToString();
			if (IsProperty(tokenName, "link"))
			{
				link = reader.ReadAsString();
			}
			else if (IsProperty(tokenName, "value"))
			{
				value = reader.ReadAsString();
			}
			else
			{
				_ = reader.Read();
			}
		}

		return (link, value);
	}

	private static bool IsProperty(string? tokenName, string expected)
		=> string.Equals(tokenName, expected, StringComparison.InvariantCultureIgnoreCase);

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is null)
		{
			writer.WriteNull();
		}
		else
		{
			writer.WriteValue(value.ToString());
		}
	}
}
