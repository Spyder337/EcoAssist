using EcoRecipeLoader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoCalc
{
    public class TableModule
    {
        public static List<int> ModuleTiers { get; set; } = new List<int>()
        {
            0,1,2,3,4,5
        };
        public CraftingTable Table { get; set; }
        [Range(0,5)]
        public int Level { get; set; }
    }
}
