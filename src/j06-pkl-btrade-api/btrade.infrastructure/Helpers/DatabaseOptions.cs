namespace btrade.infrastructure.Helpers;

public class DatabaseOptions
{
    public const string SECTION_NAME = "Database";

    public string Server { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
}
