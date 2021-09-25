using System;
using System.Collections.Generic;
using System.IO;
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.Components;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using Eco.Shared;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static EcoAssist.RecipeLoaderHelper;
namespace EcoAssist;

public enum CraftingTable
{
    Invalid,
    AdvancedCarpentryTable,
    AdvancedMasonryTable,
    AdvancedTailoringTable,
    Anvil,
    Arrastra,
    AssemblyLine,
    AutomaticLoom,
    BakeryOven,
    BlastFurnace,
    Bloomery,
    ButcheryTable,
    Campfire,
    Campsite,
    Capitol,
    CarpentryTable,
    CastIronStove,
    CementKiln,
    ElectricLathe,
    ElectricMachinistTable,
    ElectricPlaner,
    ElectricStampingPress,
    ElectronicsAssembly,
    FarmersTable,
    Fishery,
    FrothFloatationCell,
    JawCrusher,
    Kiln,
    Kitchen,
    Laboratory,
    Lathe,
    Loom,
    MachinistTable,
    MasonryTable,
    Mill,
    OilRefinery,
    PumpJack,
    ResearchTable,
    RoboticAssemblyLine,
    RockerBox,
    RollingMill,
    Sawmill,
    ScreeningMachine,
    ScrewPress,
    SensorBasedBeltSorter,
    Shaper,
    SpinMelter,
    StampMill,
    Stove,
    TailoringTable,
    ToolBench,
    WainwrightTable,
    Workbench
}

public class RecipeLoader : IModKitPlugin
{
    private static string _saveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
    private string _recipesFile = Path.Combine(_saveDir, "Recipes.json");
    private string _tagsFile = Path.Combine(_saveDir, "Tags.json");
    private string _localizationsFile = Path.Combine(_saveDir, "Localizations.json");
    private string _usedSkill;
    private User _testUser = User.CreateUser("debug", "debugSteamId", "debugSlgId", null, true);
    private Dictionary<string, List<JObject>> _recipeDict = new();
    private Dictionary<string, HashSet<string>> _itemTagsDict = new();
    private JObject _localizationsToken;

    public RecipeLoader()
    {
        InitLocalizations();

        Localizer.TrySetLanguage(SupportedLanguage.English);
        AssignTags();

        foreach (var recipe in RecipeFamily.AllRecipes)
        {
            ProcessRecipeType(recipe);
        }

        WriteToDisk();
    }

    public string GetStatus()
    {
        return "Idle.";
    }

    private void ProcessRecipeType(RecipeFamily recipeFamily)
    {
        JObject result = new JObject();

        _usedSkill = string.Empty;
        bool first = true;
        var tableName = GetTableType(GetTableName(ref recipeFamily)).ToString();
        //Log.WriteLine(Localizer.DoStr($"Parsing recipe: {recipeFamily.DisplayName}"));

        AssignProducts(ref recipeFamily, ref result, ref first);
        AssignIngredients(ref recipeFamily, ref result);
        
        if (_usedSkill != string.Empty)
        {
            result["skill"] = _usedSkill;
        }
        if (!_recipeDict.ContainsKey(tableName))
        {
            _recipeDict.Add(tableName, new List<JObject>() { result });
        }
        else
        {
            _recipeDict[tableName].Add(result);
        }
    }

    private string GetTableName(ref RecipeFamily recipeFamily)
    {
        try
        {
            var toolTip = recipeFamily.TableTooltip();
            var itemStart = toolTip.IndexOf(":") + 1;
            var itemEnd = toolTip.IndexOf('"', itemStart);
            return toolTip[itemStart..itemEnd];
        }
        catch
        {
            return $"Could not read {recipeFamily.DisplayName} table type.";
        }
    }

    private void AssignProducts(ref RecipeFamily recipeFamily, ref JObject result, ref bool first)
    {
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
    }

    private void AssignIngredients(ref RecipeFamily recipeFamily, ref JObject result)
    {
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
    }

    private string EvaluateDynamicValue(IDynamicValue value)
    {
        Log.Write(Localizer.DoStr($"Tested value is {value.GetType().Name}"));

        float diffModifier = DifficultySettings.Obj.Config.DifficultyModifiers.CraftResourceModifier;

        if(value is ConstantValue)
        {
            return Localizer.DoStr($"{value.GetBaseValue}");
        }
        else if(value is MultiDynamicValue)
        {
            return Localizer.DoStr($"{DynamicValueExtensions.GetCurrentValue(value, _testUser) * diffModifier}");
        }
        else if(value is ModuleModifiedValue mVal)
        {
            return Localizer.DoStr($"{mVal.GetBaseValue * diffModifier}");
        }
        throw new Exception($"Can't evaluate value {value}");
    }

    private void InitLocalizations()
    {
        JObject token = new JObject();
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
        _localizationsToken = token;
    }

    private void AssignTags()
    {
        foreach(var item in Item.AllItems)
        {
            foreach(var tag in item.Tags())
            {
                var tagName = tag.DisplayName.ToString();
                var name = item.DisplayName.ToString();
                if (tagName == "Currency")
                    continue;
                if (!_itemTagsDict.ContainsKey(tagName))
                {
                    _itemTagsDict.Add(tagName, new HashSet<string>() { name });
                }
                else
                {
                    _itemTagsDict[tagName].Add(name);
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
                if (!_itemTagsDict.ContainsKey(tagName))
                {
                    _itemTagsDict.Add(tagName, new HashSet<string>() { name });
                }
                else
                {
                    _itemTagsDict[tagName].Add(name);
                }
            }
        }
    }

    private void WriteToDisk()
    {
        
        try
        {
            using var f = File.Create(_recipesFile);
            using var sw = new StreamWriter(f);
            sw.WriteLine(JsonConvert.SerializeObject(_recipeDict, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Log.WriteErrorLineLocStr($"Error writing recipes to the file.");
            Log.WriteErrorLineLocStr($"{ex.Message}");
        }
        try
        {
            using var f = File.Create(_tagsFile);
            using var sw = new StreamWriter(f);
            sw.WriteLine(JsonConvert.SerializeObject(_itemTagsDict, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Log.WriteErrorLineLocStr($"Error writing item tags to the file.");
            Log.WriteErrorLineLocStr($"{ex.Message}");
        }
        try
        {
            using var f = File.Create(_localizationsFile);
            using var sw = new StreamWriter(f);
            sw.WriteLine(JsonConvert.SerializeObject(_localizationsToken, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Log.WriteErrorLineLocStr($"Error writing localizations to the file.");
            Log.WriteErrorLineLocStr($"{ex.Message}");
        }

    }
}
