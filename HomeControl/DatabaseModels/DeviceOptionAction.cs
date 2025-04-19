using HomeControl.Database;
using HomeControl.Integrations;
using HomeControl.Modeling;
using System.ComponentModel;
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
        [JsonField]
        public object Data { get => Get<object>(); set => Set(value); }

        public string Display { get => ToString(); }

        public override string ToString()
        {
            return Data?.ToString();
        }
    }

    public enum DeviceOptionActionType
    {
        [Description("Excute Feature")]
        ExecuteFeature,
        [Description("Schedule Feature Execution")]
        ScheduleFeatureExecution
    }

    public class ExecuteFeatureDeviceOptionActionData : Model
    {
        public string FeatureName { get => Get<string>(); set => Set(value); }

        public override string ToString()
        {
            return FeatureName;
        }
    }

    public class ScheduleFeatureExecutionDeviceOptionActionData : ExecuteFeatureDeviceOptionActionData
    {
        public int ExecuteIn { get => Get<int>(); set => Set(value); }

        public override string ToString()
        {
            return base.ToString() + $" after {ExecuteIn} min";
        }
    }
}