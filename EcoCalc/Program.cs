// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string _saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
string _filePath = Path.Combine(_saveDir, "Recipes.json");

