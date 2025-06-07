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
    public class ManageIntegrationsModel(IDatabaseConnection db, IDeviceService deviceService) : ViewModelPageModel<ManageIntegrationsModel.ManageIntegrationsViewModel>
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
            var databaseDevices = await db.SelectAllAsync<Device>();

            var tpLinkDevices = HomeControl.Integrations.TPLink.Discovery.Discover();

            var rediscoveredDeviceIds = new List<int>();

            foreach (var tpLinkDevice in tpLinkDevices)
            {
                var databaseDevice = (await db.SelectAsync(WhereBuilder.Where<Device>().Compare(i => i.Hostname, ComparisonOperator.Equals, tpLinkDevice.Hostname))).FirstOrDefault();

                if (databaseDevice == null)
                {
                    await db.InsertAsync(new Device
                    {
                        Type = tpLinkDevice.DeviceType,
                        Hostname = tpLinkDevice.Hostname,
                        Port = tpLinkDevice.Port,
                    });
                }
                else rediscoveredDeviceIds.Add(databaseDevice.Id);
            }

            var deviceOptions = await db.SelectAllAsync<DeviceOption>();

            var deviceOptionActions = await db.SelectAllAsync<DeviceOptionAction>();

            foreach (var databaseDeviceToDelete in databaseDevices.Where(databaseDevice => !rediscoveredDeviceIds.Contains(databaseDevice.Id)))
            {
                foreach (var deviceOption in deviceOptions.Where(option => option.DeviceId == databaseDeviceToDelete.Id))
                {
                    foreach (var action in deviceOptionActions.Where(action => action.DeviceOptionId == deviceOption.Id))
                    {
                        await db.DeleteAsync(action);
                    }

                    await db.DeleteAsync(deviceOption);
                }

                await db.DeleteAsync(databaseDeviceToDelete);
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