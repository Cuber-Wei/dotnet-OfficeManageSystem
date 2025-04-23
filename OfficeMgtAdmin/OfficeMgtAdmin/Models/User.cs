using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OfficeMgtAdmin.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [Required]
        [JsonPropertyName("userId")]
        public required string UserId { get; set; }

        [Required]
        [JsonPropertyName("userName")]
        public required string UserName { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public required string Password { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("birthDate")]
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }
    }

    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString(), out DateTime date))
                {
                    return date;
                }
            }
            return DateTime.Now;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }
} 