namespace Jobber.App.Settings;

public class SlackSettings
{
    public string OAuthToken { get; set; }
    public SlackChannelsSettings Channels { get; set; }
}

public class SlackChannelsSettings
{
    public Dictionary<string, string> FixedPrice { get; set; }
    public Dictionary<string, string> Hourly { get; set; }
}
