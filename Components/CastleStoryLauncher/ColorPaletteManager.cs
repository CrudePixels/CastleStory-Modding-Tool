using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;

namespace CastleStoryLauncher
{
    public class ColorPalette
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public List<FactionColor> Colors { get; set; } = new List<FactionColor>();
        public bool IsDefault { get; set; } = false;
        public bool IsCustom { get; set; } = true;

        public ColorPalette()
        {
        }

        public ColorPalette(string name, string description = "")
        {
            Name = name;
            Description = description;
            CreatedDate = DateTime.Now;
            LastModified = DateTime.Now;
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                LastModified = DateTime.Now;
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save color palette: {ex.Message}", ex);
            }
        }

        public static ColorPalette LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Color palette file not found: {filePath}");
                }

                var json = File.ReadAllText(filePath);
                var palette = JsonSerializer.Deserialize<ColorPalette>(json);
                
                if (palette == null)
                {
                    throw new Exception("Failed to deserialize color palette");
                }

                return palette;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load color palette: {ex.Message}", ex);
            }
        }

        public ColorPalette Clone()
        {
            var clone = new ColorPalette
            {
                Name = $"{Name} (Copy)",
                Description = Description,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now,
                Colors = new List<FactionColor>(Colors.Select(c => new FactionColor 
                { 
                    Name = c.Name, 
                    Color = c.Color 
                })),
                IsDefault = false,
                IsCustom = true
            };
            return clone;
        }

        public void AddColor(FactionColor color)
        {
            if (color == null)
            {
                throw new ArgumentNullException(nameof(color));
            }

            // Check if color with same name already exists
            if (Colors.Any(c => c.Name.Equals(color.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Color with name '{color.Name}' already exists in this palette");
            }

            Colors.Add(color);
            LastModified = DateTime.Now;
        }

        public void RemoveColor(FactionColor color)
        {
            if (color == null)
            {
                throw new ArgumentNullException(nameof(color));
            }

            Colors.Remove(color);
            LastModified = DateTime.Now;
        }

        public void UpdateColor(FactionColor oldColor, FactionColor newColor)
        {
            if (oldColor == null || newColor == null)
            {
                throw new ArgumentNullException("Color cannot be null");
            }

            var index = Colors.IndexOf(oldColor);
            if (index >= 0)
            {
                Colors[index] = newColor;
                LastModified = DateTime.Now;
            }
        }

        public FactionColor? GetColorByName(string name)
        {
            return Colors.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return $"{Name} ({Colors.Count} colors)";
        }
    }

    public class ColorPaletteManager
    {
        private readonly string palettesDirectory;
        private readonly List<ColorPalette> palettes;

        public ColorPaletteManager(string palettesDirectory)
        {
            this.palettesDirectory = palettesDirectory;
            this.palettes = new List<ColorPalette>();
            
            // Ensure palettes directory exists
            if (!Directory.Exists(palettesDirectory))
            {
                Directory.CreateDirectory(palettesDirectory);
            }
            
            LoadPalettes();
            CreateDefaultPalettes();
        }

        public IReadOnlyList<ColorPalette> Palettes => palettes.AsReadOnly();

        public ColorPalette? CurrentPalette { get; private set; }

        public event EventHandler<ColorPalette>? PaletteChanged;
        public event EventHandler<ColorPalette>? PaletteSaved;
        public event EventHandler<ColorPalette>? PaletteDeleted;

        private void LoadPalettes()
        {
            palettes.Clear();
            
            try
            {
                var paletteFiles = Directory.GetFiles(palettesDirectory, "*.json");
                
                foreach (var file in paletteFiles)
                {
                    try
                    {
                        var palette = ColorPalette.LoadFromFile(file);
                        palettes.Add(palette);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load palette from {file}: {ex.Message}");
                    }
                }

                // Sort palettes by name
                palettes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading palettes: {ex.Message}");
            }
        }

        private void CreateDefaultPalettes()
        {
            // Create default Castle Story palette if it doesn't exist
            if (!palettes.Any(p => p.Name.Equals("Castle Story Default", StringComparison.OrdinalIgnoreCase)))
            {
                var defaultPalette = CreateCastleStoryDefaultPalette();
                palettes.Insert(0, defaultPalette);
                SavePalette(defaultPalette);
            }

            // Create vibrant palette if it doesn't exist
            if (!palettes.Any(p => p.Name.Equals("Vibrant Colors", StringComparison.OrdinalIgnoreCase)))
            {
                var vibrantPalette = CreateVibrantPalette();
                palettes.Add(vibrantPalette);
                SavePalette(vibrantPalette);
            }

            // Create pastel palette if it doesn't exist
            if (!palettes.Any(p => p.Name.Equals("Pastel Colors", StringComparison.OrdinalIgnoreCase)))
            {
                var pastelPalette = CreatePastelPalette();
                palettes.Add(pastelPalette);
                SavePalette(pastelPalette);
            }

            // Create military palette if it doesn't exist
            if (!palettes.Any(p => p.Name.Equals("Military Colors", StringComparison.OrdinalIgnoreCase)))
            {
                var militaryPalette = CreateMilitaryPalette();
                palettes.Add(militaryPalette);
                SavePalette(militaryPalette);
            }

            // Set default palette if none is set
            if (CurrentPalette == null)
            {
                CurrentPalette = palettes.FirstOrDefault(p => p.IsDefault);
            }
        }

        private ColorPalette CreateCastleStoryDefaultPalette()
        {
            var palette = new ColorPalette("Castle Story Default", "Default Castle Story faction colors")
            {
                IsDefault = true,
                IsCustom = false
            };

            var defaultColors = new[]
            {
                new FactionColor { Name = "Blue", Color = Colors.Blue },
                new FactionColor { Name = "Green", Color = Colors.Green },
                new FactionColor { Name = "Orange", Color = Colors.Orange },
                new FactionColor { Name = "Purple", Color = Colors.Purple },
                new FactionColor { Name = "Red", Color = Colors.Red },
                new FactionColor { Name = "Yellow", Color = Colors.Yellow },
                new FactionColor { Name = "Cyan", Color = Colors.Cyan },
                new FactionColor { Name = "Magenta", Color = Colors.Magenta },
                new FactionColor { Name = "Lime", Color = Colors.Lime },
                new FactionColor { Name = "Pink", Color = Colors.Pink },
                new FactionColor { Name = "Teal", Color = Colors.Teal },
                new FactionColor { Name = "Indigo", Color = Colors.Indigo },
                new FactionColor { Name = "Brown", Color = Colors.Brown },
                new FactionColor { Name = "Gray", Color = Colors.Gray },
                new FactionColor { Name = "Gold", Color = Colors.Gold },
                new FactionColor { Name = "Silver", Color = Colors.Silver },
                new FactionColor { Name = "Crimson", Color = Colors.Crimson },
                new FactionColor { Name = "Forest Green", Color = Colors.ForestGreen },
                new FactionColor { Name = "Navy", Color = Colors.Navy },
                new FactionColor { Name = "Maroon", Color = Colors.Maroon },
                new FactionColor { Name = "Olive", Color = Colors.Olive },
                new FactionColor { Name = "Turquoise", Color = Colors.Turquoise },
                new FactionColor { Name = "Violet", Color = Colors.Violet },
                new FactionColor { Name = "Coral", Color = Colors.Coral },
                new FactionColor { Name = "Khaki", Color = Colors.Khaki },
                new FactionColor { Name = "Salmon", Color = Colors.Salmon },
                new FactionColor { Name = "Lavender", Color = Colors.Lavender },
                new FactionColor { Name = "Mint", Color = Colors.MintCream },
                new FactionColor { Name = "Peach", Color = Colors.PeachPuff },
                new FactionColor { Name = "Sky Blue", Color = Colors.SkyBlue }
            };

            palette.Colors.AddRange(defaultColors);
            return palette;
        }

        private ColorPalette CreateVibrantPalette()
        {
            var palette = new ColorPalette("Vibrant Colors", "Bright and vibrant colors for high contrast")
            {
                IsCustom = false
            };

            var vibrantColors = new[]
            {
                new FactionColor { Name = "Electric Blue", Color = Color.FromRgb(0, 123, 255) },
                new FactionColor { Name = "Neon Green", Color = Color.FromRgb(40, 255, 0) },
                new FactionColor { Name = "Hot Pink", Color = Color.FromRgb(255, 20, 147) },
                new FactionColor { Name = "Electric Purple", Color = Color.FromRgb(138, 43, 226) },
                new FactionColor { Name = "Bright Orange", Color = Color.FromRgb(255, 165, 0) },
                new FactionColor { Name = "Lime Green", Color = Color.FromRgb(50, 205, 50) },
                new FactionColor { Name = "Cyan", Color = Color.FromRgb(0, 255, 255) },
                new FactionColor { Name = "Magenta", Color = Color.FromRgb(255, 0, 255) },
                new FactionColor { Name = "Yellow", Color = Color.FromRgb(255, 255, 0) },
                new FactionColor { Name = "Red", Color = Color.FromRgb(255, 0, 0) },
                new FactionColor { Name = "Deep Blue", Color = Color.FromRgb(0, 0, 139) },
                new FactionColor { Name = "Forest Green", Color = Color.FromRgb(34, 139, 34) }
            };

            palette.Colors.AddRange(vibrantColors);
            return palette;
        }

        private ColorPalette CreatePastelPalette()
        {
            var palette = new ColorPalette("Pastel Colors", "Soft and gentle pastel colors")
            {
                IsCustom = false
            };

            var pastelColors = new[]
            {
                new FactionColor { Name = "Baby Blue", Color = Color.FromRgb(173, 216, 230) },
                new FactionColor { Name = "Mint Green", Color = Color.FromRgb(152, 251, 152) },
                new FactionColor { Name = "Rose Pink", Color = Color.FromRgb(255, 182, 193) },
                new FactionColor { Name = "Lavender", Color = Color.FromRgb(230, 230, 250) },
                new FactionColor { Name = "Peach", Color = Color.FromRgb(255, 218, 185) },
                new FactionColor { Name = "Lemon Chiffon", Color = Color.FromRgb(255, 250, 205) },
                new FactionColor { Name = "Light Cyan", Color = Color.FromRgb(224, 255, 255) },
                new FactionColor { Name = "Thistle", Color = Color.FromRgb(216, 191, 216) },
                new FactionColor { Name = "Light Yellow", Color = Color.FromRgb(255, 255, 224) },
                new FactionColor { Name = "Light Coral", Color = Color.FromRgb(240, 128, 128) },
                new FactionColor { Name = "Powder Blue", Color = Color.FromRgb(176, 224, 230) },
                new FactionColor { Name = "Light Green", Color = Color.FromRgb(144, 238, 144) }
            };

            palette.Colors.AddRange(pastelColors);
            return palette;
        }

        private ColorPalette CreateMilitaryPalette()
        {
            var palette = new ColorPalette("Military Colors", "Military and tactical color schemes")
            {
                IsCustom = false
            };

            var militaryColors = new[]
            {
                new FactionColor { Name = "Army Green", Color = Color.FromRgb(75, 83, 32) },
                new FactionColor { Name = "Navy Blue", Color = Color.FromRgb(0, 0, 128) },
                new FactionColor { Name = "Desert Tan", Color = Color.FromRgb(210, 180, 140) },
                new FactionColor { Name = "Forest Camo", Color = Color.FromRgb(34, 61, 38) },
                new FactionColor { Name = "Urban Gray", Color = Color.FromRgb(128, 128, 128) },
                new FactionColor { Name = "Maroon", Color = Color.FromRgb(128, 0, 0) },
                new FactionColor { Name = "Olive Drab", Color = Color.FromRgb(107, 142, 35) },
                new FactionColor { Name = "Charcoal", Color = Color.FromRgb(54, 69, 79) },
                new FactionColor { Name = "Rust", Color = Color.FromRgb(183, 65, 14) },
                new FactionColor { Name = "Steel Blue", Color = Color.FromRgb(70, 130, 180) },
                new FactionColor { Name = "Dark Green", Color = Color.FromRgb(0, 100, 0) },
                new FactionColor { Name = "Brown", Color = Color.FromRgb(139, 69, 19) }
            };

            palette.Colors.AddRange(militaryColors);
            return palette;
        }

        public ColorPalette CreatePalette(string name, string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Palette name cannot be empty", nameof(name));
            }

            if (palettes.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Palette with name '{name}' already exists");
            }

            var palette = new ColorPalette(name, description);
            palettes.Add(palette);
            SavePalette(palette);
            
            return palette;
        }

        public void SavePalette(ColorPalette palette)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            try
            {
                var fileName = SanitizeFileName(palette.Name) + ".json";
                var filePath = Path.Combine(palettesDirectory, fileName);
                palette.SaveToFile(filePath);
                
                PaletteSaved?.Invoke(this, palette);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save palette '{palette.Name}': {ex.Message}", ex);
            }
        }

        public void DeletePalette(ColorPalette palette)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            if (!palette.IsCustom)
            {
                throw new InvalidOperationException("Cannot delete built-in palettes");
            }

            try
            {
                var fileName = SanitizeFileName(palette.Name) + ".json";
                var filePath = Path.Combine(palettesDirectory, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                palettes.Remove(palette);
                PaletteDeleted?.Invoke(this, palette);

                // If we deleted the current palette, switch to default
                if (CurrentPalette == palette)
                {
                    CurrentPalette = palettes.FirstOrDefault(p => p.IsDefault);
                    PaletteChanged?.Invoke(this, CurrentPalette!);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete palette '{palette.Name}': {ex.Message}", ex);
            }
        }

        public void SetCurrentPalette(ColorPalette palette)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            if (!palettes.Contains(palette))
            {
                throw new InvalidOperationException("Palette is not in the palettes list");
            }

            CurrentPalette = palette;
            PaletteChanged?.Invoke(this, palette);
        }

        public ColorPalette? GetPaletteByName(string name)
        {
            return palettes.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ColorPalette ClonePalette(ColorPalette palette, string newName)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("New palette name cannot be empty", nameof(newName));
            }

            if (palettes.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Palette with name '{newName}' already exists");
            }

            var clone = palette.Clone();
            clone.Name = newName;
            palettes.Add(clone);
            SavePalette(clone);
            
            return clone;
        }

        public void SetAsDefault(ColorPalette palette)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            // Remove default flag from all palettes
            foreach (var p in palettes)
            {
                p.IsDefault = false;
            }

            // Set the selected palette as default
            palette.IsDefault = true;
            SavePalette(palette);
        }

        public void RefreshPalettes()
        {
            LoadPalettes();
            CreateDefaultPalettes();
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public void ExportPalette(ColorPalette palette, string exportPath)
        {
            if (palette == null)
            {
                throw new ArgumentNullException(nameof(palette));
            }

            if (string.IsNullOrWhiteSpace(exportPath))
            {
                throw new ArgumentException("Export path cannot be empty", nameof(exportPath));
            }

            try
            {
                palette.SaveToFile(exportPath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export palette '{palette.Name}': {ex.Message}", ex);
            }
        }

        public ColorPalette ImportPalette(string importPath)
        {
            if (string.IsNullOrWhiteSpace(importPath))
            {
                throw new ArgumentException("Import path cannot be empty", nameof(importPath));
            }

            if (!File.Exists(importPath))
            {
                throw new FileNotFoundException($"Import file not found: {importPath}");
            }

            try
            {
                var palette = ColorPalette.LoadFromFile(importPath);
                
                // Check if palette with same name already exists
                var existingPalette = GetPaletteByName(palette.Name);
                if (existingPalette != null)
                {
                    // Generate unique name
                    var counter = 1;
                    var baseName = palette.Name;
                    do
                    {
                        palette.Name = $"{baseName} ({counter})";
                        counter++;
                    } while (GetPaletteByName(palette.Name) != null);
                }

                palette.IsCustom = true;
                palettes.Add(palette);
                SavePalette(palette);
                
                return palette;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to import palette: {ex.Message}", ex);
            }
        }
    }
}
