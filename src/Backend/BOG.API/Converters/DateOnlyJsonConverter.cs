using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BOG.API.Converters;

/// <summary>
/// JSON converter for DateOnly that handles both date-only strings ("2026-01-15")
/// and full ISO DateTime strings ("2026-01-15T00:00:00.000Z").
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Cannot convert null or empty string to DateOnly.");
        }

        // Try parsing as DateOnly first (yyyy-MM-dd)
        if (DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
        {
            return dateOnly;
        }

        // For ISO 8601 format from JavaScript (e.g., "2026-01-15T21:00:00.000Z")
        // Extract just the date part (first 10 characters) to avoid timezone shift
        if (value.Length >= 10 && value.Contains('T'))
        {
            var datePart = value.Substring(0, 10);
            if (DateOnly.TryParseExact(datePart, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var extractedDate))
            {
                return extractedDate;
            }
        }

        throw new JsonException($"Unable to convert \"{value}\" to DateOnly.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// JSON converter for nullable DateOnly.
/// </summary>
public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string DateFormat = "yyyy-MM-dd";

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var value = reader.GetString();

        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Try parsing as DateOnly first (yyyy-MM-dd)
        if (DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
        {
            return dateOnly;
        }

        // For ISO 8601 format from JavaScript (e.g., "2026-01-15T21:00:00.000Z")
        // Extract just the date part (first 10 characters) to avoid timezone shift
        if (value.Length >= 10 && value.Contains('T'))
        {
            var datePart = value.Substring(0, 10);
            if (DateOnly.TryParseExact(datePart, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var extractedDate))
            {
                return extractedDate;
            }
        }

        throw new JsonException($"Unable to convert \"{value}\" to DateOnly.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
