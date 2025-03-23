using HomeControl.Database;
using HomeControl.DatabaseModels;
using HomeControl.Events.EventDatas;
using System.Reflection;
using System.Timers;

namespace HomeControl.Events
{
    public interface IEventService
    {
        Task<Event> ScheduleEventAsync(IDatabaseConnection db, EventType eventType, EventData eventData, DateTime plannedExecution);
    }

    public class EventService : IEventService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private static readonly Dictionary<Type, Type> _eventHandlers = [];

        private readonly System.Timers.Timer _timer;

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

        public EventService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            _timer = new System.Timers.Timer(TimeSpan.FromSeconds(2));
            _timer.Elapsed += ExecuteScheduledEventAsync;
            _timer.Start();
        }

        public async Task<Event> ScheduleEventAsync(IDatabaseConnection db, EventType eventType, EventData eventData, DateTime plannedExecution)
        {
            switch (eventType)
            {
                case EventType.ExecuteDeviceFeature:
                    if (eventData is not ExecuteDeviceFeatureEventData) throw new ArgumentException("Invalid EventData", nameof(eventData));
                    break;
                default: throw new NotImplementedException();
            }

            if (plannedExecution < DateTime.Now) throw new ArgumentException("plannedExecution cannot be in the past.", nameof(plannedExecution));

            var newEvent = new Event
            {
                Type = eventType,
                Data = System.Text.Json.JsonSerializer.Serialize((object)eventData),
                PlannedExecution = plannedExecution,
            };

            await db.InsertAsync(newEvent);

            return newEvent;
        }

        private async void ExecuteScheduledEventAsync(object sender, ElapsedEventArgs e)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();

            var db = serviceScope.ServiceProvider.GetService<IDatabaseConnection>();

            var allEvents = await db.SelectAllAsync<Event>();

            if (allEvents.Count == 0) return;

            foreach (var eventToExecute in allEvents.Where(e => !e.Handled))
            {
                // TODO: Events nicht mehr löschen, sondern Handled auf true setzen

                if (DateTime.Now < eventToExecute.PlannedExecution) continue;

                try
                {
                    ExecuteEvent(serviceScope.ServiceProvider, eventToExecute);

                    eventToExecute.Executed = true;
                }
                catch (Exception ex)
                {
                    var eventError = new EventError()
                    {
                        EventId = eventToExecute.Id,
                        Error = ex.ToString()
                    };

                    await db.InsertAsync(eventError);
                }

                eventToExecute.Handled = true;

                await db.UpdateAsync(eventToExecute);
            }
        }

        public void ExecuteEvent(IServiceProvider serviceProvider, Event eventToExecute)
        {
            EventData eventData;

            switch (eventToExecute.Type)
            {
                case EventType.ExecuteDeviceFeature:
                    eventData = System.Text.Json.JsonSerializer.Deserialize<ExecuteDeviceFeatureEventData>(eventToExecute.Data);
                    break;
                default: throw new NotImplementedException();
            }

            var eventDataType = eventData.GetType();

            if (!_eventHandlers.TryGetValue(eventDataType, out var eventHandlerType)) return;

            var eventHandler = Activator.CreateInstance(eventHandlerType);

            var handleEventMethod = eventHandlerType.GetMethod(nameof(EventHandler<EventData>.HandleEvent));

            handleEventMethod.Invoke(eventHandler, [serviceProvider, eventData]);
        }
    }
}