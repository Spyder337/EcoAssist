using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoCalc
{
    public class Recipe
    {
        [JsonProperty("result")]
        public string MainProduct { get; set; }
        [JsonProperty("quantity")]
        public float Quantity { get; set; }
        [JsonProperty("ingredients")]
        public Dictionary<string, float> Ingredients { get; set; }
        [JsonProperty("products")]
        public Dictionary<string, float> Products { get; set; }
    }
}
