namespace Repository
{
    public interface IInfoRepository
    {
        string GetSetting(string key);
        void SetSetting(string key, string value);
    }
}