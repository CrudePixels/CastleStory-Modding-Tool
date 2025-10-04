using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CastleStoryLauncher
{
    public class LanguageEntry
    {
        public string Key { get; set; } = string.Empty;
        public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
        public string Category { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
    }

    public class LanguageFileEditor
    {
        private Dictionary<string, LanguageEntry> entries;
        private List<string> supportedLanguages;
        private string filePath;

        public LanguageFileEditor()
        {
            entries = new Dictionary<string, LanguageEntry>();
            supportedLanguages = new List<string> { "en", "fr", "de", "es", "it", "pt", "ru", "ja", "ko", "zh" };
        }

        public bool LoadLanguageFile(string path, string language = "en")
        {
            try
            {
                filePath = path;
                string content = File.ReadAllText(path);

                // Try to parse as JSON first
                try
                {
                    var jsonData = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                    if (jsonData != null)
                    {
                        foreach (var kvp in jsonData)
                        {
                            if (!entries.ContainsKey(kvp.Key))
                            {
                                entries[kvp.Key] = new LanguageEntry { Key = kvp.Key };
                            }
                            entries[kvp.Key].Translations[language] = kvp.Value;
                        }
                        return true;
                    }
                }
                catch
                {
                    // Not JSON, try key=value format
                }

                // Parse key=value format
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith("--"))
                        continue;

                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim().Trim('"');

                        if (!entries.ContainsKey(key))
                        {
                            entries[key] = new LanguageEntry { Key = key };
                        }
                        entries[key].Translations[language] = value;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveLanguageFile(string path, string language = "en", bool asJson = false)
        {
            try
            {
                if (asJson)
                {
                    var jsonData = entries
                        .Where(e => e.Value.Translations.ContainsKey(language))
                        .ToDictionary(e => e.Key, e => e.Value.Translations[language]);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(jsonData, options);
                    File.WriteAllText(path, json);
                }
                else
                {
                    using (var writer = new StreamWriter(path))
                    {
                        writer.WriteLine("-- Language File: " + language);
                        writer.WriteLine("-- Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        writer.WriteLine();

                        var categories = entries.Values
                            .Where(e => e.Translations.ContainsKey(language))
                            .GroupBy(e => string.IsNullOrEmpty(e.Category) ? "General" : e.Category)
                            .OrderBy(g => g.Key);

                        foreach (var category in categories)
                        {
                            writer.WriteLine($"-- {category.Key}");
                            writer.WriteLine();

                            foreach (var entry in category.OrderBy(e => e.Key))
                            {
                                if (!string.IsNullOrEmpty(entry.Context))
                                {
                                    writer.WriteLine($"-- Context: {entry.Context}");
                                }
                                writer.WriteLine($"{entry.Key} = \"{entry.Translations[language]}\"");
                                writer.WriteLine();
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetSupportedLanguages()
        {
            return new List<string>(supportedLanguages);
        }

        public void AddSupportedLanguage(string languageCode)
        {
            if (!supportedLanguages.Contains(languageCode))
            {
                supportedLanguages.Add(languageCode);
            }
        }

        public string? GetTranslation(string key, string language)
        {
            if (entries.ContainsKey(key) && entries[key].Translations.ContainsKey(language))
            {
                return entries[key].Translations[language];
            }
            return null;
        }

        public bool SetTranslation(string key, string language, string translation)
        {
            if (!entries.ContainsKey(key))
            {
                entries[key] = new LanguageEntry { Key = key };
            }

            entries[key].Translations[language] = translation;
            return true;
        }

        public bool AddEntry(string key, string category = "", string context = "")
        {
            if (entries.ContainsKey(key))
                return false;

            entries[key] = new LanguageEntry
            {
                Key = key,
                Category = category,
                Context = context
            };

            return true;
        }

        public bool RemoveEntry(string key)
        {
            return entries.Remove(key);
        }

        public List<string> GetAllKeys()
        {
            return entries.Keys.ToList();
        }

        public List<string> GetKeysByCategory(string category)
        {
            return entries.Values
                .Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Key)
                .ToList();
        }

        public List<string> GetCategories()
        {
            return entries.Values
                .Select(e => string.IsNullOrEmpty(e.Category) ? "General" : e.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        public List<string> SearchKeys(string searchTerm)
        {
            return entries.Keys
                .Where(k => k.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<string> SearchTranslations(string searchTerm, string language)
        {
            return entries
                .Where(e => e.Value.Translations.ContainsKey(language) &&
                           e.Value.Translations[language].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Key)
                .ToList();
        }

        public List<string> FindMissingTranslations(string language)
        {
            return entries
                .Where(e => !e.Value.Translations.ContainsKey(language) || 
                           string.IsNullOrWhiteSpace(e.Value.Translations[language]))
                .Select(e => e.Key)
                .ToList();
        }

        public Dictionary<string, int> GetTranslationStatistics(string language)
        {
            var stats = new Dictionary<string, int>();

            stats["Total Entries"] = entries.Count;
            stats["Translated"] = entries.Values.Count(e => e.Translations.ContainsKey(language) && 
                                                            !string.IsNullOrWhiteSpace(e.Translations[language]));
            stats["Missing"] = stats["Total Entries"] - stats["Translated"];
            stats["Completion %"] = stats["Total Entries"] > 0 
                ? (stats["Translated"] * 100) / stats["Total Entries"] 
                : 0;

            return stats;
        }

        public bool ValidateTranslation(string key, string language, out List<string> errors)
        {
            errors = new List<string>();

            if (!entries.ContainsKey(key))
            {
                errors.Add("Key does not exist");
                return false;
            }

            if (!entries[key].Translations.ContainsKey(language))
            {
                errors.Add("Translation not found for language: " + language);
                return false;
            }

            string translation = entries[key].Translations[language];

            if (string.IsNullOrWhiteSpace(translation))
            {
                errors.Add("Translation is empty");
                return false;
            }

            // Check for placeholder consistency
            var sourceLanguage = supportedLanguages.FirstOrDefault(lang => 
                entries[key].Translations.ContainsKey(lang) && !string.IsNullOrEmpty(entries[key].Translations[lang]));

            if (sourceLanguage != null && sourceLanguage != language)
            {
                string source = entries[key].Translations[sourceLanguage];
                
                // Check for %s, %d, {0}, {1}, etc.
                var sourcePlaceholders = System.Text.RegularExpressions.Regex.Matches(source, @"%[sd]|{\d+}");
                var translationPlaceholders = System.Text.RegularExpressions.Regex.Matches(translation, @"%[sd]|{\d+}");

                if (sourcePlaceholders.Count != translationPlaceholders.Count)
                {
                    errors.Add($"Placeholder mismatch: source has {sourcePlaceholders.Count}, translation has {translationPlaceholders.Count}");
                }
            }

            return errors.Count == 0;
        }

        public bool AutoTranslate(string fromLanguage, string toLanguage, Func<string, string, string> translateFunc)
        {
            try
            {
                var keysToTranslate = FindMissingTranslations(toLanguage);

                foreach (var key in keysToTranslate)
                {
                    if (entries[key].Translations.ContainsKey(fromLanguage))
                    {
                        string sourceText = entries[key].Translations[fromLanguage];
                        string translated = translateFunc(sourceText, toLanguage);
                        entries[key].Translations[toLanguage] = translated;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ExportToCSV(string outputPath)
        {
            try
            {
                using (var writer = new StreamWriter(outputPath))
                {
                    // Write header
                    writer.Write("Key,Category,Context");
                    foreach (var lang in supportedLanguages)
                    {
                        writer.Write($",{lang}");
                    }
                    writer.WriteLine();

                    // Write entries
                    foreach (var entry in entries.Values.OrderBy(e => e.Key))
                    {
                        writer.Write($"\"{entry.Key}\",\"{entry.Category}\",\"{entry.Context}\"");
                        
                        foreach (var lang in supportedLanguages)
                        {
                            string translation = entry.Translations.ContainsKey(lang) 
                                ? entry.Translations[lang] 
                                : "";
                            writer.Write($",\"{translation.Replace("\"", "\"\"")}\"");
                        }
                        writer.WriteLine();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ImportFromCSV(string inputPath)
        {
            try
            {
                var csvEditor = new CsvDataEditor(inputPath, ',', true);
                var headers = csvEditor.GetHeaders();

                // Determine language columns
                var languageColumns = headers
                    .Skip(3) // Skip Key, Category, Context
                    .ToList();

                for (int i = 0; i < csvEditor.GetRowCount(); i++)
                {
                    var row = csvEditor.GetRow(i);
                    if (row == null || row.Count < 3) continue;

                    string key = row[0];
                    string category = row[1];
                    string context = row[2];

                    if (!entries.ContainsKey(key))
                    {
                        entries[key] = new LanguageEntry
                        {
                            Key = key,
                            Category = category,
                            Context = context
                        };
                    }

                    for (int j = 0; j < languageColumns.Count; j++)
                    {
                        int columnIndex = j + 3;
                        if (columnIndex < row.Count && !string.IsNullOrWhiteSpace(row[columnIndex]))
                        {
                            entries[key].Translations[languageColumns[j]] = row[columnIndex];
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, Dictionary<string, string>> GetAllTranslations()
        {
            var result = new Dictionary<string, Dictionary<string, string>>();

            foreach (var entry in entries)
            {
                result[entry.Key] = new Dictionary<string, string>(entry.Value.Translations);
            }

            return result;
        }

        public bool MergeWith(LanguageFileEditor other, bool overwriteExisting = false)
        {
            try
            {
                foreach (var entry in other.entries)
                {
                    if (!entries.ContainsKey(entry.Key))
                    {
                        entries[entry.Key] = new LanguageEntry
                        {
                            Key = entry.Value.Key,
                            Category = entry.Value.Category,
                            Context = entry.Value.Context,
                            Translations = new Dictionary<string, string>(entry.Value.Translations)
                        };
                    }
                    else if (overwriteExisting)
                    {
                        foreach (var translation in entry.Value.Translations)
                        {
                            entries[entry.Key].Translations[translation.Key] = translation.Value;
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Clear()
        {
            entries.Clear();
        }
    }
}

