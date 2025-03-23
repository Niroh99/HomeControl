using HomeControl.Modeling;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(DeviceOptionAction))]
    public class DeviceOptionAction : IdentityKeyModel
    {
        [Column]
        public int DeviceOptionId { get => Get<int>(); set => Set(value); }

        [Column]
        public int Index { get => Get<int>(); set { Set(value); } }

        [Column]
        public DeviceOptionActionType Type { get => Get<DeviceOptionActionType>(); set => Set(value); }

        [Column]
        public string Data { get => Get<string>(); set => Set(value); }
    }

    public enum DeviceOptionActionType
    {
        ExecuteFeature,
        ScheduleFeatureExecution
    }

    public class ExecuteFeatureDeviceOptionActionData
    {
        public string FeatureName { get; set; }
    }

    public class ScheduleFeatureExecutionDeviceOptionActionData : ExecuteFeatureDeviceOptionActionData
    {
        public TimeSpan ExecuteIn { get; set; } 
    }
}