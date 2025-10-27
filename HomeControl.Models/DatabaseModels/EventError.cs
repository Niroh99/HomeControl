using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class EventError : IdentityKeyModel
    {
        [Column]
        public int EventId { get => Get<int>(); set => Set(value); }

        [Column]
        public string Error { get => Get<string>(); set => Set(value); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(EventId))]
        public Event Event { get => Get<Event>(); }
    }
}