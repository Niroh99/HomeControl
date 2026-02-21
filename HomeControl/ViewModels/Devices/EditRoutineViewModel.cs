using HomeControl.Database;
using HomeControl.Integrations;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.Extensions;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.Routines;
using Microsoft.AspNetCore.Mvc.Rendering;
using NTIH.Database;
using NTIH.ViewModeling;

namespace HomeControl.ViewModels.Devices
{
    public class EditRoutineViewModel(IDatabaseConnectionService db, IDeviceService deviceService) : ViewModel
    {
        public int RoutineId { get => Get<int>(); set => Set(value); }

        public Routine Routine { get => Get<Routine>(); set => Set(value); }

        public List<RoutineTrigger> RoutineTriggers { get => GetList<RoutineTrigger>(); }

        public List<SelectListItem> TriggerTypes { get => GetList<SelectListItem>(); }

        public List<RoutineAction> RoutineActions { get => GetList<RoutineAction>(); }

        public List<SelectListItem> ActionTypes { get => GetList<SelectListItem>(); }

        public List<DeviceInfo> Devices { get => GetList<DeviceInfo>(); }

        public async override Task Initialize()
        {
            Routine = await db.SelectSingle<Routine>(RoutineId).ExecuteAsync();

            if (Routine == null) return;

            var triggersSelect = db.Select<RoutineTrigger>();
            triggersSelect.BeginWhere().Compare(i => i.RoutineId, ComparisonOperator.Equals, Routine.Id);

            RoutineTriggers.AddRange(await triggersSelect.ExecuteAsync());

            foreach (var triggerType in IRoutinesService.RoutineTriggerTypeDataMap.Keys)
            {
                TriggerTypes.Add(new SelectListItem(triggerType.GetValueDescription(), triggerType.ToString()));
            }

            var actionsSelect = db.Select<RoutineAction>();
            actionsSelect.BeginWhere().Compare(i => i.RoutineId, ComparisonOperator.Equals, Routine.Id);

            RoutineActions.AddRange((await actionsSelect.ExecuteAsync()).OrderBy(action => action.Index));

            foreach (var actionType in IRoutinesService.RoutineActionTypeDataMap.Keys)
            {
                ActionTypes.Add(new SelectListItem(actionType.GetValueDescription(), actionType.ToString()));
            }

            var devices = await db.Select<Device>().ExecuteAsync();

            foreach (var device in devices)
            {
                Devices.Add(await DeviceInfo.CreateAsync(device, deviceService, db));
            }
        }
    }
}