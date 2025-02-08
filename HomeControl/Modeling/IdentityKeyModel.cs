using HomeControl.Database;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Modeling
{
    public abstract class IdentityKeyModel : Model
    {
        [Key]
        [Column(DatabaseConnection.RowIdColumnName)]
        public int Id { get => Get<int>(); set => Set(value); }
    }
}