using System.Configuration;

public static class AppConfig
{
    public static string OverlayColorHex
    {
        get => ConfigurationManager.AppSettings["OverlayColorHex"] ?? "#000000";
        set => UpdateAppSetting("OverlayColorHex", value);
    }

    public static double OverlayOpacity
    {
        get => double.TryParse(ConfigurationManager.AppSettings["OverlayOpacity"], out var value) ? value : 0.5;
        set => UpdateAppSetting("OverlayOpacity", value.ToString());
    }
    public static DateTime LastChecked
    {
        get => DateTime.TryParse(ConfigurationManager.AppSettings["LastChecked"], out var value) ? value : DateTime.MinValue;
        set => UpdateAppSetting("LastChecked", value.ToString("o")); 
    }
    public static string LastCheckedMessage
    {
        get => ConfigurationManager.AppSettings["LastCheckedMessage"] ?? $"Last Checked {DateTime.Now}";
        set => UpdateAppSetting("LastCheckedMessage", value);
    }


    public static float CircleRadius
    {
        get => float.TryParse(ConfigurationManager.AppSettings["CircleRadius"], out var value) ? value : 50.0f;
        set => UpdateAppSetting("CircleRadius", value.ToString());
    }

    public static string BorderColorHex
    {
        get => ConfigurationManager.AppSettings["BorderColorHex"] ?? "#FF0000";
        set => UpdateAppSetting("BorderColorHex", value);
    }

    public static double BorderColorOpacity
    {
        get => double.TryParse(ConfigurationManager.AppSettings["BorderColorOpacity"], out var value) ? value : 1.0;
        set => UpdateAppSetting("BorderColorOpacity", value.ToString());
    }

    public static float BorderThickness
    {
        get => float.TryParse(ConfigurationManager.AppSettings["BorderThickness"], out var value) ? value : 1.0f;
        set => UpdateAppSetting("BorderThickness", value.ToString());
    }

    public static float ZoomScaleFactor
    {
        get => float.TryParse(ConfigurationManager.AppSettings["ZoomScaleFactor"], out var value) ? value : 1.0f;
        set => UpdateAppSetting("ZoomScaleFactor", value.ToString());
    }

    private static void UpdateAppSetting(string key, string value)
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings[key].Value = value;
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
    }
}
