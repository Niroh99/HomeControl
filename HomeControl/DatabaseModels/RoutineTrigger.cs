using HomeControl.Database;
using HomeControl.Helpers;
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

        public override Task<string> ToString(IServiceProvider serviceProvider)
        {
            return Data.ToString(serviceProvider);
        }

        public override async Task<string> GetAdditionalInfo(IServiceProvider serviceProvider)
        {
            return await Data.GetAdditionalInfo(serviceProvider);
        }
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

    public abstract class DailyRoutineTriggerData : Model
    {
        public HashSet<DayOfWeek> ActiveWeekDays { get; set; }

        public override async Task<string> GetAdditionalInfo(IServiceProvider serviceProvider)
        {
            await Task.CompletedTask;
            return string.Join(", ", ActiveWeekDays.Select(dayOfWeek => dayOfWeek.ToShortDayOfWeek()));
        }
    }

    public class TimeOfDayRoutineTriggerData : DailyRoutineTriggerData
    {
        public TimeOnly TimeOfDay { get; set; }

        public override string ToString()
        {
            return TimeOfDay.ToShortTimeString();
        }
    }

    public class SunriseRoutineTriggerData : DailyRoutineTriggerData
    {
        public override string ToString()
        {
            return "Sunrise";
        }
    }

    public class SunsetRoutineTriggerData : DailyRoutineTriggerData
    {
        public override string ToString()
        {
            return "Sunset";
        }
    }

    public class IntervalTriggerData : Model
    {
        public TimeSpan Interval { get; set; }

        public override string ToString()
        {
            return $"Interval: {Interval}";
        }
    }
}