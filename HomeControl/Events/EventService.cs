using HomeControl.Database;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using NTIH.Database;
using System.Reflection;
using System.Timers;

namespace HomeControl.Events
{
    public interface IEventService
    {
        Task<Event> ScheduleEventAsync(IDatabaseConnectionService db, ActionType actionType, ActionData actionData, DateTime plannedExecution);

        Task ExecuteScheduledEventsAsync();
    }

    public class EventService(IServiceProvider serviceProvider, IActionsService actionsService) : IEventService
    {
        public async Task<Event> ScheduleEventAsync(IDatabaseConnectionService db, ActionType actionType, ActionData actionData, DateTime plannedExecution)
        {
            if (plannedExecution < DateTime.Now) throw new ArgumentException("plannedExecution cannot be in the past.", nameof(plannedExecution));

            var newEvent = new Event
            {
                Type = actionType,
                Data = actionData,
                PlannedExecution = plannedExecution,
            };

            await db.Insert(newEvent).ExecuteAsync();

            return newEvent;
        }

        public async Task ExecuteScheduledEventsAsync()
        {
            var db = serviceProvider.GetService<IDatabaseConnectionService>();

            var eventsToExecuteSelect = db.Select<Event>();
            eventsToExecuteSelect.BeginWhere().Compare(@event => @event.Handled, ComparisonOperator.Equals, false);

            foreach (var eventToExecute in await eventsToExecuteSelect.ExecuteAsync())
            {
                if (DateTime.Now < eventToExecute.PlannedExecution) continue;

                try
                {
                    ExecuteEvent(eventToExecute);

                    eventToExecute.Executed = true;
                }
                catch (Exception ex)
                {
                    var eventError = new EventError()
                    {
                        EventId = eventToExecute.Id,
                        Error = ex.ToString()
                    };

                    await db.Insert(eventError).ExecuteAsync();
                }

                eventToExecute.Handled = true;

                await db.Update(eventToExecute).ExecuteAsync();
            }
        }

        private void ExecuteEvent(Event eventToExecute)
        {
            actionsService.ExecuteActionSequenceAsync([eventToExecute], serviceProvider); 
        }
    }
}