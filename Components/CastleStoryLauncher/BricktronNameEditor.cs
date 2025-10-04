using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CastleStoryLauncher
{
    public class NameCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<string> Names { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public bool IsCustom { get; set; } = false;
    }

    public class BricktronNameEditor
    {
        private readonly Dictionary<string, NameCategory> categories;
        private readonly string configDirectory;

        public BricktronNameEditor(string gameDirectory)
        {
            categories = new Dictionary<string, NameCategory>();
            configDirectory = Path.Combine(gameDirectory, "Info", "Lua");
            LoadDefaultCategories();
        }

        private void LoadDefaultCategories()
        {
            // Default Castle Story name categories
            categories["Male"] = new NameCategory
            {
                CategoryName = "Male",
                Description = "Traditional male bricktron names",
                Names = new List<string>
                {
                    "Alban", "Baldric", "Cedric", "Darian", "Edmund", "Felix", "Gareth", "Harold",
                    "Ivan", "Jasper", "Kelvin", "Leopold", "Magnus", "Nigel", "Oscar", "Percy",
                    "Quentin", "Reginald", "Sebastian", "Theodore", "Ulric", "Victor", "Winston",
                    "Xavier", "York", "Zachary", "Arthur", "Benedict", "Cornelius", "Dominic"
                }
            };

            categories["Female"] = new NameCategory
            {
                CategoryName = "Female",
                Description = "Traditional female bricktron names",
                Names = new List<string>
                {
                    "Adelaide", "Beatrice", "Catherine", "Diana", "Eleanor", "Felicity", "Genevieve",
                    "Helena", "Isabelle", "Josephine", "Katherine", "Lydia", "Margaret", "Natalie",
                    "Olivia", "Penelope", "Quinn", "Rosalind", "Sophia", "Theodora", "Ursula",
                    "Victoria", "Winifred", "Xena", "Yvonne", "Zelda", "Amelia", "Bridget", "Charlotte"
                }
            };

            categories["Warrior"] = new NameCategory
            {
                CategoryName = "Warrior",
                Description = "Names for combat-oriented bricktrons",
                Names = new List<string>
                {
                    "Axebeard", "Battlecry", "Crusher", "Defender", "Ironhide", "Shieldwall",
                    "Stormbringer", "Thunderfist", "Warhammer", "Blademaster", "Sentinel",
                    "Vanguard", "Champion", "Gladiator", "Berserker", "Paladin", "Knight",
                    "Warrior", "Fighter", "Soldier", "Guardian", "Protector", "Defender"
                }
            };

            categories["Builder"] = new NameCategory
            {
                CategoryName = "Builder",
                Description = "Names for construction-focused bricktrons",
                Names = new List<string>
                {
                    "Architect", "Bricklayer", "Constructor", "Designer", "Engineer", "Fabricator",
                    "Groundbreaker", "Hammer", "Inventor", "Joiner", "Keystone", "Mason",
                    "Planner", "Quarryman", "Stonemason", "Tinkerer", "Woodworker", "Carpenter",
                    "Builder", "Crafter", "Maker", "Artisan", "Craftsman"
                }
            };

            categories["Worker"] = new NameCategory
            {
                CategoryName = "Worker",
                Description = "Names for resource gathering bricktrons",
                Names = new List<string>
                {
                    "Chopper", "Digger", "Farmer", "Forager", "Gatherer", "Harvester", "Lumberjack",
                    "Miner", "Pickaxe", "Quarrier", "Sawyer", "Stonecutter", "Woodcutter",
                    "Collector", "Provider", "Supplier", "Laborer", "Helper", "Assistant"
                }
            };

            categories["Funny"] = new NameCategory
            {
                CategoryName = "Funny",
                Description = "Humorous and quirky names",
                Names = new List<string>
                {
                    "Bob", "Steve", "Jeff", "Kevin", "Gary", "Larry", "Barry", "Terry",
                    "Jerry", "Perry", "Harry", "Brick McBrickface", "Blocky", "Cubey",
                    "Stumpy", "Chunky", "Clumsy", "Dizzy", "Goofy", "Silly", "Wobbly",
                    "Bumblefist", "Dropstone", "Tripsy", "Oopsie", "Bonk", "Clonk"
                }
            };

            categories["Fantasy"] = new NameCategory
            {
                CategoryName = "Fantasy",
                Description = "Fantasy-themed names",
                Names = new List<string>
                {
                    "Aragorn", "Boromir", "Celeborn", "Denethor", "Elrond", "Faramir", "Gandalf",
                    "Gimli", "Legolas", "Merry", "Pippin", "Frodo", "Sam", "Bilbo", "Thorin",
                    "Balin", "Dwalin", "Fili", "Kili", "Oin", "Gloin", "Dori", "Nori", "Ori",
                    "Bifur", "Bofur", "Bombur", "Thrain", "Thror"
                }
            };
        }

        public List<string> GetCategoryNames()
        {
            return categories.Keys.ToList();
        }

        public NameCategory? GetCategory(string categoryName)
        {
            return categories.ContainsKey(categoryName) ? categories[categoryName] : null;
        }

        public List<string> GetNamesFromCategory(string categoryName)
        {
            return categories.ContainsKey(categoryName) 
                ? new List<string>(categories[categoryName].Names) 
                : new List<string>();
        }

        public bool AddCategory(string categoryName, string description)
        {
            if (categories.ContainsKey(categoryName))
                return false;

            categories[categoryName] = new NameCategory
            {
                CategoryName = categoryName,
                Description = description,
                Names = new List<string>(),
                IsCustom = true
            };

            return true;
        }

        public bool RemoveCategory(string categoryName)
        {
            if (!categories.ContainsKey(categoryName))
                return false;

            // Don't allow removing default categories
            if (!categories[categoryName].IsCustom)
                return false;

            categories.Remove(categoryName);
            return true;
        }

        public bool AddName(string categoryName, string name)
        {
            if (!categories.ContainsKey(categoryName))
                return false;

            if (categories[categoryName].Names.Contains(name))
                return false;

            categories[categoryName].Names.Add(name);
            return true;
        }

        public bool RemoveName(string categoryName, string name)
        {
            if (!categories.ContainsKey(categoryName))
                return false;

            return categories[categoryName].Names.Remove(name);
        }

        public bool ValidateName(string name, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Name cannot be empty";
                return false;
            }

            if (name.Length < 2)
            {
                error = "Name must be at least 2 characters";
                return false;
            }

            if (name.Length > 50)
            {
                error = "Name must not exceed 50 characters";
                return false;
            }

            if (!char.IsLetter(name[0]))
            {
                error = "Name must start with a letter";
                return false;
            }

            return true;
        }

        public List<string> FindDuplicates(string categoryName)
        {
            if (!categories.ContainsKey(categoryName))
                return new List<string>();

            var names = categories[categoryName].Names;
            var duplicates = names.GroupBy(n => n.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            return duplicates;
        }

        public bool RemoveDuplicates(string categoryName)
        {
            if (!categories.ContainsKey(categoryName))
                return false;

            var uniqueNames = categories[categoryName].Names
                .GroupBy(n => n.ToLower())
                .Select(g => g.First())
                .ToList();

            categories[categoryName].Names = uniqueNames;
            return true;
        }

        public bool ExportCategory(string categoryName, string filePath)
        {
            try
            {
                if (!categories.ContainsKey(categoryName))
                    return false;

                var category = categories[categoryName];
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(category, options);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ImportCategory(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var category = JsonSerializer.Deserialize<NameCategory>(json);

                if (category != null && !string.IsNullOrEmpty(category.CategoryName))
                {
                    categories[category.CategoryName] = category;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ExportAllCategories(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(categories, options);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ImportAllCategories(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var imported = JsonSerializer.Deserialize<Dictionary<string, NameCategory>>(json);

                if (imported != null)
                {
                    foreach (var kvp in imported)
                    {
                        categories[kvp.Key] = kvp.Value;
                    }
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateRandomName(string categoryName)
        {
            if (!categories.ContainsKey(categoryName) || categories[categoryName].Names.Count == 0)
                return "Bricktron";

            var random = new Random();
            var names = categories[categoryName].Names;
            return names[random.Next(names.Count)];
        }

        public List<string> GenerateRandomNames(string categoryName, int count)
        {
            var result = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                result.Add(GenerateRandomName(categoryName));
            }

            return result;
        }

        public bool SortCategory(string categoryName, bool ascending = true)
        {
            if (!categories.ContainsKey(categoryName))
                return false;

            categories[categoryName].Names = ascending
                ? categories[categoryName].Names.OrderBy(n => n).ToList()
                : categories[categoryName].Names.OrderByDescending(n => n).ToList();

            return true;
        }

        public List<string> SearchNames(string searchTerm)
        {
            var results = new List<string>();

            foreach (var category in categories.Values)
            {
                var matches = category.Names
                    .Where(n => n.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(n => $"{category.CategoryName}: {n}");
                
                results.AddRange(matches);
            }

            return results;
        }

        public Dictionary<string, int> GetStatistics()
        {
            var stats = new Dictionary<string, int>();

            foreach (var category in categories)
            {
                stats[category.Key] = category.Value.Names.Count;
            }

            stats["Total Names"] = categories.Values.Sum(c => c.Names.Count);
            stats["Total Categories"] = categories.Count;

            return stats;
        }
    }
}

