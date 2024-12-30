using HomeControl.Integrations;
using HomeControl.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    public class IndexModel : MenuPageModel
    {
        public const string MenuItem = "Devices";
        public const string MenuItemUrl = "/Devices";

        public IndexModel() : base(MenuItem)
        {
            
        }

        public List<IDevice> Devices { get; } = new List<IDevice>();

        public override void OnGet()
        {
            base.OnGet();

            Devices.AddRange(CreateDevices(Device.SelectAll()).OrderBy(x => x.DisplayName));
        }

        public Dictionary<string, string> GetDeviceFeatureRouteData(IDevice device, Feature feature)
        {
            return new Dictionary<string, string>
            {
                { "deviceId", device.Owner.Id.ToString() },
                { "featureName", feature.Name },
            };
        }

        public IActionResult OnPostExecuteFeature(int deviceId, string featureName)
        {
            var device = Device.Select(deviceId);

            var integrationDevice = device.Create();

            var feature = integrationDevice.GetExecutableFeatures().FirstOrDefault(x => x.Name == featureName);

            if (feature != null) feature.Execute.Invoke();
            
            return RedirectToPage();
        }

        public IActionResult OnPostRename(int deviceId, string displayName)
        {
            var device = Device.Select(deviceId);

            var integrationDevice = device.Create();

            integrationDevice.Rename(displayName);

            return RedirectToPage();
        }

        private IEnumerable<IDevice> CreateDevices(List<Device> deviceList)
        {
            foreach (var device in deviceList)
            {
                yield return device.Create();
            }
        }
    }
}
