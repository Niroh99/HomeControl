using HomeControl.Database;
using HomeControl.Helpers;
using HomeControl.Modeling;
using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;
using NTIH.Modeling;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeControl.DatabaseModels
{
    [Table(nameof(RoutineTrigger))]
    public class RoutineTrigger : IdentityKeyModel, IDisplayable
    {
        [Column]
        public int RoutineId { get => Get<int>(); set => Set(value); }

        [Column]
        public RoutineTriggerType Type { get => Get<RoutineTriggerType>(); set => Set(value); }

        [Column]
        [JsonField]
        public Model Data { get => Get<Model>(); set => Set(value); }

        private string _display;
        public string Display => _display;

        private string _additionalInfo;
        public string AdditionalInfo => _additionalInfo;

        public async Task CreateDisplay(IServiceProvider serviceProvider)
        {
            if (Data is IDisplayable displayableData)
            {
                await displayableData.CreateDisplay(serviceProvider);
                _display = displayableData.Display;
                _additionalInfo = displayableData.AdditionalInfo;
            }
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

    public abstract class RoutineTriggerData : Model, IDisplayable
    {
        private string _display;
        public string Display => _display;
        private string _additionalInfo;
        public string AdditionalInfo => _additionalInfo;
        public virtual async Task CreateDisplay(IServiceProvider serviceProvider)
        {
            _display = ToString();
            _additionalInfo = GetAdditionalInfo(serviceProvider);
            await Task.CompletedTask;
        }

        protected virtual string GetAdditionalInfo(IServiceProvider serviceProvider)
        {
            return null;
        }
    }

    public abstract class DailyRoutineTriggerData : RoutineTriggerData
    {
        public HashSet<DayOfWeek> ActiveWeekDays { get; set; }

        protected override string GetAdditionalInfo(IServiceProvider serviceProvider)
        {
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

    public class IntervalTriggerData : RoutineTriggerData
    {
        public TimeSpan Interval { get; set; }

        public override string ToString()
        {
            return $"Interval: {Interval}";
        }
    }
}