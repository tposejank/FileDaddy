﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Diagnostics;
using System.Reflection;

namespace FNF_Mod_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Config config;
        public Logger logger;
        public string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        // Separated from config so that order is updated when datagrid is modified
        public ObservableCollection<Mod> ModList;
        public List<string> exes;
        private FileSystemWatcher ModsWatcher;

        public MainWindow()
        {
            InitializeComponent();
            logger = new Logger(ConsoleWindow);
            config = new Config();

            // Get Version Number
            var FNFMMVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            var version = FNFMMVersion.Substring(0, FNFMMVersion.LastIndexOf('.'));
            Title = $"FileDaddy v{version}";

            logger.WriteLine($"Launched FileDaddy v{version}!", LoggerType.Info);
            // Get config if it exists
            if (File.Exists($@"{assemblyLocation}/Config.json"))
            {
                try
                {
                    string configString = File.ReadAllText($@"{assemblyLocation}/Config.json");
                    config = JsonSerializer.Deserialize<Config>(configString);
                }
                catch (Exception e)
                {
                    logger.WriteLine(e.Message, LoggerType.Error);
                }
            }

            if (config.ModList == null)
                config.ModList = new ObservableCollection<Mod>();

            ModList = config.ModList;

            if (config.exe == null || !File.Exists(config.exe))
                logger.WriteLine("Please set up Game Path in Config.", LoggerType.Warning);
            else if (config.exe != null)
                logger.WriteLine($"Current Game Path set as {config.exe}", LoggerType.Info);

            // Create Mods Directory if it doesn't exist
            Directory.CreateDirectory($@"{assemblyLocation}/Mods");
            Refresh();

            // Watch mods folder to detect
            ModsWatcher = new FileSystemWatcher($@"{assemblyLocation}/Mods");
            ModsWatcher.Created += OnModified;
            ModsWatcher.Deleted += OnModified;
            ModsWatcher.Renamed += OnModified;

            ModsWatcher.EnableRaisingEvents = true;
        }
        private void OnModified(object sender, FileSystemEventArgs e)
        {
            Refresh();
            UpdateConfig();
            // Bring window to front after download is done
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Activate();
            });
        }

        private void Refresh()
        {
            // Add new folders found in Mods to the ModList
            foreach (var mod in Directory.GetDirectories($@"{assemblyLocation}/Mods"))
            {
                if (ModList.ToList().Where(x => x.name == Path.GetFileName(mod)).Count() == 0)
                {
                    logger.WriteLine($"Adding {Path.GetFileName(mod)}", LoggerType.Info);
                    Mod m = new Mod();
                    m.name = Path.GetFileName(mod);
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ModList.Add(m);
                    });
                }
            }
            // Remove deleted folders that are still in the ModList
            foreach (var mod in ModList.ToList())
            {
                if (!Directory.GetDirectories($@"{assemblyLocation}/Mods").ToList().Select(x => Path.GetFileName(x)).Contains(mod.name))
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        ModList.Remove(mod);
                    });
                    logger.WriteLine($"{mod.name} was deleted.", LoggerType.Info);
                }
            }
            // Move all enabled mods to top
            ModList = new ObservableCollection<Mod>(ModList.ToList().OrderByDescending(x => x.enabled).ToList());

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                ModGrid.ItemsSource = ModList;
            });
            config.ModList = ModList;
            logger.WriteLine("Refreshed!", LoggerType.Info);
        }

        // Events for Enabled checkboxes
        private void OnChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;

            Mod mod = checkBox?.DataContext as Mod;

            if (mod != null)
            {
                mod.enabled = true;
                List<Mod> temp = config.ModList.ToList();
                foreach (var m in temp)
                {
                    if (m.name == mod.name)
                        m.enabled = true;
                }
                config.ModList = new ObservableCollection<Mod>(temp);
                UpdateConfig();
            }
        }
        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;

            Mod mod = checkBox?.DataContext as Mod;

            if (mod != null)
            {
                mod.enabled = false;
                List<Mod> temp = config.ModList.ToList();
                foreach (var m in temp)
                {
                    if (m.name == mod.name)
                        m.enabled = false;
                }
                config.ModList = new ObservableCollection<Mod>(temp);
                UpdateConfig();
            }
        }
        // Triggered when priority is switched on drag and dropped
        private void ModGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            UpdateConfig();
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow(this);
            configWindow.ShowDialog();
        }
        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            if (config.exe != null && File.Exists(config.exe))
            {
                logger.WriteLine($"Launching {config.exe}", LoggerType.Info);
                try
                {
                    var ps = new ProcessStartInfo(config.exe)
                    {
                        // Game throws error if not launched from same directory
                        WorkingDirectory = (Path.GetDirectoryName(config.exe)),
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(ps);
                }
                catch (Exception ex)
                {
                    logger.WriteLine($"Couldn't launch {config.exe} ({ex.Message})", LoggerType.Error);
                }
            }
            else
                logger.WriteLine($"Please set up your Game Path in Config!", LoggerType.Warning);
        }
        private void GameBanana_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ps = new ProcessStartInfo("https://gamebanana.com/games/8694")
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
            catch (Exception ex)
            {
                logger.WriteLine($"Couldn't open up GameBanana ({ex.Message})", LoggerType.Error);
            }
        }
        private void ScrollToBottom(object sender, TextChangedEventArgs args)
        {
            ConsoleWindow.ScrollToEnd();
        }

        private void ModGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            ContextMenu contextMenu = element.ContextMenu;
            if (ModGrid.SelectedItem == null)
                element.ContextMenu.Visibility = Visibility.Collapsed;
            else
                element.ContextMenu.Visibility = Visibility.Visible;
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            Mod row = (Mod)ModGrid.SelectedItem;
            if (row != null)
            {
                var dialogResult = MessageBox.Show($@"Are you sure you want to delete {row.name}?" + Environment.NewLine + "This cannot be undone.", $@"Deleting {row.name}: Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        Directory.Delete($@"{assemblyLocation}/Mods/{row.name}", true);
                        logger.WriteLine($@"[INFO] Deleting {row.name}.", LoggerType.Info);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLine($@"Couldn't delete {row.name} ({ex.Message})", LoggerType.Error);
                    }
                }
            }
        }
        public void UpdateConfig()
        {
            config.ModList = ModList;
            string configString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                File.WriteAllText($@"{assemblyLocation}/Config.json", configString);
            }
            catch (Exception e)
            {
                logger.WriteLine($"Couldn't write Config.json ({e.Message})", LoggerType.Error);
            }
        }

        private async void Build_Click(object sender, RoutedEventArgs e)
        {
            if (config.exe != null && Directory.Exists($@"{Path.GetDirectoryName(config.exe)}/Assets"))
            {
                ModGrid.IsHitTestVisible = false;
                ConfigButton.IsHitTestVisible = false;
                BuildButton.IsHitTestVisible = false;
                LaunchButton.IsHitTestVisible = false;
                OpenModsButton.IsHitTestVisible = false;
                Refresh();
                await Build($@"{Path.GetDirectoryName(config.exe)}/assets");
                ModGrid.IsHitTestVisible = true;
                ConfigButton.IsHitTestVisible = true;
                BuildButton.IsHitTestVisible = true;
                LaunchButton.IsHitTestVisible = true;
                OpenModsButton.IsHitTestVisible = true;
                MessageBox.Show($@"Finished building loadout and ready to launch!", "Notification", MessageBoxButton.OK);
            }
            else
                logger.WriteLine("Please set up correct Game Path in Config", LoggerType.Warning);
        }

        private async Task Build(string path)
        {
            await Task.Run(() =>
            { 
                ModLoader.Restart(path, logger);
                List<string> mods = config.ModList.Where(x => x.enabled).Select(y => $@"{assemblyLocation}/Mods/{y.name}").ToList();
                mods.Reverse();
                ModLoader.Build(path, mods, logger);
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenItem_Click(object sender, RoutedEventArgs e)
        {
            Mod row = (Mod)ModGrid.SelectedItem;
            if (row != null)
            {
                var folderName = $@"{assemblyLocation}\Mods\{row.name}";
                if (Directory.Exists(folderName))
                {
                    try
                    {
                        Process process = Process.Start("explorer.exe", folderName);
                        logger.WriteLine($@"Opened {folderName}.", LoggerType.Info);
                    }
                    catch (Exception ex)
                    {
                        logger.WriteLine($@"Couldn't open {folderName}. ({ex.Message})", LoggerType.Error);
                    }
                }
            }
        }
        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            Mod row = (Mod)ModGrid.SelectedItem;
            if (row != null)
            {
                EditWindow ew = new EditWindow(row, logger);
                ew.ShowDialog();
            }
        }
        private void ModsFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderName = $@"{assemblyLocation}\Mods";
            if (Directory.Exists(folderName))
            {
                try
                {
                    Process process = Process.Start("explorer.exe", folderName);
                    logger.WriteLine($@"Opened {folderName}.", LoggerType.Info);
                }
                catch (Exception ex)
                {
                    logger.WriteLine($@"Couldn't open {folderName}. ({ex.Message})", LoggerType.Error);
                }
            }
        }
    }
}