using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HomeControl.Modeling;
using HomeControl.Integrations;
using System.Threading.Tasks;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Manage Integrations", "/Devices/ManageIntegrations")]
    public class ManageIntegrationsModel(IDatabaseConnectionService db, IDeviceService deviceService) : ViewModelPageModel<ManageIntegrationsModel.ManageIntegrationsViewModel>
    {
        public class ManageIntegrationsViewModel(ViewModelPageModelBase page) : PageViewModel(page)
        {

        }

        protected override PageViewModel CreateViewModel()
        {
            return new ManageIntegrationsViewModel(this);
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostDiscoverDevices()
        {
            var databaseDevices = await db.Select<Device>().ExecuteAsync();

            var tpLinkDevices = HomeControl.Integrations.TPLink.Discovery.Discover();

            var rediscoveredDeviceIds = new List<int>();

            foreach (var tpLinkDevice in tpLinkDevices)
            {
                var databaseDeviceSelect = db.Select<Device>();
                databaseDeviceSelect.Where().Compare(i => i.Hostname, ComparisonOperator.Equals, tpLinkDevice.Hostname);

                var databaseDevice = (await databaseDeviceSelect.ExecuteAsync()).FirstOrDefault();

                if (databaseDevice == null)
                {
                    await db.Insert(new Device
                    {
                        Type = tpLinkDevice.DeviceType,
                        Hostname = tpLinkDevice.Hostname,
                        Port = tpLinkDevice.Port,
                    }).ExecuteAsync();
                }
                else rediscoveredDeviceIds.Add(databaseDevice.Id);
            }

            var deviceOptions = await db.Select<DeviceOption>().ExecuteAsync();

            var deviceOptionActions = await db.Select<DeviceOptionAction>().ExecuteAsync();

            foreach (var databaseDeviceToDelete in databaseDevices.Where(databaseDevice => !rediscoveredDeviceIds.Contains(databaseDevice.Id)))
            {
                foreach (var deviceOption in deviceOptions.Where(option => option.DeviceId == databaseDeviceToDelete.Id))
                {
                    foreach (var action in deviceOptionActions.Where(action => action.DeviceOptionId == deviceOption.Id))
                    {
                        await db.Delete(action).ExecuteAsync();
                    }

                    await db.Delete(deviceOption).ExecuteAsync();
                }

                await db.Delete(databaseDeviceToDelete).ExecuteAsync();
            }

            return await ViewModelResponse();
        }

        public void OnPostClearTPLinkDevicesCache()
        {
            if (deviceService.TryGetIntegrationDeviceCache<HomeControl.Integrations.TPLink.DeviceCache>(out var cache))
            {
                cache.InvalidateAll();
            }
        }
    }
}