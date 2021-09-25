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
using EcoRecipeLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static EcoRecipeLoader.RecipeLoaderHelper;
namespace EcoRecipeLoader;

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
    private string _itemsFile = Path.Combine(_saveDir, "Items.json");

    private User _testUser = User.CreateUser("debug", "debugSteamId", "debugSlgId", null, true);
    private int _currItemId = 0;
    private HashSet<string> _itemNames = new();
    private Dictionary<string, int> _itemByIdDict = new();
    private Dictionary<CraftingTable, List<EcoRecipeLoader.Recipe>> _recipeDict = new();
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
        var recipe = new EcoRecipeLoader.Recipe();
        bool first = true;
        recipe.Table = GetTableType(GetTableName(ref recipeFamily));

        AssignProducts(ref recipeFamily, ref recipe, ref first);
        AssignIngredients(ref recipeFamily, ref recipe);

        if (!_recipeDict.ContainsKey(recipe.Table))
        {
            _recipeDict.Add(recipe.Table, new List<EcoRecipeLoader.Recipe>() { recipe });
        }
        else
        {
            _recipeDict[recipe.Table].Add(recipe);
        }
    }

    private void AssignIngredients(ref RecipeFamily recipeFamily, ref EcoRecipeLoader.Recipe recipe)
    {
        foreach (var craftingElement in recipeFamily.Ingredients)
        {
            string name = string.Empty;
            bool isTag = false;
            try
            {
                name = craftingElement.Item.Type.GetLocDisplayName();
            }
            catch
            {
                name = craftingElement.Tag.DisplayName.ToString();
                isTag = true;
                if (!_itemByIdDict.ContainsKey(name))
                {
                    _itemByIdDict.Add(name, _currItemId);
                    _currItemId++;
                }
            }

            recipe.Ingredients.Add(new RecipeItem()
            {
                Id = _itemByIdDict[name],
                Name = name,
                IsTag = isTag,
                Quantity = EvaluateDynamicValue(craftingElement.Quantity)
            });
        }
    }

    private void AssignProducts(ref RecipeFamily recipeFamily, ref EcoRecipeLoader.Recipe recipe, ref bool first)
    {
        foreach (var craftingElement in recipeFamily.Product)
        {
            string name = string.Empty;
            try
            {
                name = craftingElement.Item.Type.GetLocDisplayName();
            }
            catch
            {
                Log.WriteErrorLine(Localizer.DoStr($"Error parsing product for : {recipeFamily.DisplayName}"));
            }

            recipe.Products.Add(new RecipeItem()
            {
                Id = _itemByIdDict[name],
                Name = name,
                IsTag = false,
                Quantity = EvaluateDynamicValue(craftingElement.Quantity)
            });
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

    private float EvaluateDynamicValue(IDynamicValue value)
    {
        float diffModifier = DifficultySettings.Obj.Config.DifficultyModifiers.CraftResourceModifier;

        if(value is ConstantValue)
        {
            return value.GetBaseValue;
        }
        else if(value is MultiDynamicValue)
        {
            return DynamicValueExtensions.GetCurrentValue(value, _testUser) * diffModifier;
        }
        else if(value is ModuleModifiedValue mVal)
        {
            return mVal.GetBaseValue * diffModifier;
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
                if (tagName == "Currency" || tagName == "NotInBrowser")
                    continue;
                if (!_itemTagsDict.ContainsKey(tagName))
                {
                    _itemTagsDict.Add(tagName, new HashSet<string>() { name });
                }
                else
                {
                    _itemTagsDict[tagName].Add(name);
                }

                if (!_itemNames.Contains(name))
                {
                    _itemNames.Add(name);
                    _itemByIdDict.Add(name, _currItemId);
                    _currItemId++;
                }
            }
        }

        foreach(var block in BlockItem.AllItems)
        {
            foreach (var tag in block.Tags())
            {
                var tagName = tag.DisplayName.ToString();
                var name = block.DisplayName.ToString();

                if (tagName == "Currency" || tagName == "NotInBrowser")
                    continue;

                if (!_itemTagsDict.ContainsKey(tagName))
                {
                    _itemTagsDict.Add(tagName, new HashSet<string>() { name });
                }
                else
                {
                    _itemTagsDict[tagName].Add(name);
                }

                if (!_itemNames.Contains(name))
                {
                    _itemNames.Add(name);
                    _itemByIdDict.Add(name, _currItemId);
                    _currItemId++;
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

        try
        {
            using var f = File.Create(_itemsFile);
            using var sw = new StreamWriter(f);
            sw.WriteLine(JsonConvert.SerializeObject(_itemByIdDict, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Log.WriteErrorLineLocStr($"Error writing items to the file.");
            Log.WriteErrorLineLocStr($"{ex.Message}");
        }
    }
}
