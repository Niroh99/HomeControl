using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Routines", "/Devices/Routines")]
    public class RoutinesModel(IDatabaseConnection db) : ViewModelPageModel<RoutinesModel.RoutinesViewModel>
    {
        public class RoutinesViewModel(RoutinesModel page, IDatabaseConnection db) : PageViewModel(page)
        {
            public List<Routine> Routines { get; } = [];

            public override async Task Initialize()
            {
                Routines.AddRange(await db.SelectAllAsync<Routine>());
            }
        }

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

            await db.InsertAsync(routine);

            return RedirectToPage("/Devices/EditRoutine", new { RoutineId = routine.Id });
        }

        protected override PageViewModel CreateViewModel()
        {
            return new RoutinesViewModel(this, db);
        }
    }
}
