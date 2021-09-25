using EcoRecipeLoader;
using Newtonsoft.Json;

namespace EcoCalc
{
    internal class RecipeManager
    {
        public Dictionary<CraftingTable, List<Recipe>> RecipesByTable { get; set; } = new();
        public Dictionary<string, Recipe> RecipesByName { get; set; } = new();
        public Dictionary<int, Recipe> RecipeById { get; set; } = new();
        public Dictionary<string, List<string>> Tags { get; set; } = new();

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

        public Dictionary<string, float> CraftRecipe(string recipeName, int amount)
        {
            var results = CraftRecipe(recipeName);
            var itemNames = results.Keys;
            foreach(var item in itemNames)
            {
                results[item] *= amount;
            }
            return results;
        }

        public Dictionary<string, float> CraftRecipe(string recipeName)
        {
            if (!RecipesByName.ContainsKey(recipeName))
            {
                Console.WriteLine("Item recipe not found.");
            }
            Dictionary<string, float> itemQuantities = new();
            var recipe = RecipesByName[recipeName];

            //Loop through current recipes
            foreach(var recipeItem in recipe.Ingredients)
            {
                //If the item does not have a recipe add cost
                if (!RecipesByName.ContainsKey(recipeItem.Name))
                {
                    itemQuantities.Add(recipeItem.Name, recipeItem.Quantity);
                }
                //Otherwise recursively go down the crafting cost.
                else
                {
                    CraftRecipe(RecipesByName[recipeItem.Name], ref itemQuantities, recipeItem.Quantity);
                }
            }

            return itemQuantities;
        }

        private void CraftRecipe(Recipe recipe, ref Dictionary<string, float> itemQuantities, float times)
        {
            //Loop through current recipes
            foreach (var recipeItem in recipe.Ingredients)
            {
                //If the item does not have a recipe add cost
                if (!RecipesByName.ContainsKey(recipeItem.Name))
                {
                    if (!itemQuantities.ContainsKey(recipeItem.Name))
                    {
                        itemQuantities.Add(recipeItem.Name, recipeItem.Quantity * times);
                    }
                    else
                    {
                        itemQuantities[recipeItem.Name] += recipeItem.Quantity * times;
                    }
                }
                //Otherwise recursively go down the crafting cost.
                else
                {
                    CraftRecipe(RecipesByName[recipeItem.Name], ref itemQuantities, recipeItem.Quantity * times);
                }
            }
        }
    }
}
