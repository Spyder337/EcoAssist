using System;
using System.Collections.Generic;
using EcoRecipeLoader;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EcoCalc
{
    public static class RecipeManager
    {

        public static readonly string TableUgradesPath = Path.Combine(Program.SaveDir, "TableUpgrades.json");
        public static Dictionary<CraftingTable, List<Recipe>> RecipesByTable { get; set; } = new();
        public static Dictionary<string, Recipe> RecipesByName { get; set; } = new();
        public static Dictionary<int, Recipe> RecipeById { get; set; } = new();
        public static Dictionary<string, List<string>> Tags { get; set; } = new();
        public static SortedDictionary<CraftingTable, int> TableUpgrades { get; set; } = new();
        [Range(1, 3)]
        public static int RecipeLevel = 1;
        
        public static double GetUpgradeValue(int upgradeLevel)
        {
            return upgradeLevel switch
            {
                1 => .1,
                2 => .25,
                3 => .40,
                4 => .45,
                5 => .5,
                _ => 1
            };
        }

        public static Recipe GetActiveRecipe(Recipe recipe)
        {
            if (RecipeLevel == 1) return recipe;
            else if(RecipeLevel == 2)
            {
                return recipe.Table switch
                {
                    CraftingTable.Arrastra => RecipesByTable[CraftingTable.StampMill].Where(r => r.MainProduct.Name == recipe.MainProduct.Name).First(),
                    CraftingTable.ScreeningMachine => RecipesByTable[CraftingTable.SensorBasedBeltSorter].Where(r => r.MainProduct.Name == recipe.MainProduct.Name).First(),
                    _ => recipe
                };
            }
            else
            {
                return recipe.Table switch
                {
                    CraftingTable.Arrastra => RecipesByTable[CraftingTable.JawCrusher].Where(r => r.MainProduct.Name == recipe.MainProduct.Name).First(),
                    _ => throw new Exception("Invalid table entry.")
                };
            }
        }

        public static bool HasRecipe(string recipeName)
        {
            return RecipesByName.ContainsKey(recipeName);
        }

        public static bool HasRecipe(string recipeName, CraftingTable table)
        {
            return RecipesByTable[table].Any(r => r.Name == recipeName);
        }

        static RecipeManager()
        {
            LoadRecipes();
        }

        private static void LoadRecipes()
        {
            try
            {
                using var f = File.OpenRead(Program.RecipesFile);
                using var sr = new StreamReader(f);
                string json = sr.ReadToEnd();
                RecipesByTable = JsonConvert.DeserializeObject<Dictionary<CraftingTable, List<Recipe>>>(json);

                foreach(var pair in RecipesByTable)
                {
                    foreach(var recipe in pair.Value)
                    {
                        RecipeById.Add(recipe.Id, recipe);
                        RecipesByName.Add(recipe.Name, recipe);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error reading in the recipe file.");
            }

            try
            {
                using var f = File.OpenRead(Program.TagsFile);
                using var sr = new StreamReader(f);
                string json = sr.ReadToEnd();
                Tags = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            }
            catch
            {
                Console.WriteLine("Error reading in tags.");
            }

            try
            {
                if (File.Exists(TableUgradesPath))
                {
                    using var f = File.OpenRead(TableUgradesPath);
                    using var sr = new StreamReader(f);
                    var json = sr.ReadToEnd();
                    TableUpgrades = JsonConvert.DeserializeObject<SortedDictionary<CraftingTable, int>>(json);
                }
                else
                {
                    foreach (var craftingTable in RecipesByTable)
                    {
                        TableUpgrades.Add(craftingTable.Key, 0);
                    }
                    using var f = File.Create(TableUgradesPath);
                    using var sw = new StreamWriter(f);
                    var json = JsonConvert.SerializeObject(TableUpgrades, Formatting.Indented);
                    sw.WriteLine(json);
                }
            }
            catch
            {
                Console.WriteLine($"Error! Failed to load {TableUgradesPath}");
            }
        }
    }
}
