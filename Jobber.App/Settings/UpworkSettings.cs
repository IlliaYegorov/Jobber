namespace Jobber.App.Settings;

public class UpworkSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = [];
}
