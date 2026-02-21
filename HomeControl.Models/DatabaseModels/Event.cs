using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class Event : Action
    {
        [Column]
        public DateTime PlannedExecution { get => Get<DateTime>(); set => Set(value); }

        [Column]
        public bool Executed { get => Get<bool>(); set => Set(value); }

        [Column]
        public bool Handled { get => Get<bool>(); set => Set(value); }
    }
}