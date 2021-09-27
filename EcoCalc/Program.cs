using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EcoCalc.RecipeStrategies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EcoCalc
{
    static class Program
    {
        public static string SaveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
        public static readonly string RecipesFile = Path.Combine(SaveDir, "Recipes.json");
        public static readonly string TagsFile = Path.Combine(SaveDir, "Tags.json");
        public static readonly string LocalizationsFile = Path.Combine(SaveDir, "Localizations.json");
        public static readonly string EcoVersion;

        private static RecipeManager _recipeManager;

        public static void Main(params string[] args)
        {
            _recipeManager = new RecipeManager();
            string defaultRecipe = "Laser";
            //Dictionary<string, float> res = new();
            RecipeTreeNode root = null;
            bool failed = false;
            if(args.Length == 0)
            {
                /*
                RecipeManager.RecipeLevel = 1;
                res = RecipeManager.CraftRecipe(defaultRecipe, 1, new BasicResourcesStrategy());
                */
                RecipeManager.RecipeLevel = 2;
                var recipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[defaultRecipe]);
                root = new RecipeTreeNode(recipe);
                //root.ProcessRecipe();
                //root.ProcessVerbiseRecipe();
            }
            else if(args.Length == 1)
            {
                if (!RecipeManager.RecipesByName.ContainsKey(args[0]))
                {
                    failed = true;
                    Console.WriteLine($"Error! {args[0]} is not a valid item name.");
                }
                else
                {
                    var recipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[args[0]]);
                    root = new RecipeTreeNode(recipe);
                    //res = RecipeManager.CraftRecipe(args[0], 1, new BasicResourcesStrategy());
                }
            }
            else if(args.Length == 2)
            {
                if (!RecipeManager.RecipesByName.ContainsKey(args[0]))
                {
                    failed = true;
                    Console.WriteLine($"Error! {args[0]} is not a valid item name.");
                }
                else
                {
                    if (!int.TryParse(args[1], out int amount))
                    {
                        Console.WriteLine($"Error! Second argument must be an integer.");
                    }
                    //res = _recipeManager.CraftRecipe(args[0], amount, new BasicResourcesStrategy());
                    var recipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[args[0]]);
                    root = new RecipeTreeNode(recipe, amount);
                }
            }
            else if(args.Length == 4)
            {
                if (!RecipeManager.RecipesByName.ContainsKey(args[0]))
                {
                    failed = true;
                    Console.WriteLine($"Error! {args[0]} is not a valid item name.");
                    return;
                }
                if (!int.TryParse(args[1], out int amount))
                {
                    failed = true;
                    Console.WriteLine($"Error! Second argument must be an integer.");
                    return;
                }
                if(args[2] == "--recipe-level" ||  args[2] == "-rl")
                {
                    if(!int.TryParse(args[3], out RecipeManager.RecipeLevel)){
                        Console.WriteLine($"Error! Recipe level must be a valid integer between 1 and 3");
                    }
                }
                //res = _recipeManager.CraftRecipe(args[0], amount, new BasicResourcesStrategy());
                var recipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[args[0]]);
                root = new RecipeTreeNode(recipe, amount);
            }
            root.ProcessVerbiseRecipe();
            /*
            if (!failed)
            {
                string itemName = args.Length == 0 ? defaultRecipe : args[0];
                Console.WriteLine($"Loaded recipe: {itemName}\n");
                Console.WriteLine($"Resources:");
                foreach (var item in res)
                {
                    Console.WriteLine($"\t{item.Key} : {item.Value}");
                }
            }
            */
        }
    }
}