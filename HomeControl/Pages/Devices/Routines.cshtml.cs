using HomeControl.Attributes;
using HomeControl.Database;
using HomeControl.DatabaseModels;

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

        protected override PageViewModel CreateViewModel()
        {
            return new RoutinesViewModel(this, db);
        }
    }
}
