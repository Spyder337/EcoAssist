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

        static Program()
        {
            _recipeManager = new RecipeManager();
        }

        public static void Main(params string[] args)
        {
            Dictionary<string, float> res = new();
            bool failed = false;
            if(args.Length == 0)
            {
                res = _recipeManager.CraftRecipe("Ashlar Basalt");
            }
            else if(args.Length == 1)
            {
                if (!_recipeManager.RecipesByName.ContainsKey(args[0]))
                {
                    failed = true;
                    Console.WriteLine($"Error! {args[0]} is not a valid item name.");
                }
                else
                {
                    res = _recipeManager.CraftRecipe(args[0]);
                }
            }
            if (!failed)
            {
                foreach (var item in res)
                {
                    Console.WriteLine($"{item.Key} : {item.Value}");
                }
            }
        }

    }
}