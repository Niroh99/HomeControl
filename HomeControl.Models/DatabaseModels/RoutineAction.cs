using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class RoutineAction : Action
    {
        [Column]
        public int RoutineId { get => Get<int>(); set => Set(value); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RoutineId))]
        public Routine Routine { get => Get<Routine>(); }
    }
}