using HomeControl.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(EventError))]
    public class EventError : IdentityKeyModel
    {
        [Column]
        public int EventId { get => Get<int>(); set => Set(value); }

        [Column]
        public string Error { get => Get<string>(); set => Set(value); }
    }
}