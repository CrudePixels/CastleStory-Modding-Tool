using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CastleStoryLauncher
{
    public class JsonConfigEditor
    {
        private JsonNode? rootNode;
        private string filePath;

        public JsonConfigEditor(string filePath)
        {
            this.filePath = filePath;
            LoadJsonFile(filePath);
        }

        public bool LoadJsonFile(string path)
        {
            try
            {
                string jsonContent = File.ReadAllText(path);
                rootNode = JsonNode.Parse(jsonContent);
                filePath = path;
                return true;
            }
            catch
            {
                rootNode = null;
                return false;
            }
        }

        public bool SaveJsonFile(string? path = null)
        {
            try
            {
                string savePath = path ?? filePath;
                if (rootNode == null) return false;

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonContent = rootNode.ToJsonString(options);
                File.WriteAllText(savePath, jsonContent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public object? GetValue(string jsonPath)
        {
            try
            {
                if (rootNode == null) return null;

                var parts = jsonPath.Split('.');
                JsonNode? current = rootNode;

                foreach (var part in parts)
                {
                    if (current is JsonObject obj && obj.ContainsKey(part))
                    {
                        current = obj[part];
                    }
                    else if (current is JsonArray arr && int.TryParse(part, out int index))
                    {
                        current = arr[index];
                    }
                    else
                    {
                        return null;
                    }
                }

                return current?.GetValueKind() switch
                {
                    JsonValueKind.String => current.GetValue<string>(),
                    JsonValueKind.Number => current.GetValue<double>(),
                    JsonValueKind.True or JsonValueKind.False => current.GetValue<bool>(),
                    _ => current?.ToJsonString()
                };
            }
            catch
            {
                return null;
            }
        }

        public bool SetValue(string jsonPath, object value)
        {
            try
            {
                if (rootNode == null) return false;

                var parts = jsonPath.Split('.');
                JsonNode? current = rootNode;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (current is JsonObject obj && obj.ContainsKey(parts[i]))
                    {
                        current = obj[parts[i]];
                    }
                    else if (current is JsonArray arr && int.TryParse(parts[i], out int index))
                    {
                        current = arr[index];
                    }
                    else
                    {
                        return false;
                    }
                }

                string lastPart = parts[parts.Length - 1];
                if (current is JsonObject finalObj)
                {
                    finalObj[lastPart] = JsonValue.Create(value);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateSchema(string schemaPath)
        {
            // Basic validation - can be enhanced with a proper JSON schema validator
            try
            {
                if (rootNode == null) return false;
                
                string schemaContent = File.ReadAllText(schemaPath);
                var schema = JsonNode.Parse(schemaContent);
                
                // Very basic validation - just check if it's valid JSON
                return schema != null;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAllKeys(string? prefix = null)
        {
            var keys = new List<string>();
            if (rootNode == null) return keys;

            CollectKeys(rootNode, prefix ?? "", keys);
            return keys;
        }

        private void CollectKeys(JsonNode node, string prefix, List<string> keys)
        {
            if (node is JsonObject obj)
            {
                foreach (var kvp in obj)
                {
                    string key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                    keys.Add(key);
                    
                    if (kvp.Value != null)
                    {
                        CollectKeys(kvp.Value, key, keys);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    string key = $"{prefix}[{i}]";
                    keys.Add(key);
                    
                    if (arr[i] != null)
                    {
                        CollectKeys(arr[i]!, key, keys);
                    }
                }
            }
        }

        public string FormatJson()
        {
            if (rootNode == null) return "{}";

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return rootNode.ToJsonString(options);
        }

        public bool MinifyJson(string outputPath)
        {
            try
            {
                if (rootNode == null) return false;

                var options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonContent = rootNode.ToJsonString(options);
                File.WriteAllText(outputPath, jsonContent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, string> GetFlatKeyValuePairs()
        {
            var result = new Dictionary<string, string>();
            if (rootNode == null) return result;

            FlattenNode(rootNode, "", result);
            return result;
        }

        private void FlattenNode(JsonNode node, string prefix, Dictionary<string, string> result)
        {
            if (node is JsonObject obj)
            {
                foreach (var kvp in obj)
                {
                    string key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
                    
                    if (kvp.Value is JsonValue)
                    {
                        result[key] = kvp.Value.ToJsonString().Trim('"');
                    }
                    else if (kvp.Value != null)
                    {
                        FlattenNode(kvp.Value, key, result);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    string key = $"{prefix}[{i}]";
                    if (arr[i] is JsonValue)
                    {
                        result[key] = arr[i]!.ToJsonString().Trim('"');
                    }
                    else if (arr[i] != null)
                    {
                        FlattenNode(arr[i]!, key, result);
                    }
                }
            }
        }

        public bool SearchKey(string searchTerm, out List<string> matchingKeys)
        {
            matchingKeys = new List<string>();
            if (rootNode == null) return false;

            var allKeys = GetAllKeys();
            foreach (var key in allKeys)
            {
                if (key.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matchingKeys.Add(key);
                }
            }

            return matchingKeys.Count > 0;
        }

        public bool MergeWith(JsonConfigEditor other)
        {
            try
            {
                if (rootNode == null || other.rootNode == null) return false;

                if (rootNode is JsonObject thisObj && other.rootNode is JsonObject otherObj)
                {
                    MergeObjects(thisObj, otherObj);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void MergeObjects(JsonObject target, JsonObject source)
        {
            foreach (var kvp in source)
            {
                if (target.ContainsKey(kvp.Key))
                {
                    if (target[kvp.Key] is JsonObject targetObj && kvp.Value is JsonObject sourceObj)
                    {
                        MergeObjects(targetObj, sourceObj);
                    }
                    else
                    {
                        target[kvp.Key] = kvp.Value?.DeepClone();
                    }
                }
                else
                {
                    target[kvp.Key] = kvp.Value?.DeepClone();
                }
            }
        }

        public string GetJsonAsString()
        {
            return FormatJson();
        }
    }
}

