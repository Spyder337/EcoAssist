using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoRecipeLoader
{
    public class RecipeItem
    {
        public int Id { get; set; } = 0;
        public string Tag { get; set; } = string.Empty;
        public bool IsTag { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public float Quantity { get; set; } = 1;
    }
}
