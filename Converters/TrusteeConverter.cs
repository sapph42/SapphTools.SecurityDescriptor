using System.Text.Json;
using System.Text.Json.Serialization;

namespace SapphTools.SecurityDescriptor.Converters;
public class TrusteeConverter : JsonConverter<Trustee> {
    public override Trustee? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        Trustee? trustee = null;
        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                if (trustee is null) {
                    throw new JsonException("Trustee did not contain a valid trustee string.");
                }
                return trustee;
            }
            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }
            if (reader.ValueTextEquals(nameof(Trustee.Sid))) {
                reader.Read();
                if (reader.GetString() is string sid) {
                    try {
                        trustee = Trustee.Construct(sid);
                    } catch {
                        throw new JsonException("Invalid Trustee SID");
                    }
                }
            }
        }
        throw new JsonException("Unexpected end of JSON");
    }
    public override void Write(Utf8JsonWriter writer, Trustee value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WritePropertyName(nameof(Trustee.Sid));
        writer.WriteStringValue(value.Sid);
        writer.WriteEndObject();
    }
}
