using HomeControl.Actions;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Weather;
using System.Timers;

namespace HomeControl.Routines
{
    public interface IRoutinesService
    {
        Task ExecuteActiveRoutinesAsync();
    }

    public class RoutinesService(IServiceProvider serviceProvider, IDatabaseConnection db, IWeatherService weatherService, IActionsService actionsService) : IRoutinesService
    {
        public async Task ExecuteActiveRoutinesAsync()
        {
            foreach (var routine in await db.SelectAsync(WhereBuilder.Where<Routine>().Compare(i => i.IsActive, ComparisonOperator.Equals, true)))
            {
                if (await ShouldExecuteRoutine(routine, db, weatherService))
                {
                    try
                    {
                        var actions = await db.SelectAsync(WhereBuilder.Where<RoutineAction>().Compare(i => i.RoutineId, ComparisonOperator.Equals, routine.Id));

                        await actionsService.ExecuteActionSequenceAsync(actions, serviceProvider);
                    }
                    catch
                    {

                    }

                    routine.LastExecution = DateTime.Now;

                    await db.UpdateAsync(routine);
                }
            }
        }

        private async Task<bool> ShouldExecuteRoutine(Routine routine, IDatabaseConnection db, IWeatherService weatherService)
        {
            foreach (var trigger in await db.SelectAsync(WhereBuilder.Where<RoutineTrigger>().Compare(i => i.RoutineId, ComparisonOperator.Equals, routine.Id)))
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
                        var sunriseTriggerData = (DailyRoutineTriggerData)trigger.Data;

                        await weatherService.EnsureValidTodaysForecastAsync();

                        if (ShouldExecuteFromDailyTrigger(routine, sunriseTriggerData, weatherService.Today.Sunrise)) return true;
                        break;
                    case RoutineTriggerType.Sunset:
                        var sunsetTriggerData = (DailyRoutineTriggerData)trigger.Data;

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