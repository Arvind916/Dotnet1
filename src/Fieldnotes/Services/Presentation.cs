namespace Fieldnotes.Services;

public static class Presentation
{
    public static string TrackIcon(string track) => track switch
    {
        "C# essentials" => "braces",
        "Runtime & concurrency" => "cpu",
        "Web & APIs" => "globe",
        "Data & performance" => "database",
        "Architecture" => "layers",
        _ => "flask-conical"
    };

    public static string TrackTone(string track) => track switch
    {
        "C# essentials" => "green",
        "Runtime & concurrency" => "blue",
        "Web & APIs" => "coral",
        "Data & performance" => "gold",
        "Architecture" => "violet",
        _ => "cyan"
    };

    public static string Since(DateTime date, DateTime now)
    {
        var elapsed = now - date;
        if (elapsed.TotalMinutes < 1) return "Just now";
        if (elapsed.TotalHours < 1) return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalDays < 1) return $"{(int)elapsed.TotalHours} hr ago";
        return date.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture);
    }
}