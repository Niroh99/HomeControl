using HomeControl.Actions;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Weather;
using System.Collections.ObjectModel;
using System.Timers;

namespace HomeControl.Routines
{
    public interface IRoutinesService
    {
        public static ReadOnlyDictionary<RoutineTriggerType, Type> RoutineTriggerTypeDataMap { get; } = new Dictionary<RoutineTriggerType, Type>
        {
            { RoutineTriggerType.Interval, typeof(IntervalTriggerData) },
            { RoutineTriggerType.TimeOfDay, typeof(TimeOfDayRoutineTriggerData) },
            { RoutineTriggerType.Sunrise, typeof(SunriseRoutineTriggerData) },
            { RoutineTriggerType.Sunset, typeof(SunsetRoutineTriggerData) },
        }.AsReadOnly();

        public static ReadOnlyDictionary<ActionType, Type> RoutineActionTypeDataMap { get; } = IActionsService.ActionTypeDataMap;

        Task ExecuteActiveRoutinesAsync();
    }

    public class RoutinesService(IServiceProvider serviceProvider, IDatabaseConnectionService db, IWeatherService weatherService, IActionsService actionsService) : IRoutinesService
    {
        public async Task ExecuteActiveRoutinesAsync()
        {
            var routinesSelect = db.Select<Routine>();
            routinesSelect.Where().Compare(i => i.IsActive, ComparisonOperator.Equals, true);

            foreach (var routine in await routinesSelect.ExecuteAsync())
            {
                if (await ShouldExecuteRoutine(routine))
                {
                    try
                    {
                        var actionsSelect = db.Select<RoutineAction>();
                        actionsSelect.Where().Compare(i => i.RoutineId, ComparisonOperator.Equals, routine.Id);

                        var actions = await actionsSelect.ExecuteAsync();

                        await actionsService.ExecuteActionSequenceAsync(actions, serviceProvider);
                    }
                    catch
                    {

                    }

                    routine.LastExecution = DateTime.Now;

                    await db.Update(routine).ExecuteAsync();
                }
            }
        }

        private async Task<bool> ShouldExecuteRoutine(Routine routine)
        {
            var triggersSelect = db.Select<RoutineTrigger>();
            triggersSelect.Where().Compare(i => i.RoutineId, ComparisonOperator.Equals, routine.Id);

            foreach (var trigger in await triggersSelect.ExecuteAsync())
            {
                switch (trigger.Type)
                {
                    case RoutineTriggerType.Interval:
                        if (routine.LastExecution == null) return true;

                        var intervalTriggerData = (IntervalTriggerData)trigger.Data;

                        if (DateTime.Now >= routine.LastExecution.Value.Add(intervalTriggerData.Interval)) return true;
                        break;
                    case RoutineTriggerType.TimeOfDay:
                        var timeOfDayTriggerData = (TimeOfDayRoutineTriggerData)trigger.Data;

                        if (ShouldExecuteFromDailyTrigger(routine, timeOfDayTriggerData, timeOfDayTriggerData.TimeOfDay)) return true;
                        break;
                    case RoutineTriggerType.Sunrise:
                        var sunriseTriggerData = (SunriseRoutineTriggerData)trigger.Data;

                        await weatherService.EnsureValidTodaysForecastAsync();

                        if (ShouldExecuteFromDailyTrigger(routine, sunriseTriggerData, weatherService.Today.Sunrise)) return true;
                        break;
                    case RoutineTriggerType.Sunset:
                        var sunsetTriggerData = (SunsetRoutineTriggerData)trigger.Data;

                        await weatherService.EnsureValidTodaysForecastAsync();

                        if (ShouldExecuteFromDailyTrigger(routine, sunsetTriggerData, weatherService.Today.Sunset)) return true;
                        break;
                }
            }

            return false;
        }

        private bool ShouldExecuteFromDailyTrigger(Routine routine, DailyRoutineTriggerData dailyTriggerData, TimeOnly timeOfDay)
        {
            if (!dailyTriggerData.ActiveWeekDays.Contains(DateTime.Today.DayOfWeek)) return false;

            if (!PassedTimeOfDay(routine.LastExecution ?? DateTime.Today, timeOfDay)
                && PassedTimeOfDay(DateTime.Now, timeOfDay)) return true;

            return false;
        }

        private bool PassedTimeOfDay(DateTime referenceDate, TimeOnly timeOfDay)
        {
            if (referenceDate > DateTime.Today.Add(timeOfDay.ToTimeSpan())) return true;

            return false;
        }
    }
}