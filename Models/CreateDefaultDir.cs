using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TempFolderApp.Models
{
    public class CreateDefaultDir
    {
        public static void mkdir(string path)
        {
            // Verifica se o diretório já existe
            if (!Directory.Exists(path))
            {
                try
                {
                    // Cria o diretório
                    Directory.CreateDirectory(path);
                    Console.WriteLine($"[INFO] Diretório '{path}' criado com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERRO] Falha ao criar o diretório: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[INFO] Diretório '{path}' já existe.");
            }
        }
    }
}
