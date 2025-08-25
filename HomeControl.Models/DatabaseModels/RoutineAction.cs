using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Models.DatabaseModels
{
    [Table(nameof(RoutineAction))]
    public class RoutineAction : Action
    {
        [Column]
        public int RoutineId { get => Get<int>(); set => Set(value); }
    }
}