using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chuech.ProjectSce.Core.API.Data
{
    [JsonConverter(typeof(Json))]
    public readonly struct RgbColor
    {
        public static bool TryParseHex(string input, out RgbColor color)
        {
            static bool TryParseHexNumber(ReadOnlySpan<char> span, out int value)
                => int.TryParse(span, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);

            if (input.Length == 7 && input.StartsWith("#"))
            {
                var spanInput = input.AsSpan();

                if (TryParseHexNumber(spanInput.Slice(1, 2), out var r) &&
                    TryParseHexNumber(spanInput.Slice(3, 2), out var g) &&
                    TryParseHexNumber(spanInput.Slice(5, 2), out var b))
                {
                    color = new RgbColor(r, g, b);
                    return true;
                }
            }

            color = default;
            return false;
        }

        public int Value { get; }
        public int R => (Value & 0xFF0000) >> 16;
        public int G => (Value & 0x00FF00) >> 8;
        public int B => Value & 0x0000FF;

        public RgbColor(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The given RGB value is negative.");
            }

            Value = value & 0x00FFFFFF; // remove any A part if we're given an ARGB code
        }

        public RgbColor(int r, int g, int b)
        {
            static int ClampByte(int value) => Math.Clamp(value, 0, 255);

            Value = (ClampByte(r) << 16) | (ClampByte(g) << 8) | ClampByte(b);
        }

        public override string ToString()
        {
            return $"#{Value:X6}";
        }

        private class Json : JsonConverter<RgbColor>
        {
            public override RgbColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var hexColorString = reader.GetString() ?? throw new JsonException();
                if (TryParseHex(hexColorString, out var color))
                {
                    return color;
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, RgbColor value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}