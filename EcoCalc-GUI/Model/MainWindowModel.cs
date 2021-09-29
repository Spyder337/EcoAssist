using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EcoCalc;
using EcoCalc_GUI.Annotations;
using EcoRecipeLoader;

namespace EcoCalc_GUI.Model
{
    public class MainWindowModel : INotifyPropertyChanged, INotifyCollectionChanged
    {
        public static string SaveDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EcoRecipes");
        public static string RecipesFile { get; } = Path.Combine(SaveDir, "Recipes.json");
        public static string TagsFile { get; } = Path.Combine(SaveDir, "Tags.json");
        public static string LocalizationsFile { get; } = Path.Combine(SaveDir, "Localizations.json");

        private Dictionary<string, RecipeItem> _craftingResults = new();
        private ObservableCollection<RecipeItem> _craftingItems = new();
        private ObservableCollection<RecipeItem> _simpleCraftingItems = new();
        private string _selectedCraftingTable;
        private IEnumerable<Recipe> _recipes;
        private string _amountText = "1.0";
        private ObservableCollection<TableModule> _modules;

        public ObservableCollection<RecipeItem> CraftingResults
        {
            get
            {
                return _craftingItems;
            }
            set
            {
                _craftingItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<RecipeItem> SimpleCraftingResults
        {
            get
            {
                return _simpleCraftingItems;
            }
            set
            {
                _simpleCraftingItems = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TableModule> Modules
        {
            get => _modules;
            set
            {
                if (Equals(value, _modules)) return;
                _modules = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCraftingTable
        {
            get
            {
                return _selectedCraftingTable;
            }
            set
            {
                _selectedCraftingTable = value;
                OnPropertyChanged();
                Recipes = GetCurrentRecipes();
            }
        }

        public string AmountText
        {
            get => _amountText;
            set
            {
                if (value == _amountText) return;
                _amountText = value;
                OnPropertyChanged();
            }
        }

        public double Amount
        {
            get
            {
                if (double.TryParse(_amountText, out double res))
                {
                    return res;
                }
                else
                {
                    return 1.0;
                }
            }
        }

        public IEnumerable<string> CraftingTables { get; } = GetCraftingTables();

        public Recipe CurrentRecipe { get; set; }
        public IEnumerable<Recipe> Recipes
        {
            get
            {
                return _recipes;
            }
            set
            {
                _recipes = value;
                OnPropertyChanged();
            }
        }

        public MainWindowModel()
        {
            var test = RecipeManager.RecipeLevel;
            Modules = new(RecipeManager.TableUpgrades);
        }

        public void Craft()
        {
            _craftingResults = new();
            var root = new RecipeTreeNode(CurrentRecipe.MainProduct, Amount);
            root.ProcessRecipe(ref _craftingResults, 0);
            
            CraftingResults = new(_craftingResults.Values);
            SimpleCraftingResults = new();

            foreach (var kvp in _craftingResults)
            {
                foreach (var child in root.Children)
                {
                    if (kvp.Key == child.Name)
                    {
                        SimpleCraftingResults.Add(child.Item);
                    }   
                }
            }
        }

        private IEnumerable<Recipe> GetCurrentRecipes()
        {
            if (SelectedCraftingTable != string.Empty)
            {
                var table = Enum.Parse<CraftingTable>(SelectedCraftingTable);
                return RecipeManager.RecipesByTable[table];
            }
            else
            {
                return RecipeManager.RecipesByName.Values;
            }
        }

        private static IEnumerable<string> GetCraftingTables()
        {
            return Enum.GetNames<CraftingTable>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
    }
}
