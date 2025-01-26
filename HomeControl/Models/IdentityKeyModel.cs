using HomeControl.Sql;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.Models
{
    public class IdentityKeyModel : Model
    {
        [Key]
        [Column(DatabaseConnection.RowIdColumnName)]
        public int Id { get => Get<int>(); set => Set(value); }
    }
}