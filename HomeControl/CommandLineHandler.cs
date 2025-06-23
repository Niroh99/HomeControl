using HomeControl.Database;
using HomeControl.DatabaseModels;

namespace HomeControl
{
    public static class CommandLineHandler
    {
        public static async Task<bool> HandleCommands(string[] args, ConfigurationManager configuration)
        {
            if (Array.BinarySearch(args, "-seed") >= 0)
            {
                await GenerateSeedData(configuration);
                return true;
            }

            return false;
        }

        private static async Task GenerateSeedData(ConfigurationManager configuration)
        {
            var connectionString = configuration.GetConnectionString(Configuration.ConnectionStringName);

            using var db = new DatabaseConnection(connectionString);

            #region ClearDeviceCacheRoutine
            var clearDeviceCacheRoutine = new Routine
            {
                IsActive = true,
                Name = "Clear Device Caches"
            };

            await db.Insert(clearDeviceCacheRoutine).ExecuteAsync();

            var clearDeviceCacheRoutineTrigger = new RoutineTrigger
            {
                RoutineId = clearDeviceCacheRoutine.Id,
                Type = RoutineTriggerType.Interval,
                Data = new IntervalTriggerData { Interval = TimeSpan.FromMinutes(5) }
            };

            await db.Insert(clearDeviceCacheRoutineTrigger).ExecuteAsync();

            var clearDeviceCacheRoutineAction = new RoutineAction
            {
                Index = 1,
                RoutineId = clearDeviceCacheRoutine.Id,
                Type = ActionType.ClearIntegrationDevicesCache,
                Data = new ClearIntegrationDevicesCacheActionData()
            };

            await db.Insert(clearDeviceCacheRoutineAction).ExecuteAsync();
            #endregion

            #region Default Location
            var defaultLocation = new Location
            {
                Name = "Default",
            };

            await db.Insert(defaultLocation).ExecuteAsync();
            #endregion
        }
    }
}