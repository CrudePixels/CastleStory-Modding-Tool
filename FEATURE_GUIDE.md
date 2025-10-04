# Castle Story Modding Tool - Feature Guide

## New Features in v1.4.0+

### ✨ Completed High-Priority Features

This release includes **4 comprehensive file editors** and **enhanced configuration management** for Castle Story modding.

---

## 🎨 Enhanced Faction Color System

The Faction Color Editor now includes 30 pre-configured colors with advanced editing capabilities.

### Features:
- **30 Built-in Colors**: Blue, Green, Orange, Purple, Red, Yellow, Cyan, Magenta, Lime, Pink, Teal, Indigo, Brown, Gray, Gold, Silver, Crimson, Forest Green, Navy, Maroon, Olive, Turquoise, Violet, Coral, Khaki, Salmon, Lavender, Mint, Peach, Sky Blue
- **RGB Sliders**: Fine-tune colors with precise Red, Green, Blue control (0-255)
- **Hex Input**: Enter colors using standard hex format (#RRGGBB)
- **Live Preview**: See color changes in real-time
- **Color Validation**: Automatic validation of hex input

### Usage:
1. Open Castle Story Launcher
2. Navigate to Faction Color Editor
3. Select a preset color or create custom colors
4. Adjust using RGB sliders or hex input
5. Apply to your game configuration

---

## 📋 Gamemode Preset Management System

The new PresetManager allows you to save, load, and share gamemode configurations.

### Features:
- **Built-in Presets**:
  - **Easy Survival**: 1000 starting resources, 0.5x enemy difficulty
  - **Normal Survival**: 500 starting resources, 1.0x enemy difficulty
  - **Hard Survival**: 250 starting resources, 1.5x enemy difficulty
  - **Creative Sandbox**: Unlimited resources, no enemies, instant build
  - **Competitive Conquest**: Balanced team vs team (16 players)

- **Custom Presets**: Create your own difficulty configurations
- **Import/Export**: Share presets with other players
- **Gamemode Types**: Support for Sandbox, Invasion, Conquest

### Usage:
```csharp
// Create a preset manager
var presetManager = new PresetManager(gameDirectory);

// Get available presets
var presets = presetManager.GetPresets();

// Apply a preset to a config file
var preset = presetManager.GetPresetByName("Hard Survival");
presetManager.ApplyPresetToConfig(preset, "config.lua");

// Create custom preset
var customPreset = presetManager.CreateCustomPreset(
    "Ultra Hard", 
    "Extreme difficulty", 
    "invasion"
);
customPreset.Settings["startingResources"] = 100;
customPreset.Settings["enemyDifficulty"] = 2.0;
presetManager.SavePreset(customPreset);
```

### Preset File Format:
```json
{
  "Name": "Hard Survival",
  "Description": "Challenging gameplay for veterans",
  "GamemodeType": "invasion",
  "Settings": {
    "startingResources": 250,
    "enemyDifficulty": 1.5,
    "waveDelay": 180,
    "buildPhaseTime": 120
  },
  "CreatedDate": "2025-10-04T...",
  "Author": "User"
}
```

---

## 🔧 JSON Configuration Editor

Comprehensive JSON editing system for game configuration files.

### Features:
- **Hierarchical Navigation**: Browse JSON structure with dot notation (e.g., `settings.gameplay.difficulty`)
- **Key-Value Editing**: Modify values directly using simple paths
- **Flattened View**: Get all key-value pairs in a flat dictionary
- **Search**: Find keys by partial match
- **Format/Minify**: Pretty-print or compress JSON files
- **Merge**: Combine multiple JSON configurations
- **Schema Validation**: Basic validation support

### Usage:
```csharp
// Load a JSON config file
var jsonEditor = new JsonConfigEditor("faction_config.json");

// Get a value
var difficulty = jsonEditor.GetValue("gameplay.difficulty");

// Set a value
jsonEditor.SetValue("gameplay.difficulty", 2.5);
jsonEditor.SetValue("faction.maxPlayers", 16);

// Save changes
jsonEditor.SaveJsonFile();

// Search for keys
jsonEditor.SearchKey("player", out var matchingKeys);
// matchingKeys might contain: ["faction.maxPlayers", "gameplay.playerStartResources"]

// Get all keys as flat dictionary
var flatData = jsonEditor.GetFlatKeyValuePairs();
foreach (var kvp in flatData)
{
    Console.WriteLine($"{kvp.Key} = {kvp.Value}");
}

// Format and minify
string formatted = jsonEditor.FormatJson();
jsonEditor.MinifyJson("config.min.json");
```

### Supported Operations:
- Load/Save JSON files
- Get/Set values by path
- Search keys
- Format/Minify
- Merge configurations
- Export formatted JSON

---

## 🏗️ Modular Modding System

The ModManager architecture provides extensible mod support with 5 integration types.

### Integration Types:

#### 1. **FileModification**
Modify Lua, config, and game files directly
```csharp
var fileIntegration = new FileModificationIntegration("MyMod", modifications);
```

#### 2. **DLLInjection**
Inject compiled DLL mods into the game process
```csharp
var dllIntegration = new DLLInjectionIntegration("MyMod", "path/to/mod.dll");
```

#### 3. **AssetReplacement**
Replace game assets (textures, models, sounds)
```csharp
var assetIntegration = new AssetReplacementIntegration("MyMod", assetReplacements);
```

#### 4. **LuaInjection**
Inject Lua code into game scripts
```csharp
var luaIntegration = new LuaInjectionIntegration("MyMod", luaInjections);
```

#### 5. **MemoryPatching**
Runtime memory modifications for limits and values
```csharp
var memoryIntegration = new MemoryPatchingIntegration("MyMod", memoryPatches);
```

### Creating Custom Mods:
1. Create a ModDefinition class
2. Implement the integration type(s) needed
3. Register with ModManager
4. Apply during game launch

Example:
```csharp
// Define a custom mod
public static class CustomModDefinition
{
    public static IModIntegration CreateIntegration()
    {
        var modifications = new List<FileModification>
        {
            new FileModification
            {
                RelativePath = "Info/Lua/config.lua",
                Type = FileModificationType.ReplaceText,
                SearchText = "maxPlayers = 4",
                ReplaceText = "maxPlayers = 16"
            }
        };

        return new FileModificationIntegration("CustomMod", modifications);
    }
}

// Register and apply
modManager.RegisterMod("CustomMod", CustomModDefinition.CreateIntegration());
modManager.ApplyMod("CustomMod", gameDirectory, logFile);
```

---

## 📁 File Structure

### New Files Added:
```
Components/CastleStoryLauncher/
├── PresetManager.cs              # Gamemode preset system
├── JsonConfigEditor.cs           # JSON editing utilities
├── CsvDataEditor.cs              # CSV file editor
├── XmlConfigEditor.cs            # XML configuration editor
├── BricktronNameEditor.cs        # Name management system
├── LanguageFileEditor.cs         # Translation management
├── ModManager.cs                 # Mod management system
├── IModIntegration.cs            # Mod integration interface
├── ModDefinitions/
│   ├── LadderModDefinition.cs
│   └── MultiplayerModDefinition.cs
└── ModIntegrations/
    ├── FileModificationIntegration.cs
    ├── DLLInjectionIntegration.cs
    ├── AssetReplacementIntegration.cs
    ├── LuaInjectionIntegration.cs
    └── MemoryPatchingIntegration.cs
```

---

## 📊 CSV Data Editor

Comprehensive CSV editing system for name lists, resource data, and statistics.

### Features:
- **CRUD Operations**: Create, read, update, delete rows and cells
- **Headers Management**: Set and manage column headers
- **Sorting**: Sort by any column (ascending/descending)
- **Searching**: Find rows by content with flexible matching
- **Statistics**: Column statistics, value frequency counts
- **Duplicate Detection**: Find and remove duplicate entries
- **Validation**: Custom validation rules per column
- **Export**: Export to JSON format
- **Import**: Load from CSV files with custom delimiters

### Usage:
```csharp
// Load a CSV file
var csvEditor = new CsvDataEditor("names.csv", ',', hasHeaders: true);

// Get data
var headers = csvEditor.GetHeaders();
int rowCount = csvEditor.GetRowCount();
var row = csvEditor.GetRow(0);
var cell = csvEditor.GetCell(0, "Name");

// Edit data
csvEditor.SetCell(0, "Name", "NewName");
csvEditor.AddRow(new List<string> { "Value1", "Value2" });
csvEditor.DeleteRow(5);

// Sort and search
csvEditor.SortByColumn("Name", ascending: true);
var matches = csvEditor.SearchRows("search term");

// Find and remove duplicates
var duplicates = csvEditor.FindDuplicates(0); // column 0
csvEditor.RemoveDuplicates(0);

// Validate data
csvEditor.Validate(0, value => !string.IsNullOrEmpty(value), out var invalidRows);

// Export
csvEditor.SaveCsvFile("output.csv");
csvEditor.ExportToJson("output.json");
```

---

## 📝 XML Configuration Editor

Full-featured XML editing with XPath support and schema validation.

### Features:
- **XPath Queries**: Select elements using XPath expressions
- **Element Operations**: Add, remove, modify elements and attributes
- **Schema Validation**: Validate against XSD schemas with error reporting
- **Search**: Find elements by name, value, or attributes
- **Merge**: Combine multiple XML documents
- **XSLT Transform**: Apply XSLT transformations
- **Flattening**: Convert hierarchical XML to key-value pairs
- **Format**: Pretty-print or compact XML output

### Usage:
```csharp
// Load an XML file
var xmlEditor = new XmlConfigEditor("config.xml");

// Get values using XPath
var value = xmlEditor.GetValue("/config/settings/option");
var attribute = xmlEditor.GetAttribute("/config/settings", "enabled");

// Set values
xmlEditor.SetValue("/config/settings/option", "newValue");
xmlEditor.SetAttribute("/config/settings", "enabled", "true");

// Add/remove elements
xmlEditor.AddElement("/config/settings", "newOption", "defaultValue");
xmlEditor.RemoveElement("/config/settings/oldOption");

// Validate against schema
bool isValid = xmlEditor.ValidateAgainstSchema("schema.xsd", out var errors);
if (!isValid)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}

// Search
var matches = xmlEditor.SearchElements("search term", searchValues: true, searchAttributes: true);

// Get all paths
var allPaths = xmlEditor.GetAllElementPaths();

// Merge with another XML
var otherXml = new XmlConfigEditor("other.xml");
xmlEditor.MergeWith(otherXml, overwriteExisting: true);

// Transform
xmlEditor.TransformWith("transform.xslt", "output.xml");

// Save
xmlEditor.SaveXmlFile("config.xml", indent: true);
```

---

## 👤 Bricktron Names Editor

Enhanced name management system with 7 built-in categories and custom name support.

### Features:
- **Built-in Categories**:
  - Male: Traditional male names (30 names)
  - Female: Traditional female names (29 names)
  - Warrior: Combat-oriented names (23 names)
  - Builder: Construction-focused names (23 names)
  - Worker: Resource gathering names (19 names)
  - Funny: Humorous/quirky names (27 names)
  - Fantasy: Fantasy-themed names (29 names)

- **Custom Categories**: Create your own name categories
- **Name Generation**: Random name selection from categories
- **Validation**: Name length, format, and character rules
- **Duplicate Detection**: Find and remove duplicate names
- **Import/Export**: JSON format for sharing name collections
- **Statistics**: Count names per category
- **Search**: Find names across all categories

### Usage:
```csharp
// Create name editor
var nameEditor = new BricktronNameEditor(gameDirectory);

// Get categories
var categories = nameEditor.GetCategoryNames();
var maleNames = nameEditor.GetNamesFromCategory("Male");

// Add custom category
nameEditor.AddCategory("Steampunk", "Victorian-era steampunk names");
nameEditor.AddName("Steampunk", "Cogsworth");
nameEditor.AddName("Steampunk", "Gearsley");

// Generate random names
var randomName = nameEditor.GenerateRandomName("Warrior");
var multipleNames = nameEditor.GenerateRandomNames("Builder", 10);

// Validate names
bool isValid = nameEditor.ValidateName("TestName", out var error);

// Find and remove duplicates
var duplicates = nameEditor.FindDuplicates("Male");
nameEditor.RemoveDuplicates("Male");

// Search across all categories
var searchResults = nameEditor.SearchNames("iron");

// Import/Export
nameEditor.ExportCategory("Warrior", "warrior_names.json");
nameEditor.ImportCategory("custom_names.json");
nameEditor.ExportAllCategories("all_names.json");

// Statistics
var stats = nameEditor.GetStatistics();
// stats["Total Names"], stats["Total Categories"], etc.
```

---

## 🌍 Language File Editor

Multi-language translation management system supporting 10 languages.

### Features:
- **Supported Languages**: en, fr, de, es, it, pt, ru, ja, ko, zh
- **Translation Management**: Add, edit, remove translations
- **Category System**: Organize translations by category
- **Context Support**: Add context notes for translators
- **Search**: Find by key or translation text
- **Validation**: 
  - Missing translation detection
  - Placeholder consistency checking (%s, %d, {0}, {1})
  - Empty translation detection
- **Statistics**: Completion percentage per language
- **Import/Export**: CSV and JSON formats
- **Merge**: Combine translation files
- **Auto-translate Support**: Hook for machine translation

### Usage:
```csharp
// Create language editor
var langEditor = new LanguageFileEditor();

// Load language files
langEditor.LoadLanguageFile("english.txt", "en");
langEditor.LoadLanguageFile("french.txt", "fr");

// Add entries
langEditor.AddEntry("ui.button.start", category: "UI", context: "Start button label");
langEditor.SetTranslation("ui.button.start", "en", "Start");
langEditor.SetTranslation("ui.button.start", "fr", "Commencer");

// Get translations
var englishText = langEditor.GetTranslation("ui.button.start", "en");

// Search
var keysFound = langEditor.SearchKeys("button");
var translationsFound = langEditor.SearchTranslations("start", "en");

// Find missing translations
var missingInFrench = langEditor.FindMissingTranslations("fr");

// Validate
bool isValid = langEditor.ValidateTranslation("ui.button.start", "fr", out var errors);

// Statistics
var stats = langEditor.GetTranslationStatistics("fr");
// stats["Total Entries"], stats["Translated"], stats["Missing"], stats["Completion %"]

// Export/Import
langEditor.ExportToCSV("translations.csv");
langEditor.ImportFromCSV("translations.csv");
langEditor.SaveLanguageFile("french.txt", "fr", asJson: true);

// Merge with another translation file
var otherEditor = new LanguageFileEditor();
otherEditor.LoadLanguageFile("additional_translations.txt", "en");
langEditor.MergeWith(otherEditor, overwriteExisting: false);

// Auto-translate (with custom translator function)
langEditor.AutoTranslate("en", "fr", (text, targetLang) => {
    // Call your translation API here
    return TranslateAPI(text, targetLang);
});
```

---

## 🎯 Future Enhancements

### Planned Features (Medium Priority):
- Advanced multiplayer features (32+ players)
- Map editor enhancements with terrain tools
- UI improvements and modernization
- Performance optimizations
- Advanced AI behavior customization

### Planned Features (Low Priority):
- Advanced scripting support
- Community mod sharing platform
- Cross-platform support
- Steam Workshop integration
- Achievement system

---

## 📖 Additional Resources

- **TASKS.md**: Full task list with priorities
- **README.md**: Project overview and setup
- **PROJECT_STRUCTURE.md**: Codebase organization

---

## 🐛 Known Limitations

1. **Ladder System**: Deferred until Unity assets can be restored
2. **Schema Validation**: Basic implementation, can be enhanced
3. **Preset Application**: Simple text replacement for Lua files

---

## 💡 Tips

- **Backup First**: Always backup game files before applying mods
- **Test Presets**: Test custom presets in sandbox mode first
- **JSON Validation**: Validate JSON files after manual editing
- **Mod Order**: Apply mods in correct dependency order

---

*Last Updated: October 4, 2025*
*Version: 1.4.0*

