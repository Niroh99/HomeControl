using HomeControl.Integrations;
using HomeControl.Models;
using HomeControl.Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    public class IndexModel(IDatabaseConnection db) : MenuPageModel(MenuItem)
    {
        public const string MenuItem = "Devices";
        public const string MenuItemUrl = "/Devices";

        public List<IDevice> Devices { get; } = new List<IDevice>();

        public async Task<IActionResult> OnGet()
        {
            Initialize();

            await PopulateDevicesAsync(await db.SelectAllAsync<Device>());

            return null;
        }

        public Dictionary<string, string> GetDeviceFeatureRouteData(IDevice device, Feature feature)
        {
            return new Dictionary<string, string>
            {
                { "deviceId", device.Owner.Id.ToString() },
                { "featureName", feature.Name },
            };
        }

        public async Task<IActionResult> OnPostExecuteFeature(int deviceId, string featureName)
        {
            var device = await db.SelectAsync<Device>(deviceId);

            var integrationDevice = device.Create();

            var feature = integrationDevice.GetExecutableFeatures().FirstOrDefault(x => x.Name == featureName);

            if (feature != null) await feature.Execute.Invoke();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRename(int deviceId, string displayName)
        {
            var device = await db.SelectAsync<Device>(deviceId);

            var integrationDevice = device.Create();

            await integrationDevice.RenameAsync(displayName);

            return RedirectToPage();
        }

        private async Task PopulateDevicesAsync(List<Device> deviceList)
        {
            for (int i = 0; i < deviceList.Count; i++)
            {
                var device = deviceList[i];

                var integrationDevice = device.Create();

                Devices.Add(integrationDevice);

                await integrationDevice.InitializeAsync();
            }
        }
    }
}
