using Newtonsoft.Json;
using ServiceNow.Api.Interfaces;
using System;

namespace ServiceNow.Api.Converters;

/// <summary>
/// Custom Json converter for the ResourceLink type
/// </summary>
internal class ResourceLinkConverter : JsonConverter
{
	public override bool CanConvert(Type objectType) => typeof(IResourceLink).IsAssignableFrom(objectType);

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartObject)
		{

			if (!reader.Read())
			{
				return null;
			}

			string? link = null;
			string? value = null;
			while (reader.TokenType != JsonToken.EndObject)
			{
				if (reader.Value is not null && reader.Value.ToString().Equals("link", StringComparison.InvariantCultureIgnoreCase))
				{
					link = reader.ReadAsString();
				}
				else if (reader.Value is not null && reader.Value.ToString().Equals("value", StringComparison.InvariantCultureIgnoreCase))
				{
					value = reader.ReadAsString();
				}
				else
				{
					_ = reader.Read();
				}
			}

			var resourceLink = (IResourceLink)Activator.CreateInstance(objectType);
			resourceLink.Link = link;
			resourceLink.Value = value;
			return resourceLink;
		}

		return null;
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
