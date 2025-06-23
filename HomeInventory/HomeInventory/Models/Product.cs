using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventory.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string GlobalTradeItemNumber { get; set; }

        public string Name { get; set; }

        public string Brands { get; set; }

        public string FrontImageLarge { get; set; }

        public string FrontImageSmall { get; set; }

        public string NutritionImage { get; set; }
    }
}