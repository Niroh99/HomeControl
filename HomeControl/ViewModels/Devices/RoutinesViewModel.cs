using HomeControl.Models.DatabaseModels;
using HomeControl.Models.ServicesInterfaces;

namespace HomeControl.ViewModels.Devices
{
    public class RoutinesViewModel(IDatabaseConnectionService db) : PageViewModel
    {
        public List<Routine> Routines { get => GetList<Routine>(); }

        public override async Task Initialize()
        {
            Routines.AddRange(await db.Select<Routine>().ExecuteAsync());
        }
    }
}