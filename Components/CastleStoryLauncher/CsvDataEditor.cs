using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CastleStoryLauncher
{
    public class CsvDataEditor
    {
        private List<List<string>> data;
        private List<string> headers;
        private string filePath;
        private char delimiter;

        public CsvDataEditor(string filePath, char delimiter = ',', bool hasHeaders = true)
        {
            this.filePath = filePath;
            this.delimiter = delimiter;
            this.data = new List<List<string>>();
            this.headers = new List<string>();
            
            if (File.Exists(filePath))
            {
                LoadCsvFile(filePath, hasHeaders);
            }
        }

        public bool LoadCsvFile(string path, bool hasHeaders = true)
        {
            try
            {
                data.Clear();
                headers.Clear();
                
                var lines = File.ReadAllLines(path);
                if (lines.Length == 0) return false;

                int startIndex = 0;
                if (hasHeaders && lines.Length > 0)
                {
                    headers = ParseLine(lines[0]);
                    startIndex = 1;
                }

                for (int i = startIndex; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        data.Add(ParseLine(lines[i]));
                    }
                }

                filePath = path;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SaveCsvFile(string? path = null)
        {
            try
            {
                string savePath = path ?? filePath;
                var sb = new StringBuilder();

                // Write headers
                if (headers.Count > 0)
                {
                    sb.AppendLine(string.Join(delimiter.ToString(), headers.Select(EscapeField)));
                }

                // Write data
                foreach (var row in data)
                {
                    sb.AppendLine(string.Join(delimiter.ToString(), row.Select(EscapeField)));
                }

                File.WriteAllText(savePath, sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == delimiter && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            fields.Add(currentField.ToString());
            return fields;
        }

        private string EscapeField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(delimiter) || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        public List<string> GetHeaders()
        {
            return new List<string>(headers);
        }

        public void SetHeaders(List<string> newHeaders)
        {
            headers = new List<string>(newHeaders);
        }

        public int GetRowCount()
        {
            return data.Count;
        }

        public int GetColumnCount()
        {
            return headers.Count > 0 ? headers.Count : (data.Count > 0 ? data[0].Count : 0);
        }

        public List<string>? GetRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < data.Count)
            {
                return new List<string>(data[rowIndex]);
            }
            return null;
        }

        public bool SetRow(int rowIndex, List<string> rowData)
        {
            if (rowIndex >= 0 && rowIndex < data.Count)
            {
                data[rowIndex] = new List<string>(rowData);
                return true;
            }
            return false;
        }

        public bool AddRow(List<string> rowData)
        {
            data.Add(new List<string>(rowData));
            return true;
        }

        public bool InsertRow(int rowIndex, List<string> rowData)
        {
            if (rowIndex >= 0 && rowIndex <= data.Count)
            {
                data.Insert(rowIndex, new List<string>(rowData));
                return true;
            }
            return false;
        }

        public bool DeleteRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < data.Count)
            {
                data.RemoveAt(rowIndex);
                return true;
            }
            return false;
        }

        public string? GetCell(int rowIndex, int columnIndex)
        {
            if (rowIndex >= 0 && rowIndex < data.Count && 
                columnIndex >= 0 && columnIndex < data[rowIndex].Count)
            {
                return data[rowIndex][columnIndex];
            }
            return null;
        }

        public string? GetCell(int rowIndex, string columnName)
        {
            int columnIndex = headers.IndexOf(columnName);
            if (columnIndex >= 0)
            {
                return GetCell(rowIndex, columnIndex);
            }
            return null;
        }

        public bool SetCell(int rowIndex, int columnIndex, string value)
        {
            if (rowIndex >= 0 && rowIndex < data.Count && 
                columnIndex >= 0 && columnIndex < data[rowIndex].Count)
            {
                data[rowIndex][columnIndex] = value;
                return true;
            }
            return false;
        }

        public bool SetCell(int rowIndex, string columnName, string value)
        {
            int columnIndex = headers.IndexOf(columnName);
            if (columnIndex >= 0)
            {
                return SetCell(rowIndex, columnIndex, value);
            }
            return false;
        }

        public List<List<string>> GetAllData()
        {
            return data.Select(row => new List<string>(row)).ToList();
        }

        public List<string> GetColumn(int columnIndex)
        {
            var column = new List<string>();
            foreach (var row in data)
            {
                if (columnIndex >= 0 && columnIndex < row.Count)
                {
                    column.Add(row[columnIndex]);
                }
                else
                {
                    column.Add("");
                }
            }
            return column;
        }

        public List<string> GetColumn(string columnName)
        {
            int columnIndex = headers.IndexOf(columnName);
            if (columnIndex >= 0)
            {
                return GetColumn(columnIndex);
            }
            return new List<string>();
        }

        public List<int> SearchRows(string searchTerm, int columnIndex = -1)
        {
            var matchingRows = new List<int>();

            for (int i = 0; i < data.Count; i++)
            {
                if (columnIndex >= 0)
                {
                    if (columnIndex < data[i].Count && 
                        data[i][columnIndex].Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingRows.Add(i);
                    }
                }
                else
                {
                    if (data[i].Any(cell => cell.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    {
                        matchingRows.Add(i);
                    }
                }
            }

            return matchingRows;
        }

        public bool SortByColumn(int columnIndex, bool ascending = true)
        {
            try
            {
                if (columnIndex < 0) return false;

                data = ascending
                    ? data.OrderBy(row => row.Count > columnIndex ? row[columnIndex] : "").ToList()
                    : data.OrderByDescending(row => row.Count > columnIndex ? row[columnIndex] : "").ToList();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SortByColumn(string columnName, bool ascending = true)
        {
            int columnIndex = headers.IndexOf(columnName);
            if (columnIndex >= 0)
            {
                return SortByColumn(columnIndex, ascending);
            }
            return false;
        }

        public Dictionary<string, int> GetColumnStatistics(int columnIndex)
        {
            var stats = new Dictionary<string, int>();

            foreach (var row in data)
            {
                if (columnIndex >= 0 && columnIndex < row.Count)
                {
                    string value = row[columnIndex];
                    if (stats.ContainsKey(value))
                    {
                        stats[value]++;
                    }
                    else
                    {
                        stats[value] = 1;
                    }
                }
            }

            return stats;
        }

        public List<int> FindDuplicates(int columnIndex)
        {
            var duplicateRows = new List<int>();
            var seenValues = new Dictionary<string, int>();

            for (int i = 0; i < data.Count; i++)
            {
                if (columnIndex >= 0 && columnIndex < data[i].Count)
                {
                    string value = data[i][columnIndex];
                    if (seenValues.ContainsKey(value))
                    {
                        duplicateRows.Add(i);
                    }
                    else
                    {
                        seenValues[value] = i;
                    }
                }
            }

            return duplicateRows;
        }

        public bool RemoveDuplicates(int columnIndex)
        {
            try
            {
                var uniqueRows = new List<List<string>>();
                var seenValues = new HashSet<string>();

                foreach (var row in data)
                {
                    if (columnIndex >= 0 && columnIndex < row.Count)
                    {
                        string value = row[columnIndex];
                        if (!seenValues.Contains(value))
                        {
                            seenValues.Add(value);
                            uniqueRows.Add(row);
                        }
                    }
                }

                data = uniqueRows;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ExportToJson(string outputPath)
        {
            try
            {
                var jsonData = new List<Dictionary<string, string>>();

                foreach (var row in data)
                {
                    var rowDict = new Dictionary<string, string>();
                    for (int i = 0; i < headers.Count && i < row.Count; i++)
                    {
                        rowDict[headers[i]] = row[i];
                    }
                    jsonData.Add(rowDict);
                }

                var json = System.Text.Json.JsonSerializer.Serialize(jsonData, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(outputPath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Validate(int columnIndex, Func<string, bool> validator, out List<int> invalidRows)
        {
            invalidRows = new List<int>();

            for (int i = 0; i < data.Count; i++)
            {
                if (columnIndex >= 0 && columnIndex < data[i].Count)
                {
                    if (!validator(data[i][columnIndex]))
                    {
                        invalidRows.Add(i);
                    }
                }
            }

            return invalidRows.Count == 0;
        }

        public void Clear()
        {
            data.Clear();
        }

        public CsvDataEditor Clone()
        {
            var clone = new CsvDataEditor(filePath, delimiter, headers.Count > 0);
            clone.headers = new List<string>(headers);
            clone.data = data.Select(row => new List<string>(row)).ToList();
            return clone;
        }
    }
}

