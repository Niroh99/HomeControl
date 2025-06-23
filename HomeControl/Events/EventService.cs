using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events.EventDatas;
using System.Reflection;
using System.Timers;

namespace HomeControl.Events
{
    public interface IEventService
    {
        Task<Event> ScheduleEventAsync(IDatabaseConnectionService db, EventType eventType, EventData eventData, DateTime plannedExecution);

        Task ExecuteScheduledEventsAsync();
    }

    public class EventService(IServiceProvider serviceProvider) : IEventService
    {
        static EventService()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var eventDataTypes = assembly.DefinedTypes.Where(x => x.IsAssignableTo(typeof(EventData))).ToList();

            var eventHandlerBaseType = typeof(EventHandler<>);

            var constructedEventHandlerTypes = eventDataTypes.Select(x => eventHandlerBaseType.MakeGenericType(x));

            foreach (var eventHandlerType in assembly.DefinedTypes.Where(x => constructedEventHandlerTypes.Any(y => x.IsAssignableTo(y))))
            {
                var eventDataType = eventHandlerType.BaseType.GenericTypeArguments.First();

                _eventHandlers[eventDataType] = eventHandlerType;
            }
        }

        private static readonly Dictionary<Type, Type> _eventHandlers = [];
        private static readonly Dictionary<EventType, Type> _eventTypeEventDataTypeMap = new()
        {
            { EventType.ExecuteDeviceFeature, typeof(ExecuteDeviceFeatureEventData) },
        };

        public async Task<Event> ScheduleEventAsync(IDatabaseConnectionService db, EventType eventType, EventData eventData, DateTime plannedExecution)
        {
            if (!_eventTypeEventDataTypeMap.TryGetValue(eventType, out var eventDataType)) throw new NotImplementedException();

            if (!eventData.GetType().IsAssignableTo(eventDataType)) throw new ArgumentException("Invalid EventData", nameof(eventData));

            if (plannedExecution < DateTime.Now) throw new ArgumentException("plannedExecution cannot be in the past.", nameof(plannedExecution));

            var newEvent = new Event
            {
                Type = eventType,
                Data = eventData,
                PlannedExecution = plannedExecution,
            };

            await db.Insert(newEvent).ExecuteAsync();

            return newEvent;
        }

        public async Task ExecuteScheduledEventsAsync()
        {
            var db = serviceProvider.GetService<IDatabaseConnectionService>();

            var eventsToExecuteSelect = db.Select<Event>();
            eventsToExecuteSelect.Where().Compare(@event => @event.Handled, ComparisonOperator.Equals, false);

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
            if (!_eventTypeEventDataTypeMap.TryGetValue(eventToExecute.Type, out var eventDataType)) throw new NotImplementedException();

            EventData eventData = eventToExecute.Data;

            if (!_eventHandlers.TryGetValue(eventDataType, out var eventHandlerType)) return;

            var eventHandler = Activator.CreateInstance(eventHandlerType);

            var handleEventMethod = eventHandlerType.GetMethod(nameof(EventHandler<EventData>.HandleEvent));

            handleEventMethod.Invoke(eventHandler, [serviceProvider, eventData]);
        }
    }
}