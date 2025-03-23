using HomeControl.Events;
using HomeControl.Modeling;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Event))]
    public class Event : IdentityKeyModel
    {
        [Column]
        public EventType Type { get => Get<EventType>(); set => Set(value); }

        [Column]
        public string Data { get => Get<string>(); set => Set(value); }

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