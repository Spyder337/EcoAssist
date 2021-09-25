using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EcoCalc
{
    static class Program
    {
        private static string _saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
        private static readonly string _recipesFile = Path.Combine(_saveDir, "Recipes.json");
        private static readonly string _tagsFile = Path.Combine(_saveDir, "Tags.json");
        private static readonly string _localizationsFile = Path.Combine(_saveDir, "Localizations.json");
        private static readonly string _ecoVersion;

        static Program()
        {

        }

        public static void Main()
        {

        }

    }
}