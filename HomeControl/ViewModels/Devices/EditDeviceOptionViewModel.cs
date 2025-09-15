using HomeControl.Database;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.Extensions;
using HomeControl.Models.Integrations;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using NTIH.Database;

namespace HomeControl.ViewModels.Devices
{
    public class EditDeviceOptionViewModel(IDatabaseConnectionService db, IDeviceService deviceService) : PageViewModel
    {
        public int DeviceOptionId { get => Get<int>(); set => Set(value); }

        public Device Device { get => Get<Device>(); set => Set(value); }

        public IIntegrationDevice IntegrationDevice { get => Get<IIntegrationDevice>(); set => Set(value); }

        public DeviceOption DeviceOption { get => Get<DeviceOption>(); set => Set(value); }

        public List<DeviceOptionAction> DeviceOptionActions { get => GetList<DeviceOptionAction>(); }

        public List<SelectListItem> DeviceOptionActionTypes { get => GetList<SelectListItem>(); }

        public async override Task Initialize()
        {
            DeviceOption = await db.SelectSingle<DeviceOption>(DeviceOptionId).ExecuteAsync();

            if (DeviceOption == null) return;

            Device = await db.SelectSingle<Device>(DeviceOption.DeviceId).ExecuteAsync();
            IntegrationDevice = await deviceService.CreateAndInitializeIntegrationDeviceAsync(Device);

            var deviceOptions = await db.Select<DeviceOptionAction>()
                .BeginWhere().Compare(i => i.DeviceOptionId, ComparisonOperator.Equals, DeviceOption.Id)
                .EndWhere()
                .ExecuteAsync();

            DeviceOptionActions.AddRange(deviceOptions.OrderBy(action => action.Index));

            DeviceOptionActionTypes.AddRange(IDeviceService.DeviceOptionActionTypeDataMap
                    .Select(type => new SelectListItem(type.Key.GetValueDescription(), type.Key.ToString())));
        }
    }
}