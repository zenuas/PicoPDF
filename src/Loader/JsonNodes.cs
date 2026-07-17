using System;
using System.Drawing;
using System.Globalization;
using System.Text.Json.Nodes;

namespace PicoPDF.Loader;

public static class JsonNodes
{
    public static JsonNode GetNode(this JsonNode self, string name) => self[name] ?? throw new NullReferenceException($"Element '{name}' was not found.");
    public static JsonValue GetValue(this JsonNode self, string name) => self.GetNode(name).AsValue();

    public static int GetIntValue(this JsonNode self, string name) => (int)self.GetValue(name);
    public static double GetDoubleValue(this JsonNode self, string name) => (double)self.GetValue(name);
    public static bool GetBoolValue(this JsonNode self, string name) => (bool)self.GetValue(name);
    public static string GetStringValue(this JsonNode self, string name) => self.GetValue(name).ToString();
    public static T GetEnumValue<T>(this JsonNode self, string name) where T : struct, Enum => Enum.Parse<T>(self.GetStringValue(name));
    public static Color GetColorValue(this JsonNode self, string name) => ColorTranslator.FromHtml(self.GetStringValue(name));
    public static CultureInfo GetCultureValue(this JsonNode self, string name) => CultureInfo.GetCultureInfo(self.GetStringValue(name));

    public static int? GetIntOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (int)x.AsValue() : null;
    public static double? GetDoubleOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (double)x.AsValue() : null;
    public static bool? GetBoolOrNullValue(this JsonNode self, string name) => self[name] is { } x ? (bool)x.AsValue() : null;
    public static string? GetStringOrNullValue(this JsonNode self, string name) => self[name] is { } x ? x.AsValue().ToString() : null;
    public static T? GetEnumOrNullValue<T>(this JsonNode self, string name) where T : struct, Enum => self.GetStringOrNullValue(name) is { } x ? Enum.Parse<T>(x) : null;
    public static Color? GetColorOrNullValue(this JsonNode self, string name) => self.GetStringOrNullValue(name) is { } x ? ColorTranslator.FromHtml(x) : null;
    public static CultureInfo? GetCultureOrNullValue(this JsonNode self, string name) => self.GetStringOrNullValue(name) is { } x ? CultureInfo.GetCultureInfo(x) : null;
}
