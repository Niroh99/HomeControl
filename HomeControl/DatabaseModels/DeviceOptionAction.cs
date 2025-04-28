using HomeControl.Database;
using HomeControl.Integrations;
using HomeControl.Modeling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(DeviceOptionAction))]
    public class DeviceOptionAction : Action
    {
        [Column]
        public int DeviceOptionId { get => Get<int>(); set => Set(value); }

        public override async Task<string> ToString(IServiceProvider serviceProvider)
        {
            await Task.CompletedTask;
            return Data.ToString();
        }
    }
}