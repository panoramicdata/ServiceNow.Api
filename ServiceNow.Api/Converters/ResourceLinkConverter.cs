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
		if (reader.TokenType != JsonToken.StartObject)
		{
			return null;
		}

		if (!reader.Read())
		{
			return null;
		}

		var resourceLink = (IResourceLink?)Activator.CreateInstance(objectType);
		if (resourceLink is null)
		{
			return null;
		}

		string? link = null;
		string? value = null;
		while (reader.TokenType != JsonToken.EndObject)
		{
			var tokenName = reader.Value?.ToString();
			if (string.Equals(tokenName, "link", StringComparison.InvariantCultureIgnoreCase))
			{
				link = reader.ReadAsString();
			}
			else if (string.Equals(tokenName, "value", StringComparison.InvariantCultureIgnoreCase))
			{
				value = reader.ReadAsString();
			}
			else
			{
				_ = reader.Read();
			}
		}

		resourceLink.Link = link;
		resourceLink.Value = value;
		return resourceLink;
	}

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
