using HomeControl.Database;
using HomeControl.Modeling;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Product))]
    public class Product : IdentityKeyModel
    {
        [Column]
        [Unique]
        public string GlobalTradeItemNumber { get => Get<string>(); set => Set(value); }

        [Column]
        public string Name { get => Get<string>(); set => Set(value); }

        [Column]
        public string Brands { get => Get<string>(); set => Set(value); }

        [Column]
        public string FrontImageLarge { get => Get<string>(); set => Set(value); }
        
        [Column]
        public string FrontImageSmall { get => Get<string>(); set => Set(value); }

        [Column]
        public string NutritionImage { get => Get<string>(); set => Set(value); }
    }
}