using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace TempFolderApp.Services
{
    /// <summary>
    /// Classe responsável por perguntar ao usuário se o aplicativo deve iniciar junto com o Windows.
    /// Caso o usuário aceite, uma tarefa agendada é criada no Agendador de Tarefas do Windows.
    /// Também salva as preferências em um arquivo "startup_config.json".
    /// </summary>
    public class SchedulerOnStartup
    {
        /// <summary>
        /// Representa a estrutura básica do arquivo de configuração salvo em JSON.
        /// </summary>
        private class Config
        {
            public bool NaoPerguntar { get; set; } = false; // Indica se o usuário marcou "Não perguntar novamente"
            public bool IniciarComWindows { get; set; } = false; // Indica se o usuário quer iniciar com o Windows
        }

        // Diretório onde o aplicativo está sendo executado
        private static readonly string AppFolder = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        // Caminho completo do executável do programa
        private static readonly string ExePath = Process.GetCurrentProcess().MainModule.FileName;

        // Caminho completo do arquivo de configuração JSON
        private static readonly string ConfigPath = Path.Combine(AppFolder, "startup_config.json");

        // Nome da tarefa agendada criada no Windows
        private const string TaskName = "TempFolderAppAutoStart";

        /// <summary>
        /// Deve ser chamada no início do programa (no Main) para verificar e perguntar ao usuário
        /// se ele deseja que o app inicie automaticamente junto com o Windows.
        /// </summary>
        public static void AppStartup()
        {
            try
            {
                var config = LoadConfig(); // Carrega as configurações atuais do JSON

                // Se o usuário marcou "Não perguntar novamente"
                if (config.NaoPerguntar)
                {
                    // Se ele também marcou "Iniciar com o Windows"
                    if (config.IniciarComWindows)
                    {
                        // Garante que a tarefa ainda exista; se foi deletada, recria
                        if (!ScheduledTaskExists(TaskName))
                        {
                            CreateScheduledTaskAndLog();
                        }
                    }
                    return; // Não faz mais nada se "não perguntar novamente" estiver ativo
                }

                // Mostra o diálogo perguntando se deseja iniciar junto com o Windows
                var dialogResult = ShowStartupDialog(out bool naoPerguntar, out bool queroIniciar);

                // Atualiza o estado das configurações com base nas escolhas do usuário
                config.NaoPerguntar = naoPerguntar;
                config.IniciarComWindows = queroIniciar;

                // Se o usuário quis iniciar com o Windows, cria a tarefa agendada
                if (queroIniciar)
                {
                    var ok = CreateScheduledTaskAndLog();
                    if (!ok)
                    {
                        MessageBox.Show(
                            "Não foi possível criar a tarefa agendada automaticamente. " +
                            "Rode o programa como administrador ou crie a tarefa manualmente no Agendador de Tarefas.",
                            "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning
                        );
                    }
                }

                // Salva as preferências no arquivo JSON
                SaveConfig(config);
            }
            catch (Exception ex)
            {
                // Caso algo dê errado, tenta registrar o erro em log
                TryWriteLog($"Erro em AppStartup: {ex}");
            }
        }

        /// <summary>
        /// Lê as configurações do arquivo startup_config.json.
        /// Se não existir, retorna uma configuração padrão.
        /// </summary>
        private static Config LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return new Config(); // retorna valores padrão

                var json = File.ReadAllText(ConfigPath);
                var cfg = JsonSerializer.Deserialize<Config>(json);
                return cfg ?? new Config();
            }
            catch (Exception ex)
            {
                TryWriteLog($"Falha ao carregar config.json: {ex}");
                return new Config();
            }
        }

        /// <summary>
        /// Salva as configurações atuais no arquivo startup_config.json.
        /// </summary>
        private static void SaveConfig(Config cfg)
        {
            try
            {
                var opts = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, opts));
            }
            catch (Exception ex)
            {
                TryWriteLog($"Falha ao salvar config.json: {ex}");
            }
        }

        /// <summary>
        /// Exibe uma janela perguntando se o app deve iniciar com o Windows.
        /// Retorna true se o usuário respondeu Sim ou Não (ou seja, fechou o diálogo).
        /// </summary>
        private static bool ShowStartupDialog(out bool naoPerguntar, out bool queroIniciar)
        {
            naoPerguntar = false;
            queroIniciar = false;

            // Cria o formulário (janela) dinamicamente, sem usar Designer
            using (var form = new Form())
            {
                form.Text = "Executar com o Windows";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ClientSize = new System.Drawing.Size(420, 160);
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ShowIcon = false;

                // Texto explicativo
                var label = new Label
                {
                    AutoSize = false,
                    Text = "Deseja que o TempFolderApp seja executado automaticamente\nquando o Windows iniciar?",
                    Left = 12,
                    Top = 12,
                    Width = 396,
                    Height = 60
                };

                // Checkbox: "Não perguntar novamente"
                var chk = new CheckBox
                {
                    Text = "Não perguntar novamente",
                    Left = 12,
                    Top = 78,
                    Width = 250
                };

                // Botão "Sim"
                var btnSim = new Button
                {
                    Text = "Sim",
                    DialogResult = DialogResult.Yes,
                    Left = 100,
                    Width = 90,
                    Top = 100
                };

                // Botão "Não"
                var btnNao = new Button
                {
                    Text = "Não",
                    DialogResult = DialogResult.No,
                    Left = 210,
                    Width = 90,
                    Top = 100
                };

                // Adiciona os componentes à janela
                form.Controls.Add(label);
                form.Controls.Add(chk);
                form.Controls.Add(btnSim);
                form.Controls.Add(btnNao);

                // Define o botão padrão (Enter) e o botão de cancelamento (Esc)
                form.AcceptButton = btnSim;
                form.CancelButton = btnNao;

                // Mostra o diálogo e captura a resposta
                var result = form.ShowDialog();

                naoPerguntar = chk.Checked; // se o usuário marcou o checkbox
                queroIniciar = (result == DialogResult.Yes); // se clicou em "Sim"

                // Retorna se o usuário deu uma resposta válida
                return result == DialogResult.Yes || result == DialogResult.No;
            }
        }

        /// <summary>
        /// Cria a tarefa agendada e registra logs de sucesso ou falha.
        /// </summary>
        private static bool CreateScheduledTaskAndLog()
        {
            try
            {
                var success = CreateScheduledTask();
                if (success)
                {
                    TryWriteLog("Tarefa agendada criada/atualizada com sucesso.");
                    return true;
                }
                else
                {
                    TryWriteLog("Falha ao criar tarefa agendada (retorno false).");
                    return false;
                }
            }
            catch (Exception ex)
            {
                TryWriteLog($"Exception ao criar tarefa: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Cria a tarefa agendada do Windows para iniciar o programa junto com o login do usuário.
        /// Usa o utilitário schtasks.exe.
        /// </summary>
        private static bool CreateScheduledTask()
        {
            string exe = ExePath;
            string folder = AppFolder;

            // Comando que será executado pela tarefa agendada:
            // cmd.exe /c "cd /d "<pasta>" && "<executável>""
            string cmdAction = $"cmd.exe /c \"cd /d \\\"{folder}\\\" && \\\"{exe}\\\"\"";

            // Argumentos do schtasks:
            // /TN = nome da tarefa
            // /TR = comando a executar
            // /SC ONLOGON = dispara no login do usuário
            // /RL HIGHEST = tenta executar com privilégios elevados
            // /F = substitui caso já exista
            // /IT = executa no contexto interativo (aparece para o usuário)
            string args = $"/Create /TN \"{TaskName}\" /TR \"{cmdAction}\" /SC ONLOGON /RL HIGHEST /F /IT";

            TryWriteLog($"Executando schtasks com args: {args}");

            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
            {
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();

                TryWriteLog($"schtasks exitCode={p.ExitCode}");
                if (!string.IsNullOrWhiteSpace(stdout)) TryWriteLog("schtasks stdout: " + stdout);
                if (!string.IsNullOrWhiteSpace(stderr)) TryWriteLog("schtasks stderr: " + stderr);

                // ExitCode 0 = sucesso
                return p.ExitCode == 0;
            }
        }

        /// <summary>
        /// Verifica se a tarefa agendada já existe no Agendador de Tarefas.
        /// </summary>
        private static bool ScheduledTaskExists(string name)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/Query /TN \"{name}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    var outp = p.StandardOutput.ReadToEnd();
                    var err = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    // Se o comando retornou 0 e o nome da tarefa está listado, ela existe
                    if (p.ExitCode == 0 && outp.Contains(name, StringComparison.OrdinalIgnoreCase))
                        return true;

                    // Caso contrário, trata como inexistente
                    TryWriteLog($"schtasks /Query retornou exit={p.ExitCode}. out: {outp}. err: {err}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                TryWriteLog($"Erro ao checar existência da tarefa: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Grava mensagens de log em um arquivo "startup_debug.log" no mesmo diretório do app.
        /// Silencioso — não lança exceções.
        /// </summary>
        private static void TryWriteLog(string text)
        {
            try
            {
                var logFile = Path.Combine(AppFolder, "startup_debug.log");
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}{Environment.NewLine}";
                File.AppendAllText(logFile, line);
            }
            catch
            {
                // Ignora erros de log
            }
        }
    }
}
