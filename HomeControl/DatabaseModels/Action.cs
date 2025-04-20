using HomeControl.Database;
using HomeControl.Modeling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    public abstract class Action : IdentityKeyModel, IIndexedObject
    {
        [Column]
        public int Index { get => Get<int>(); set { Set(value); } }

        [Column]
        public ActionType Type { get => Get<ActionType>(); set => Set(value); }

        [Column]
        [JsonField]
        public Model Data { get => Get<Model>(); set => Set(value); }

        public string Display { get => ToString(); }

        public override string ToString()
        {
            return Data?.ToString();
        }
    }

    public enum ActionType
    {
        [Description("Excute Feature")]
        ExecuteFeature,
        [Description("Schedule Feature Execution")]
        ScheduleFeatureExecution
    }

    public class DeviceActionData : Model
    {
        public int DeviceId { get => Get<int>(); set => Set(value); }
    }

    public class ExecuteDeviceFeatureActionData : DeviceActionData
    {
        public string FeatureName { get => Get<string>(); set => Set(value); }

        public override string ToString()
        {
            return FeatureName;
        }
    }

    public class ScheduleDeviceFeatureExecutionActionData : ExecuteDeviceFeatureActionData
    {
        public int ExecuteIn { get => Get<int>(); set => Set(value); }

        public override string ToString()
        {
            return base.ToString() + $" after {ExecuteIn} min";
        }
    }
}