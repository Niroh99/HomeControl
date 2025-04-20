using HomeControl.Database;
using HomeControl.Modeling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(RoutineTrigger))]
    public class RoutineTrigger : IdentityKeyModel
    {
        [Column]
        public int RoutineId { get => Get<int>(); set => Set(value); }

        [Column]
        public RoutineTriggerType Type { get => Get<RoutineTriggerType>(); set => Set(value); }

        [Column]
        [JsonField]
        public Model Data { get => Get<Model>(); set => Set(value); }
    }

    public enum RoutineTriggerType
    {
        [Description("Interval")]
        Interval,
        [Description("Time of day")]
        TimeOfDay,
        [Description("Sunrise")]
        Sunrise,
        [Description("Sunset")]
        Sunset,
    }

    public class DailyRoutineTriggerData : Model
    {
        public HashSet<DayOfWeek> ActiveWeekDays { get; set; }
    }

    public class TimeOfDayRoutineTriggerData : DailyRoutineTriggerData
    {
        public TimeOnly TimeOfDay { get; set; }
    }

    public class IntervalTriggerData : Model
    {
        public TimeSpan Interval { get; set; }
    }
}