using HomeControl.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Stock))]
    public class Stock : IdentityKeyModel
    {
        [Column]
        public int ProductId { get => Get<int>(); set => Set(value); }

        [Column]
        public int LocationId { get => Get<int>(); set => Set(value); }

        [Column]
        public decimal Quantity { get => Get<decimal>(); set => Set(value); }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get => Get<Product>(); }

        [ForeignKey(nameof(LocationId))]
        public Location Location { get => Get<Location>(); }
    }
}