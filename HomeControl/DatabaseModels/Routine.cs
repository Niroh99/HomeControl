using HomeControl.Database;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(Routine))]
    public class Routine : IdentityKeyModel
    {
        [Column]
        public bool IsActive { get => Get<bool>(); set => Set(value); }

        [Column]
        public DateTime? LastExecution { get => Get<DateTime?>(); set => Set(value); }
    }
}