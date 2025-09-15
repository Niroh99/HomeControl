using HomeControl.Attributes;
using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;
using HomeControl.ViewModels.Devices;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Routines", "/Devices/Routines")]
    public partial class RoutinesModel(IServiceProvider serviceProvider, IDatabaseConnectionService db) : ViewModelPageModel<RoutinesViewModel>(serviceProvider)
    {
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostCreateRoutine(string routineName)
        {
            if (string.IsNullOrWhiteSpace(routineName)) return RedirectToPage();

            var routine = new Routine
            {
                Name = routineName,
                IsActive = true,
            };

            await db.Insert(routine).ExecuteAsync();

            return RedirectToPage("/Devices/EditRoutine", new { RoutineId = routine.Id });
        }
    }
}
