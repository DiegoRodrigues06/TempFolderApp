using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TempFolderApp.Models
{
    public class AppConfig
    {
        public List<TempFolderConfig> TempFolders { get; set; } = new();
        public double IntervalHours { get; set; }
    }

    public class TempFolderConfig
    {
        // Tem que ser public tanto o config.json quanto as outras variaveis conseguirem acessar
        public enum Options
        {
            RecycleBin,
            PermanentDelete
        }

        public string Path { get; set; } = string.Empty;
        public Options DeleteMode { get; set; } = Options.RecycleBin; // padrão é enviar para lixeira
    }
}
