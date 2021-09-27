using EcoRecipeLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        private void ProcessRecipe()
        {
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
                Console.WriteLine($"Quantity: {Quantity / Parent.ItemRecipe.MainProduct.Quantity}");
                Console.WriteLine();
            }
        }

        public void ProcessRecipe(int maxDepth = 1, int currentDepth = 0)
        {
            ProcessRecipe();
            if (currentDepth < maxDepth)
            {
                foreach (var child in Children)
                {
                    child.ProcessRecipe(maxDepth, currentDepth + 1);
                }
            }
        }

        public void AddChild(RecipeItem recipeItem, double quantity)
        {
            if (RecipeManager.RecipesByName.ContainsKey(recipeItem.Name))
            {
                var newRecipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[recipeItem.Name]);
                Children.Add(new RecipeTreeNode(newRecipe, Quantity * quantity, this));
            }
            else
            {
                Children.Add(new RecipeTreeNode(recipeItem, Quantity * quantity, this));
            }
        }
    }
}
