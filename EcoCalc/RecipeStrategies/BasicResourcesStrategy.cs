using EcoRecipeLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoCalc.RecipeStrategies
{
    internal class BasicResourcesStrategy : IRecipeStrategy
    {
        public int MaxDepth = 1;
        public Dictionary<string, float> CalculateCost(string recipeName, int amount)
        {
            if (!RecipeManager.RecipesByName.ContainsKey(recipeName))
            {
                Console.WriteLine("Item recipe not found.");
            }
            Dictionary<string, float> itemQuantities = new();
            var recipe = RecipeManager.RecipesByName[recipeName];

            CalculateCost(recipe, ref itemQuantities, 1);
            var itemNames = itemQuantities.Keys;
            foreach (var item in itemNames)
            {
                itemQuantities[item] *= amount;
            }
            return itemQuantities;
        }

        private void CalculateCost(Recipe recipe, ref Dictionary<string, float> itemQuantities, float times)
        {
            /*
            //Loop through current recipes
            foreach (var recipeItem in recipe.Ingredients)
            {
                //If the item does not have a recipe add cost
                if (!RecipeManager.RecipesByName.ContainsKey(recipeItem.Name))
                {
                    IRecipeStrategy.AddItem(recipeItem.Name, times * recipeItem.Quantity, ref itemQuantities);
                }
                //Otherwise recursively go down the crafting cost.
                else
                {
                    var newRec = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[recipeItem.Name]);
                    CalculateCost(newRec, ref itemQuantities, newRec.MainProduct.Quantity * times);
                }
            }
            */
        }
    }
}
