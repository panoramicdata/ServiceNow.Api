using Newtonsoft.Json;
using ServiceNow.Api.Tables;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace ServiceNow.Api.Converters;
internal class ResourceLinkConverter : JsonConverter
{
	public override bool CanConvert(Type objectType) => throw new NotImplementedException();

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartObject)
		{

			if (!reader.Read()) return null;
			string? link = null;
			string? value = null;
			while (reader.TokenType != JsonToken.EndObject)
			{
				if (reader.Value is not null && reader.Value.ToString().Equals("link", StringComparison.InvariantCultureIgnoreCase))
					link = reader.ReadAsString();
				else if (reader.Value is not null && reader.Value.ToString().Equals("value", StringComparison.InvariantCultureIgnoreCase))
					value = reader.ReadAsString();

				_ = reader.Read();
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
			writer.WriteNull();
		else
			writer.WriteValue(value.ToString());
	}
}
