using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EcoCalc.RecipeStrategies;
using EcoRecipeLoader;
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
        public static readonly Dictionary<string, string> RawToAlias = new()
        {
            {"--amount", "-a"},
            {"--recipe-level", "-rl"},
            {"--max-depth", "-md"},
            {"--verbose", "-v"}
        };
        public static readonly Dictionary<string, string> AliasToRaw = new()
        {
            {"-a", "--amount"},
            {"-rl", "--recipe-level"},
            {"-md", "--max-depth"},
            {"-v", "--verbose"}
        };
        public static readonly Dictionary<string, int> NumofArgs = new()
        {
            {"-a", 1},
            {"-rl", 1},
            {"-md", 1},
            {"-v", 1}
        };
        public static readonly Dictionary<string, int> ArgValues = new()
        {
            {"-a", 1},
            {"-rl", 1},
            {"-md", 1},
            {"-v", 0}
        };

        public static void Main(params string[] args)
        {
            string defaultRecipe = "Laser";
            RecipeTreeNode root;
            string recipeName = args.Length > 0 ? args[0] : defaultRecipe;

            if (!RecipeManager.HasRecipe(recipeName))
            {
                Console.WriteLine("Error! Invalid recipe or command usage.");
                Console.WriteLine("Use -h or --help for command help.");
                return;
            }

            if (args.Length > 1)
            {
                ParseArguments(args[1..]);
            }

            RecipeManager.RecipeLevel = ArgValues["-rl"];
            var recipe = RecipeManager.GetActiveRecipe(RecipeManager.RecipesByName[recipeName]);
            root = new RecipeTreeNode(recipe.MainProduct, ArgValues["-a"]);
            var condensedRecipes = new Dictionary<string, RecipeItem>();
            root.ProcessRecipe(ref condensedRecipes, ArgValues["-md"], verbose: ArgValues["-v"] == 1);
        }

        private static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var numArgs = ParseArgument(args[i]);
                i += numArgs;
                if (int.TryParse(args[i], out int result))
                {
                    ArgValues[GetAsAlias(args[i - numArgs])] = result;
                }
            }
        }

        private static string GetAsAlias(string name)
        {
            if (AliasToRaw.ContainsKey(name))
            {
                return name;
            }
            if (RawToAlias.ContainsKey(name))
            {
                return RawToAlias[name];
            }
            Console.WriteLine("Error! Invalid command argument. Use -h or --help for command usage.");
            return string.Empty;
        }

        private static int ParseArgument(string name)
        {
            int numArgs = -1;
            if (name == string.Empty)
                return numArgs;
            numArgs = NumofArgs[GetAsAlias(name)];
            return numArgs;
        }
    }
}