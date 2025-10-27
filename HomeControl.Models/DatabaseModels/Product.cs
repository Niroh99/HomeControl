using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
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