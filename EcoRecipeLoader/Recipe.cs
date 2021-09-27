using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace EcoRecipeLoader
{
    public class Recipe
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; } = 0;
        public CraftingTable Table { get; set; } = CraftingTable.Invalid;
        public RecipeItem MainProduct;
        public List<RecipeItem> Products { get; set; } = new();
        public List<RecipeItem> Ingredients { get; set; } = new();

        public double GetProductionRatio(string ingredientName)
        {
            var ingredient = Ingredients.Where(i => i.Name == ingredientName).First();
            return ingredient.Quantity / MainProduct.Quantity;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
