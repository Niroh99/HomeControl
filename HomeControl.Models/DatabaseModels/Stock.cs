using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class Stock : IdentityKeyModel
    {
        [Column]
        public int ProductId { get => Get<int>(); set => Set(value); }

        [Column]
        public int LocationId { get => Get<int>(); set => Set(value); }

        [Column(20, 3)]
        public decimal Quantity { get => Get<decimal>(); set => Set(value); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ProductId))]
        public Product Product { get => Get<Product>(); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(LocationId))]
        public Location Location { get => Get<Location>(); }
    }
}