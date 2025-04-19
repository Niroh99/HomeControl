using HomeControl.Database;
using HomeControl.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Event))]
    public class Event : IdentityKeyModel
    {
        [Column]
        public EventType Type { get => Get<EventType>(); set => Set(value); }

        [Column]
        [JsonField]
        public EventData Data { get => Get<EventData>(); set => Set(value); }

        [Column]
        public DateTime PlannedExecution { get => Get<DateTime>(); set => Set(value); }

        [Column]
        public bool Executed { get => Get<bool>(); set => Set(value); }

        [Column]
        public bool Handled { get => Get<bool>(); set => Set(value); }
    }

    public enum EventType
    {
        ExecuteDeviceFeature
    }
}