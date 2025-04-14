using HomeControl.Integrations;
using HomeControl.Database;
using Microsoft.AspNetCore.Mvc;
using HomeControl.DatabaseModels;
using HomeControl.Attributes;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HomeControl.Pages.Devices
{
    [MenuPage(null, "Devices", "/Devices/Index")]
    public class IndexModel(IDatabaseConnection db, IDeviceService deviceService) : ViewModelPageModel<IndexModel.DevicesViewModel>
    {
        public class DevicesViewModel : PageViewModel
        {
            private DevicesViewModel(IndexModel page, IDatabaseConnection db, IDeviceService deviceService) : base(page)
            {
                _db = db;
                _deviceService = deviceService;
            }

            private readonly IDatabaseConnection _db;
            private readonly IDeviceService _deviceService;

            public List<DeviceInfo> Devices { get; } = [];

            public static async Task<DevicesViewModel> CreateAsync(IndexModel page, IDatabaseConnection db, IDeviceService deviceService)
            {
                var instance = new DevicesViewModel(page, db, deviceService);
                await instance.PopulateDevicesAsync();
                return instance;
            }

            private async Task PopulateDevicesAsync()
            {
                var databaseDevices = await _db.SelectAllAsync<Device>();

                var deviceOptions = await _db.SelectAllAsync<DeviceOption>();

                var devices = new List<DeviceInfo>();

                foreach (var databaseDevice in databaseDevices)
                {
                    var integrationDevice = _deviceService.CreateIntegrationDevice(databaseDevice);

                    await integrationDevice.InitializeAsync();

                    devices.Add(new DeviceInfo(integrationDevice, deviceOptions.Where(option => option.DeviceId == databaseDevice.Id)));
                }

                Devices.AddRange(devices.OrderBy(device => device.Device.DisplayName));
            }
        }

        public class DeviceInfo(IIntegrationDevice device, IEnumerable<DeviceOption> options)
        {
            public IIntegrationDevice Device { get; } = device;

            public IEnumerable<DeviceOption> Options { get; } = options;

            public string PrimaryOption { get; } = device.GetExecutableFeatures().FirstOrDefault()?.Name;
        }

        public async Task OnGet()
        {
            ViewModel = await DevicesViewModel.CreateAsync(this, db, deviceService);
        }

        public async Task<IActionResult> OnPostExecuteFeature(int id, string featureName)
        {
            await deviceService.ExecuteFeatureAsync(id, featureName);

            ViewModel = await DevicesViewModel.CreateAsync(this, db, deviceService);
            return ViewModelResponse();
        }

        public async Task<IActionResult> OnPostExecuteOption(int optionId)
        {
            await deviceService.ExecuteDeviceOptionAsync(optionId);

            ViewModel = await DevicesViewModel.CreateAsync(this, db, deviceService);
            return ViewModelResponse();
        }

        public async Task<IActionResult> OnPostRename(int deviceId, string displayName)
        {
            var device = await db.SelectSingleAsync<Device>(deviceId);

            var integrationDevice = deviceService.CreateIntegrationDevice(device);

            await integrationDevice.RenameAsync(displayName);

            return RedirectToPage();
        }
    }
}
