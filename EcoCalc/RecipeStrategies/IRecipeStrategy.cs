using EcoRecipeLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoCalc
{
    public interface IRecipeStrategy
    {
        public Dictionary<string, float> CalculateCost(string recipeName, int amount);

        protected static void AddItem(string recipeName, float quantity, ref Dictionary<string, float> cost)
        {
            if (!cost.ContainsKey(recipeName))
            {
                cost.Add(recipeName, quantity);
            }
            else
            {
                cost[recipeName] += quantity;
            }
        }
    }
}
