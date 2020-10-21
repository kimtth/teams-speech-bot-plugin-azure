using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Repository
{
    public class InMemoryRepository : IInfoRepository
    {
        private readonly ConcurrentDictionary<string, string> settingMap = new ConcurrentDictionary<string, string>();

        public string GetSetting(string key)
        {
            return this.settingMap.GetValueOrDefault(key);
        }

        public void SetSetting(string key, string value)
        {
            _ = this.settingMap.TryAdd(key, value);
        }

    }
}