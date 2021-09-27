using System;
using System.Collections.Generic;
using EcoRecipeLoader;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace EcoCalc
{
    public class RecipeManager
    {
        public static Dictionary<CraftingTable, List<Recipe>> RecipesByTable { get; set; } = new();
        public static Dictionary<string, Recipe> RecipesByName { get; set; } = new();
        public static Dictionary<int, Recipe> RecipeById { get; set; } = new();
        public static Dictionary<string, List<string>> Tags { get; set; } = new();
        public static int RecipeLevel = 1;
        [Range(1,3)]
        public static string RecipeLevelString => GetLevelString();

        private static string GetLevelString()
        {
            if (RecipeLevel == 1) return string.Empty;
            else return $"Lv{RecipeLevel}";
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

        public RecipeManager()
        {
            LoadRecipes();
        }

        private void LoadRecipes()
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
        }

        internal Dictionary<string, float> CraftRecipe(string recipeName, int amount, IRecipeStrategy strategy)
        {
            return strategy.CalculateCost(recipeName, amount);
        }
    }
}
