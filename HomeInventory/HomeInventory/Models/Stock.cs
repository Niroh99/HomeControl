using System;
using System.Collections.Generic;
using System.Text;

namespace HomeInventory.Models
{
    public class Stock
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int LocationId { get; set; }

        public decimal Quantity { get; set; }

        public Product Product { get; set; }

        public Location Location { get; set; }
    }
}