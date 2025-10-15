using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using TempFolderApp.Models;

namespace TempFolderApp.Services
{
    public class TempCleaner
    {
        public void CleanFolder(TempFolderConfig config)
        {
            if (!Directory.Exists(config.Path))
            {
                Console.WriteLine($"erro ao procurar diretorio: {config.Path}");
                return;
            }

            var files = Directory.GetFiles(config.Path);
            var dirs = Directory.GetDirectories(config.Path);

            Console.WriteLine($"limpando: {config.Path} ({config.DeleteMode})");

            foreach (var file in files)
            {
                try
                {
                    if (config.DeleteMode == TempFolderConfig.Options.RecycleBin)
                        FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    else
                        File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"erro ao tentar deletar arquivo {file}: {ex.Message}");
                }
            }

            foreach (var dir in dirs)
            {
                try
                {
                    if (config.DeleteMode == TempFolderConfig.Options.PermanentDelete)
                        FileSystem.DeleteDirectory(dir, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    else
                        Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"erro ao tentar deletar pasta {dir}: {ex.Message}");
                }
            }
        }
    }
}
