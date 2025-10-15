using TempFolderApp.Services;
using TempFolderApp.Models;

namespace TempFolderApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string defaultPath = "C:\\TempFolder";
            CreateDefaultDir.mkdir(defaultPath);

            try
            {
                SchedulerOnStartup.AppStartup();
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("permissão negada. Execute o programa como administrador.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"algo deu errado: {ex.Message}");
            }

            var scheduler = new Scheduler("config.json");
            await scheduler.StartAsync();
        }
    }
}
