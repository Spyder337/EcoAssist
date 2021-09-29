using EcoRecipeLoader;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace EcoCalc
{
    public class RecipeTreeNode
    {
        public RecipeTreeNode? Parent { get; set; } = null;
        public List<RecipeTreeNode> Children { get; private set; } = new();
        public bool IsRoot => Parent == null;
        public string Name { get; set; }
        public Recipe? ItemRecipe { get; set; }
        public RecipeItem Item { get; set; }
        public double Quantity { get; set; }

        public RecipeTreeNode(RecipeItem item, double quantity, RecipeTreeNode parent = null)
        {
            Name = item.Name;
            if(RecipeManager.HasRecipe(item.Name))
                ItemRecipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[item.Name]);
            Parent = parent;
            Item = item;
            Quantity = quantity;

            if (ItemRecipe == null) return;

            foreach (var ingredient in ItemRecipe.Ingredients)
            {
                double numCrafts;
                if (RecipeManager.HasRecipe(ingredient.Name))
                {
                    var newRecipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[ingredient.Name]);
                    numCrafts = ingredient.Quantity / ItemRecipe.MainProduct.Quantity;
                }
                else
                {
                    numCrafts = ingredient.Quantity / ItemRecipe.MainProduct.Quantity;
                }
                Children.Add(new RecipeTreeNode(ingredient, numCrafts * quantity, this));
            }
        }

        private void ProcessRecipe(ref Dictionary<string, RecipeItem> recipes)
        {
            if (!IsRoot || IsValidSubRecipe(ItemRecipe))
            {
                string name = Name;
                if (ItemRecipe?.MainProduct.Name != null) name = ItemRecipe.MainProduct.Name;
                if (!recipes.ContainsKey(name))
                {
                    var updatedItem = Item;
                    updatedItem.Quantity = Quantity;
                    recipes.Add(name, updatedItem);
                }
                else
                {
                    recipes[name].Quantity += Quantity;
                }
            }

            if (!IsRoot && !Parent.IsRoot) return;

            if (IsRoot)
            {
                Console.WriteLine($"Item: {Name}");
                Console.WriteLine($"Quantity: {Quantity}");
                Console.WriteLine();
                Console.WriteLine("Resource Totals: ");
            }
            else
            {
                Console.WriteLine($"{Name, -40} {Quantity, -40}");
            }
        }

        private bool IsValidSubRecipe(Recipe itemRecipe)
        {
            if (itemRecipe == null) return true;
            if (RecipeManager.Tags["Ingredient"].Contains(itemRecipe.MainProduct.Name)) return true;
            return false;
        }

        public void ProcessRecipe(ref Dictionary<string, RecipeItem> recipes, int maxDepth, int currentDepth = 0, bool ignoreDepth = true, bool verbose = true)
        {
            ProcessRecipe(ref recipes);

            foreach (var child in Children)
            {
                if (ignoreDepth)
                {
                    child.ProcessRecipe(ref recipes, maxDepth, ignoreDepth: true);
                }
                else if (currentDepth < maxDepth)
                {
                    child.ProcessRecipe(ref recipes, maxDepth, currentDepth + 1);
                }
            }

            if (IsRoot && verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"Full crafting list: ");
                Console.WriteLine($"{"Item",-40} Quantity");
                foreach (var item in recipes)
                {
                    Console.WriteLine($"{item.Key, -40} {item.Value.Quantity, -40:F}");
                }
            }
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
}
