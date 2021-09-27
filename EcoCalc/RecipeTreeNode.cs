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

        public RecipeTreeNode(Recipe itemRecipe, double quantity = 1, RecipeTreeNode parent = null)
        {
            Name = itemRecipe.Name;
            ItemRecipe = itemRecipe;
            Parent = parent;
            Quantity = quantity;
            Item = ItemRecipe.MainProduct;
            foreach (var item in ItemRecipe.Ingredients)
            {
                AddChild(item,  item.Quantity);
            }
        }

        public RecipeTreeNode(RecipeItem item, double quantity = 1, RecipeTreeNode parent = null)
        {
            Name = item.Name;
            Item = item;
            Quantity = quantity;
            Parent = parent;
        }

        private void ProcessRecipe(ref Dictionary<string, RecipeItem> recipes)
        {
            if (!IsRoot && Parent.IsRoot || IsValidSubRecipe(ItemRecipe))
            {
                string name = Name;
                if (ItemRecipe?.MainProduct.Name != null) name = ItemRecipe.MainProduct.Name;
                if (!recipes.ContainsKey(name))
                {
                    recipes.Add(name, Item);
                }
                else
                {
                    recipes[name].Quantity += Quantity;
                }
            }

            if (!IsRoot && !Parent.IsRoot) return;

            Console.WriteLine($"Item: {Name}");
            if (IsRoot)
            {
                Console.WriteLine($"Quantity: {Quantity}");
                Console.WriteLine();
                Console.WriteLine("Resource Totals: ");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"Quantity: {Quantity}");
                Console.WriteLine();
            }
        }

        private bool IsValidSubRecipe(Recipe itemRecipe)
        {
            if (itemRecipe == null) return true;
            if (RecipeManager.Tags["Ingredient"].Contains(itemRecipe.MainProduct.Name)) return true;
            return false;
        }

        public void ProcessRecipe(ref Dictionary<string, RecipeItem> recipes, int maxDepth = 1, int currentDepth = 0)
        {
            ProcessRecipe(ref recipes);

            foreach (var child in Children)
            {
                if (maxDepth == 0)
                {
                    child.ProcessRecipe(ref recipes, maxDepth);
                }
                if (currentDepth < maxDepth)
                {
                    child.ProcessRecipe(ref recipes, maxDepth, currentDepth + 1);
                }
            }

            if (IsRoot)
            {
                Console.WriteLine($"Full crafting list: ");
                Console.WriteLine($"{"Item",-40} Quantity");
                foreach (var item in recipes)
                {
                    Console.WriteLine($"{item.Key, -40} {item.Value.Quantity}");
                }
            }
        }

        public void AddChild(RecipeItem recipeItem, double quantity)
        {
            if (RecipeManager.RecipesByName.ContainsKey(recipeItem.Name))
            {
                var newRecipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[recipeItem.Name]);
                var newQuantity = RecipeManager.GetUpgradeValue(RecipeManager.TableUpgrades[newRecipe.Table]) *
                    newRecipe.MainProduct.Quantity / Quantity;
                Children.Add(new RecipeTreeNode(newRecipe, newQuantity, this));
            }
            else
            {
                Children.Add(new RecipeTreeNode(recipeItem, recipeItem.Quantity, this));
            }
        }
    }
}
