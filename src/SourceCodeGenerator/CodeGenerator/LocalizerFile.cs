using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SourceCodeGenerator.CodeGenerator
{
    public class LocalizerFile
    {
        private string FilePath { get; set; }
        private Dictionary<string, string> LocalizerDictionary { get; set; } = new Dictionary<string, string>();

        public LocalizerFile(string filePath)
        {
            FilePath = filePath;
            string localizationJson = File.ReadAllText(filePath);
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(localizationJson);
            if (dictionary is not null) LocalizerDictionary = (Dictionary<string, string>)dictionary;
        }

        public void AddEntry(string key, string value)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            if (!LocalizerDictionary!.ContainsKey(key)) LocalizerDictionary.Add(key, value);
        }

        public void SaveLocalizer(bool BackupFile = true)
        {
            string jsonString = JsonSerializer.Serialize(LocalizerDictionary).Replace(",", ",\r\n").Replace("{\"", "{\r\n\"").Replace("\"}", "\"\r\n}");
            if(BackupFile) EngineFunctions.BackupFile(FilePath);
            File.WriteAllText(FilePath, jsonString);
        }
    }
}
