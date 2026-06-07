namespace btr.infrastructure.Helpers
{
    public interface IConnectionSettingProvider
    {
        string GetServerName();
        string GetDatabaseName();
    }
}
