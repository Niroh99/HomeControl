using HomeControl.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(RoutineAction))]
    public class RoutineAction : Action
    {
        [Column]
        public int RoutineId { get => Get<int>(); set => Set(value); }
    }
}