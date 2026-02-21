using HomeControl.Models.ServicesInterfaces;
using NTIH.Database;

namespace HomeControl.Database
{
    public class DatabaseConnectionService(string connectionString) : DatabaseConnection(connectionString), IDatabaseConnectionService
    {

    }
}