using System.ComponentModel;
using HomeControl.Models.Extensions;
using HomeControl.Models.Modeling;
using Microsoft.Extensions.DependencyInjection;
using NTIH.Database.Modeling;
using NTIH.Database.Modeling.Attributes;

namespace HomeControl.Models.DatabaseModels
{
    [Table]
    public class RoutineTrigger : IdentityKeyModel, IDisplayable
    {
        [Column]
        public int RoutineId { get => Get<int>(); set => Set(value); }

        [Column]
        public RoutineTriggerType Type { get => Get<RoutineTriggerType>(); set => Set(value); }

        [Column]
        [JsonField]
        public RoutineTriggerData Data { get => Get<RoutineTriggerData>(); set => Set(value); }

        [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(RoutineId))]
        public Routine Routine { get => Get<Routine>(); }
    }

    public class RoutineTriggerDisplay : DisplayBase<RoutineTrigger>
    {
        public override async Task Create(RoutineTrigger routineTrigger, IServiceProvider serviceProvider)
        {
            if (routineTrigger.Data is IDisplayable displayableData)
            {
                var displayFactory = serviceProvider.GetService<IDisplayFactory>();

                var display = await displayFactory.CreateDisplayAsync(displayableData);
                Display = display.Display;
                AdditionalInfo = display.AdditionalInfo;
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

    public abstract class RoutineTriggerData : DatabaseModel, IDisplayable
    {
        public virtual async Task<(string display, string additionalInfo)> CreateDisplay(IServiceProvider serviceProvider)
        {
            await Task.CompletedTask;
            return (ToString(), null);
        }
    }

    public class RoutineTriggerDataDisplay : DisplayBase<RoutineTriggerData>
    {
        public override async Task Create(RoutineTriggerData data, IServiceProvider serviceProvider)
        {
            (Display, AdditionalInfo) = await data.CreateDisplay(serviceProvider);
        }
    }

    public abstract class DailyRoutineTriggerData : RoutineTriggerData
    {
        public HashSet<DayOfWeek> ActiveWeekDays { get; set; }

        public override async Task<(string display, string additionalInfo)> CreateDisplay(IServiceProvider serviceProvider)
        {
            await Task.CompletedTask;
            return (ToString(), string.Join(", ", ActiveWeekDays.Select(dayOfWeek => dayOfWeek.ToShortDayOfWeek())));
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