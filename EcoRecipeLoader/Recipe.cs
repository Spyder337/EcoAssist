using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRecipeLoader
{
    public class Recipe
    {
        public string Name { get; set; } = string.Empty;
        public CraftingTable Table { get; set; } = CraftingTable.Invalid;
        public RecipeItem MainProduct => Products[0];
        public List<RecipeItem> Products { get; set; } = new();
        public List<RecipeItem> Ingredients { get; set; } = new();
    }
}
