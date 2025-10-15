using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TempFolderApp.Models;
using static TempFolderApp.Models.TempFolderConfig;

namespace TempFolderApp.Services
{
    internal class Scheduler
    {
        /* readonly é usado para garantir que a variável só possa ser atribuída no construtor
        começar com um _ é uma convenção para campos privados
        */
        private readonly List<TempFolderConfig> _folders;

        /* intervalo em horas entre cada verificação, pega do arquivo json
         percebi que não precisa de "=" pra fazer atribuição, pq é feito no construtor 
        */
        private readonly double _intervalHours;

        //cria um objeto de TempCleaner para ser usado no Scheduler
        private readonly TempCleaner _cleaner = new();

        public Scheduler(string configPath) // o configPath armazena o caminho pra um arquivo de configuração, por meio do construtor
        {
            var json = File.ReadAllText(configPath);

            var options = new JsonSerializerOptions(); // o jsonSerializerOptions é usado para configurar a desserialização (converter enums)
            options.Converters.Add(new JsonStringEnumConverter());

            var config = JsonSerializer.Deserialize<AppConfig>(json, options)
                ?? throw new Exception("Falha ao ler o arquivo de configuração.");

            _folders = config.TempFolders;
            _intervalHours = config.IntervalHours;
        }

        public async Task StartAsync()
        {
            while (true)
            {
                Console.WriteLine($"[INFO] Iniciando limpeza em {DateTime.Now}");

                foreach (var folder in _folders)
                    _cleaner.CleanFolder(folder);

                Console.WriteLine($"[INFO] Próxima verificação em {_intervalHours} horas.\n");
                await Task.Delay(TimeSpan.FromHours(_intervalHours));
            }
        }
    }
}
