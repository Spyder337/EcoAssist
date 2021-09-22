
using EcoCalc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EcoCalc
{
    static class Program
    {
        private static readonly string _saveDir;
        private static readonly string _fileDir;
        private static string _ecoVersion;
        private static SortedDictionary<string, string[]> _tags;
        private static SortedDictionary<string, Recipe> _recipes;

        static Program()
        {
            _saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
            _fileDir = Path.Combine(_saveDir, "Recipes.json");
        }

        public static void Main()
        {
            InitProgram();
        }

        private static void InitProgram()
        {

            var token = ReadRecipeFile();
            _ecoVersion = token["Version"].Value<string>();
            ParseTokens(token["Tags"], token["Recipes"]);

            Console.WriteLine($"Eco Version: {_ecoVersion}");
            Console.WriteLine($"Tags: {_tags.Count}");
            Console.WriteLine($"Recipes: {_recipes.Count}");
            Console.WriteLine();
        }

        private static JObject ReadRecipeFile()
        {
            try
            {
                using var f = File.OpenRead(_fileDir);
                using var sr = new StreamReader(f);
                var jsonText = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<JObject>(jsonText);
            }
            catch(Exception ex)
            {
                return new JObject();
            }
            return new JObject();
        }

        private static void ParseTokens(JToken tagsToken, JToken recToken)
        {
            _tags = JsonConvert.DeserializeObject<SortedDictionary<string, string[]>>(tagsToken.ToString());
            var recipes = JsonConvert.DeserializeObject<Dictionary<string, Recipe>>(recToken.ToString());
            _recipes = new SortedDictionary<string, Recipe>();
            foreach(var pair in recipes)
            {
                _recipes.Add(pair.Key, pair.Value);
            }
        }
    }
}