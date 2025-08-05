using Life;
using Life.InventorySystem;
using Life.Network;
using Life.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Menu581
{
    public class Main : Plugin
    {
        public string directoryPath;
        public string configPath;
        public Config config;

        public Main(IGameAPI api) : base(api) { }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            directoryPath = Path.Combine(pluginsPath, Assembly.GetExecutingAssembly().GetName().Name);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            configPath = Path.Combine(directoryPath, "config.json");
            if (!File.Exists(configPath))
            {
                config = new Config();
                var aaMenuPath = Path.Combine(pluginsPath, "AAMenu.dll");
                if (File.Exists(aaMenuPath))
                {
                    config.key = KeyCode.Y;
                }
                File.WriteAllText(configPath, Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
            }
            else
            {
                config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{Assembly.GetExecutingAssembly().GetName().Name} initialise !");
            Console.ResetColor();
        }

        public override void OnPlayerInput(Player player, KeyCode keyCode, bool onUI)
        {
            base.OnPlayerInput(player, keyCode, onUI);
            if (onUI)
                return;
            if (keyCode == config.key)
            {
                OpenMenu(player);
            }
        }

        public void OpenMenu(Player player)
        {
            var panel = new UIPanel(config.name, UIPanel.PanelType.Tab);
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.AddButton("Sélectionner", async ui =>
            {
                player.ClosePanel(ui);
                await Task.Delay(1);
                ui.SelectTab();
            });
            var files = Directory.GetFiles(pluginsPath, "*.dll");
            foreach (var file in files.Where(obj => obj.Contains("581")).ToList())
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name == Assembly.GetExecutingAssembly().GetName().Name)
                    continue;
                var command = name.Replace("581", "").ToLower();
                command = "/" + command;
                if (Nova.server.chat.commands.Any(obj => obj.fullCommandName == command || obj.aliases.Contains(command)))
                {
                    panel.AddTabLine(name, ui =>
                    {
                        Nova.server.chat.RunCommands(player, command, new string[] { });
                    });
                }
            }
            panel.AddTabLine($"Paramètre", ui =>
            {
                OpenSettingsMenu(player);
            });
            player.ShowPanelUI(panel);
        }

        public void OpenSettingsMenu(Player player)
        {
            var panel = new UIPanel($"Paramètre {config.name}", UIPanel.PanelType.Tab);
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.AddButton("Sélectionner", ui => ui.SelectTab());
            panel.AddButton("Retour", ui => OpenMenu(player));
            panel.AddTabLine($"Nom : {config.name}", ui =>
            {
                OpenInputNameMenu(player);
            });
            panel.AddTabLine($"Touche : {Enum.GetName(typeof(KeyCode), config.key)}", ui =>
            {
                OpenInputKeyMenu(player);
            });
            player.ShowPanelUI(panel);
        }

        public void OpenInputNameMenu(Player player)
        {
            var panel = new UIPanel($"Modification du Nom", UIPanel.PanelType.Input);
            panel.SetText("Veuillez définir le nom du menu.");
            panel.SetInputPlaceholder("Nom :");
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.AddButton("Valider", ui =>
            {
                if (!string.IsNullOrEmpty(ui.inputText))
                {
                    config.name = ui.inputText;
                    File.WriteAllText(configPath, Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                    player.Notify("Menu581", $"Le nom du menu a été modifié avec succès.", NotificationManager.Type.Success);
                    OpenSettingsMenu(player);
                }
                else
                    player.Notify("Menu581", "Format invalide.", NotificationManager.Type.Error);
            });
            panel.AddButton("Retour", ui => OpenSettingsMenu(player));
            player.ShowPanelUI(panel);
        }

        public void OpenInputKeyMenu(Player player)
        {
            var panel = new UIPanel($"Modification de la touche", UIPanel.PanelType.Tab);
            panel.AddButton("Fermer", ui => player.ClosePanel(ui));
            panel.AddButton("Sélectionner", ui => ui.SelectTab());
            panel.AddButton("Retour", ui => OpenSettingsMenu(player));
            foreach (var value in Enum.GetValues(typeof(KeyCode)))
            {
                if (value is KeyCode key)
                {
                    panel.AddTabLine(Enum.GetName(typeof(KeyCode), key), ui =>
                    {
                        config.key = key;
                        File.WriteAllText(configPath, Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                        player.Notify("Menu581", $"La touche a été modifiée avec succès.", NotificationManager.Type.Success);
                        OpenSettingsMenu(player);
                    });
                }
            }
            player.ShowPanelUI(panel);
        }
    }
}
