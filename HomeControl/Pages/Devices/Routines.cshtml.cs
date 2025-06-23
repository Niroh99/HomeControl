using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;
using Microsoft.AspNetCore.Mvc;

namespace HomeControl.Pages.Devices
{
    [MenuPage(typeof(IndexModel), "Routines", "/Devices/Routines")]
    public class RoutinesModel(IDatabaseConnectionService db) : ViewModelPageModel<RoutinesModel.RoutinesViewModel>
    {
        public class RoutinesViewModel(RoutinesModel page, IDatabaseConnectionService db) : PageViewModel(page)
        {
            public List<Routine> Routines { get; } = [];

            public override async Task Initialize()
            {
                Routines.AddRange(await db.Select<Routine>().ExecuteAsync());
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

            await db.Insert(routine).ExecuteAsync();

            return RedirectToPage("/Devices/EditRoutine", new { RoutineId = routine.Id });
        }

        protected override PageViewModel CreateViewModel()
        {
            return new RoutinesViewModel(this, db);
        }
    }
}
