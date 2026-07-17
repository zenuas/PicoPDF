using System;
using System.Drawing;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PicoPDF.Loader;

public static class JsonNodes
{
    public static JsonNode GetNode(this JsonNode self, string name) => self[name] ?? throw new NullReferenceException($"Element '{name}' was not found.");
    public static JsonValue GetValue(this JsonNode self, string name) => self.GetNode(name).AsValue();
    public static JsonValue? GetValueOrDefaultWithoutNullValue(this JsonNode self, string name) => self.AsObject() is { } x && x.ContainsKey(name) ? (x[name] is { } v ? v.AsValue() : throw new NullReferenceException($"Element '{name}' was null.")) : null;

    public static int GetIntValue(this JsonNode self, string name) => (int)self.GetValue(name);
    public static double GetDoubleValue(this JsonNode self, string name) => (double)self.GetValue(name);
    public static bool GetBoolValue(this JsonNode self, string name) => (bool)self.GetValue(name);
    public static string GetStringValue(this JsonNode self, string name) => self.GetValue(name).ToString();
    public static T GetEnumValue<T>(this JsonNode self, string name) where T : struct, Enum => Enum.Parse<T>(self.GetStringValue(name));
    public static Color GetColorValue(this JsonNode self, string name) => ColorTranslator.FromHtml(self.GetStringValue(name));
    public static CultureInfo GetCultureValue(this JsonNode self, string name) => CultureInfo.GetCultureInfo(self.GetStringValue(name));

    public static int? GetIntOrDefaultWithoutNullValue(this JsonNode self, string name) => self.GetValueOrDefaultWithoutNullValue(name) is { } x ? (int)x : null;
    public static double? GetDoubleOrDefaultWithoutNullValue(this JsonNode self, string name) => self.GetValueOrDefaultWithoutNullValue(name) is { } x ? (double)x : null;
    public static bool? GetBoolOrDefaultWithoutNullValue(this JsonNode self, string name) => self.GetValueOrDefaultWithoutNullValue(name) is { } x ? (bool)x : null;
    public static string? GetStringOrDefaultWithoutNullValue(this JsonNode self, string name) => self.GetValueOrDefaultWithoutNullValue(name) is { } x ? x.ToString() : null;
    public static T? GetEnumOrDefaultWithoutNullValue<T>(this JsonNode self, string name) where T : struct, Enum => self.GetStringOrDefaultWithoutNullValue(name) is { } x ? Enum.Parse<T>(x) : null;
    public static Color? GetColorOrDefaultWithoutNullValue(this JsonNode self, string name) => self.GetStringOrDefaultWithoutNullValue(name) is { } x ? ColorTranslator.FromHtml(x) : null;
    public static CultureInfo? GetCultureOrDefaultWithoutNullValue(this JsonNode self, string name) => self.GetStringOrDefaultWithoutNullValue(name) is { } x ? CultureInfo.GetCultureInfo(x) : null;
}
