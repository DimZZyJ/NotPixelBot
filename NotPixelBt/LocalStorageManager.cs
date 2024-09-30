using OpenQA.Selenium.Chrome;
using System.Text.Json;

namespace NotPixelBt
{
    public class LocalStorageManager
    {
        public ChromeDriver Driver;
        public List<string> AvailableStorages;
        private string _sessionsStoragePath = $"{Directory.GetCurrentDirectory()}\\sessions";
        public LocalStorageManager(ChromeDriver driver)
        {
            Driver = driver;
            if (Path.Exists(_sessionsStoragePath) == false)
                Directory.CreateDirectory(_sessionsStoragePath);

            AvailableStorages = Directory.EnumerateFiles(_sessionsStoragePath, "*.json").ToList();
        }
        
        public void SaveCurrentStorageToJson(string storageName)
        {
            if (storageName == "")
                storageName = $"{DateTime.Now}_session";

            string storageJson = GetSessionStorageJson();
            string storagePath = Path.Combine(_sessionsStoragePath, storageName+".json");
            File.WriteAllText(storagePath, storageJson);
        }
        public void SetCurrentLocalStorage(int index)
        {
            string json = File.ReadAllText(AvailableStorages[index]);
            Dictionary<string, string> localDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            
            foreach (string key in localDictionary.Keys)
            {
                localDictionary.TryGetValue(key, out string value);
                Driver.ExecuteScript($"localStorage.setItem('{key}','{value}');");
            }
        }
        private string GetSessionStorageJson()
        {
            return (string)Driver.ExecuteScript("return JSON.stringify(window.localStorage);"); ;
        }
    }

    public enum SessionMode
    {
        NewSession,
        LoadSession,
        AnonymousSession
    }
}
