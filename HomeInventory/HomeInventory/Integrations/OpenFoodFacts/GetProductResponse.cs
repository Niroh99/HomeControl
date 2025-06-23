using HomeInventory.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventory.Integrations.OpenFoodFacts
{
    public class GetProductResponse
    {
        public int Status { get; set; }

        public string StatusVerbose { get; set; }

        public Product Product { get; set; }
    }
}
