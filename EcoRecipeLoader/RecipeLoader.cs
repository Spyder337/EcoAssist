using System;
using System.Collections.Generic;
using System.IO;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Components;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Shared;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EcoAssist;
public class RecipeLoader : IModKitPlugin
{
    private string _saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
    private string _usedSkill;
    private User _testUser = User.CreateUser("debug", "debugSteamId", "debugSlgId", null, true);

    public RecipeLoader()
    {
        JToken result = new JObject();
        InitLocalizations(ref result);

        Localizer.TrySetLanguage(SupportedLanguage.English);
        JObject recipes = new JObject();
        result["Tags"] = AssignTags();
        result["Recipes"] = recipes;

        foreach (var recipe in RecipeFamily.AllRecipes)
        {
            recipes[recipe.GetType().Name] = ProcessRecipeType(recipe);
        }

        WriteToDisk(result);
    }

    public string GetStatus()
    {
        return "Idle.";
    }

    private JToken ProcessRecipeType(RecipeFamily recipeFamily)
    {
        JObject result = new JObject();

        _usedSkill = string.Empty;
        bool first = true;
        //Log.WriteLine(Localizer.DoStr($"Parsing recipe: {recipeFamily.DisplayName}"));
        
        foreach (var craftingElement in recipeFamily.Product)
        {
            string name = string.Empty;
            try
            {
                name = craftingElement.Item.Type.GetLocDisplayName();
                //Log.WriteLine(Localizer.DoStr($"Parsing Product : {name}"));
            }
            catch
            {
                Log.WriteErrorLine(Localizer.DoStr($"Error parsing product for : {recipeFamily.DisplayName}"));
            }

            if (first)
            {
                first = false;
                result["result"] = name;
                result["quantity"] = EvaluateDynamicValue(craftingElement.Quantity);
                result["ingredients"] = new JObject();
                result["products"] = new JObject();
            }

            result["products"][name] = EvaluateDynamicValue(craftingElement.Quantity);
        }
        

        foreach (var craftingElement in recipeFamily.Ingredients)
        {
            string name = string.Empty;
            try
            {
                name = craftingElement.Item.Type.GetLocDisplayName();
                //Log.WriteLine(Localizer.DoStr($"Parsing Ingredient : {name}"));
            }
            catch
            {
                name = craftingElement.Tag.DisplayName.ToString();
                //Log.WriteLine(Localizer.DoStr($"Parsing Ingredient : {craftingElement.Tag.Name}"));
            }
            result["ingredients"][name] = EvaluateDynamicValue(craftingElement.Quantity);
        }
        
        if (_usedSkill != string.Empty)
        {
            result["skill"] = _usedSkill;
        }
        return result;
    }

    private string EvaluateDynamicValue(IDynamicValue value)
    {
        Log.Write(Localizer.DoStr($"Tested value is {value.GetType().Name}"));
        
        if(value is ConstantValue)
        {
            return Localizer.DoStr($"{value.GetBaseValue}");
        }
        else if(value is MultiDynamicValue)
        {
            return Localizer.DoStr($"{DynamicValueExtensions.GetCurrentValue(value, _testUser) * 2}");
        }
        else if(value is ModuleModifiedValue mVal)
        {
            return Localizer.DoStr($"{mVal.GetBaseValue * 2}");
        }
        throw new Exception($"Can't evaluate value {value}");
    }

    private void InitLocalizations(ref JToken token)
    {
        token["Version"] = EcoVersion.Version;
        token["Localization"] = new JObject();
        foreach (SupportedLanguage language in Enum.GetValues(typeof(SupportedLanguage)))
        {
            if (!Localizer.IsNormalizedLanguage(language))
                continue;

            Localizer.TrySetLanguage(language);
            JObject localization = new JObject();
            token["Localization"][language.GetLocDisplayName().ToString()] = localization;

            foreach (Item item in Item.AllItems)
            {
                localization[item.Type.Name] = (string)item.DisplayName;
            }

            foreach (var recipe in RecipeFamily.AllRecipes)
            {
                localization[recipe.GetType().Name] = (string)recipe.DisplayName;
            }
        }
    }

    private JObject AssignTags()
    {
        var tags = new Dictionary<string, HashSet<string>>();

        foreach(var item in Item.AllItems)
        {
            foreach(var tag in item.Tags())
            {
                var tagName = tag.DisplayName.ToString();
                var name = item.DisplayName.ToString();
                if (tagName == "Currency")
                    continue;
                if (!tags.ContainsKey(tagName))
                {
                    tags.Add(tagName, new HashSet<string>() { name });
                }
                else
                {
                    tags[tagName].Add(name);
                }
            }
        }

        foreach(var block in BlockItem.AllItems)
        {
            foreach (var tag in block.Tags())
            {
                var tagName = tag.DisplayName.ToString();
                var name = block.DisplayName.ToString();
                if (tagName == "Currency")
                    continue;
                if (!tags.ContainsKey(tagName))
                {
                    tags.Add(tagName, new HashSet<string>() { name });
                }
                else
                {
                    tags[tagName].Add(name);
                }
            }
        }

        var jsonString = JsonConvert.SerializeObject(tags);
        return JsonConvert.DeserializeObject<JObject>(jsonString);
    }

    private void WriteToDisk(JToken token)
    {
        try
        {
            using var f = File.Create(Path.Combine(_saveDir, "Recipes.json"));
            using var sw = new StreamWriter(f);
            sw.WriteLine(token);
        }
        catch (Exception ex)
        {
            Log.WriteErrorLineLocStr($"Error writing to the file.");
            Log.WriteErrorLineLocStr($"{ex.Message}");
        }
    }
}
