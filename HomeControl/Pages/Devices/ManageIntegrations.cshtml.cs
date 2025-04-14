using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Manage Integrations", "/Devices/ManageIntegrations")]
    public class ManageIntegrationsModel(IDatabaseConnection db) : PageModel
    {
        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostDiscoverDevices()
        {
            var databaseDevices = await db.SelectAllAsync<Device>();

            var tpLinkDevices = Integrations.TPLink.Discovery.Discover();

            var rediscoveredDeviceIds = new List<int>();

            foreach (var tpLinkDevice in tpLinkDevices)
            {
                var databaseDevice = db.SelectAsync(WhereBuilder.Where<Device>().Compare(i => i.Hostname, ComparisonOperator.Equals, tpLinkDevice.Hostname));

                if (databaseDevice == null)
                {
                    await db.InsertAsync(new Device()
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

            return Redirect("/Devices");
        }
    }
}