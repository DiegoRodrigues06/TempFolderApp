using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace TempFolderApp.Services
{
    public class SchedulerOnStartup
    {
        public static void AppStartup()
        {
            // pega o caminho do executável atual.
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            string appName = "TempFolderApp"; // Nome da aplicação, vai aparecer como o nome do registro

            // Obtém o acesso à chave de registro "Run".
            RegistryKey startupKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // Adiciona o valor à chave.
            if (startupKey.GetValue(appName) == null)
            {
                startupKey.SetValue(appName, appPath);
                Console.WriteLine("[INFO] Programa agendado para iniciar com o Windows.");
            }
            else
            {
                Console.WriteLine("[INFO] Programa já está agendado para iniciar com o Windows.");
            }

            // Fecha a chave.
            startupKey.Close();
        }
    }
}
