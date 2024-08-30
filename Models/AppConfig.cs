using System;
using System.IO;
using System.Xml.Linq;

public static class AppConfig
{
    private static readonly object _lock = new object();
    private static readonly string AppDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
    private static readonly string ConfigDirectory = Path.Combine(AppDirectory, "..", "EasyZoomerSettings");
    private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "appsettings.config");

    static AppConfig()
    {
        if (!Directory.Exists(ConfigDirectory))
        {
            Directory.CreateDirectory(ConfigDirectory);
        }

        if (!File.Exists(ConfigFilePath))
        {
            // Create a new config file if it doesn't exist
            new XDocument(new XElement("appSettings")).Save(ConfigFilePath);
        }
    }

    public static string OverlayColorHex
    {
        get => GetAppSetting("OverlayColorHex") ?? "#000000";
        set => UpdateAppSetting("OverlayColorHex", value);
    }

    public static double OverlayOpacity
    {
        get => double.TryParse(GetAppSetting("OverlayOpacity"), out var value) ? value : 0.5;
        set => UpdateAppSetting("OverlayOpacity", value.ToString());
    }

    public static DateTime LastChecked
    {
        get => DateTime.TryParse(GetAppSetting("LastChecked"), out var value) ? value : DateTime.MinValue;
        set => UpdateAppSetting("LastChecked", value.ToString("o"));
    }

    public static string LastCheckedMessage
    {
        get => GetAppSetting("LastCheckedMessage") ?? $"Last Checked {DateTime.Now}";
        set => UpdateAppSetting("LastCheckedMessage", value);
    }

    public static float CircleRadius
    {
        get => float.TryParse(GetAppSetting("CircleRadius"), out var value) ? value : 50.0f;
        set => UpdateAppSetting("CircleRadius", value.ToString());
    }

    public static string BorderColorHex
    {
        get => GetAppSetting("BorderColorHex") ?? "#FF0000";
        set => UpdateAppSetting("BorderColorHex", value);
    }

    public static double BorderColorOpacity
    {
        get => double.TryParse(GetAppSetting("BorderColorOpacity"), out var value) ? value : 1.0;
        set => UpdateAppSetting("BorderColorOpacity", value.ToString());
    }

    public static float BorderThickness
    {
        get => float.TryParse(GetAppSetting("BorderThickness"), out var value) ? value : 1.0f;
        set => UpdateAppSetting("BorderThickness", value.ToString());
    }

    public static float ZoomScaleFactor
    {
        get => float.TryParse(GetAppSetting("ZoomScaleFactor"), out var value) ? value : 1.0f;
        set => UpdateAppSetting("ZoomScaleFactor", value.ToString());
    }

    private static string GetAppSetting(string key)
    {
        lock (_lock)
        {
            try
            {
                var doc = XDocument.Load(ConfigFilePath);
                var element = doc.Root.Element(key);
                return element?.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading config: {ex.Message}");
                return null;
            }
        }
    }

    private static void UpdateAppSetting(string key, string value)
    {
        lock (_lock)
        {
            try
            {
                var doc = XDocument.Load(ConfigFilePath);
                var element = doc.Root.Element(key);

                if (element != null)
                {
                    element.Value = value;
                }
                else
                {
                    doc.Root.Add(new XElement(key, value));
                }

                doc.Save(ConfigFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating config: {ex.Message}");
            }
        }
    }
}