using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CozyDesktopOffice
{
    public partial class MainWindow : Window
    {
        private readonly FolderLayoutManager _folderLayoutManager = new();
        private readonly DesktopIntegration _desktopIntegration = new();
        private bool _desktopEnabled;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;

            // Targeted drop handlers for the visible room surfaces.
            BookshelfTarget.Drop += BookshelfDrop;
            BoardTarget.Drop += BulletinBoardDrop;
            DrawerUnit.Drop += DrawerDrop;

            // The whole scene also accepts drops as a fallback.
            AddHandler(DragEnterEvent, new DragEventHandler(MainWindow_DragEnter), true);
            AddHandler(DropEvent, new DragEventHandler(MainWindow_Drop), true);

            LightingModeCombo.SelectionChanged += LightingModeCombo_SelectionChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _folderLayoutManager.EnsureStructure();
            LoadSettings();
            RefreshLighting();
            RenderBooks();
            RenderDeskItems();
            RenderBoardItems();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_desktopEnabled)
            {
                Left = 0;
                Top = 0;
                Width = SystemParameters.VirtualScreenWidth;
                Height = SystemParameters.VirtualScreenHeight;
            }
        }

        private void ToggleDesktopButton_Click(object sender, RoutedEventArgs e)
        {
            _desktopEnabled = !_desktopEnabled;

            if (_desktopEnabled)
            {
                if (_desktopIntegration.TryAttachToDesktop(this))
                {
                    WindowStyle = WindowStyle.None;
                    AllowsTransparency = true;
                    Topmost = true;
                    Width = SystemParameters.VirtualScreenWidth;
                    Height = SystemParameters.VirtualScreenHeight;
                    Left = 0;
                    Top = 0;
                    ShowInTaskbar = false;
                    ShowActivated = false;
                    ToggleDesktopButton.Content = "Exit Desktop Mode";
                    StatusText.Text = "Desktop mode enabled. The app is behind the desktop icons.";
                }
                else
                {
                    _desktopEnabled = false;
                    StatusText.Text = "Desktop mode could not attach to the shell. The normal window remains available.";
                    MessageBox.Show(
                        "Desktop mode could not attach to the Windows desktop shell on this PC. This is a normal fallback and the app will continue in its regular window.",
                        "Desktop mode fallback",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                _desktopIntegration.DetachFromDesktop(this);
                WindowStyle = WindowStyle.SingleBorderWindow;
                AllowsTransparency = false;
                Topmost = false;
                Width = 1600;
                Height = 900;
                Left = (SystemParameters.WorkArea.Width - Width) / 2;
                Top = (SystemParameters.WorkArea.Height - Height) / 2;
                ShowInTaskbar = true;
                ToggleDesktopButton.Content = "Enable Desktop Mode";
                StatusText.Text = "Desktop mode disabled.";
            }
        }

        private void OpenFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            _folderLayoutManager.OpenCozyFolders();
            StatusText.Text = "Opened CozyDesktopOffice folders.";
        }

        private void AddPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*",
                Multiselect = true,
                Title = "Choose photos for the bulletin board"
            };

            if (dialog.ShowDialog() == true)
            {
                foreach (var file in dialog.FileNames)
                {
                    SaveBoardPhoto(file);
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
        }

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            foreach (var file in files)
            {
                HandleSafeCopy(file, FolderLayoutManager.DeskFolderName, "Desk");
            }
        }

        private void BookshelfDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            foreach (var file in files)
            {
                HandleSafeCopy(file, FolderLayoutManager.BookshelfFolderName, "Bookshelf");
            }
        }

        private void BulletinBoardDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            foreach (var file in files)
            {
                SaveBoardPhoto(file);
            }
        }

        private void DrawerDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            foreach (var file in files)
            {
                HandleSafeCopy(file, FolderLayoutManager.DrawersFolderName, "Drawers");
            }
        }

        private void LoadSettings()
        {
            var prefs = _folderLayoutManager.LoadPreferences();
            if (prefs == null)
            {
                return;
            }

            LightingModeCombo.SelectedIndex = prefs.LightingMode switch
            {
                "Day" => 1,
                "Sunset" => 2,
                "Night" => 3,
                _ => 0
            };
        }

        private void LightingModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LightingModeCombo.SelectedIndex < 0)
            {
                return;
            }

            var text = ((ComboBoxItem)LightingModeCombo.SelectedItem)?.Content?.ToString() ?? "Automatic";
            var mode = text switch
            {
                "Day" => "Day",
                "Sunset" => "Sunset",
                "Night" => "Night",
                _ => "Automatic"
            };

            var prefs = _folderLayoutManager.LoadPreferences() ?? new RoomPreferences();
            prefs.LightingMode = mode;
            _folderLayoutManager.SavePreferences(prefs);
            RefreshLighting();
        }

        private void RefreshLighting()
        {
            var prefs = _folderLayoutManager.LoadPreferences() ?? new RoomPreferences();
            var effectiveMode = prefs.LightingMode;
            if (effectiveMode == "Automatic")
            {
                var hour = DateTime.Now.Hour;
                if (hour >= 18 || hour < 7)
                {
                    effectiveMode = "Night";
                }
                else if (hour >= 16 && hour < 18)
                {
                    effectiveMode = "Sunset";
                }
                else
                {
                    effectiveMode = "Day";
                }
            }

            var wallColor = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x22, 0x27, 0x36),
                "Sunset" => Color.FromRgb(0xF8, 0xD8, 0xD6),
                _ => Color.FromRgb(0xF7, 0xE8, 0xF1)
            };

            var windowSky = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x1A, 0x2E, 0x59),
                "Sunset" => Color.FromRgb(0xF4, 0xB6, 0x7C),
                _ => Color.FromRgb(0xB7, 0xD8, 0xF2)
            };

            var floorColor = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x4C, 0x38, 0x2D),
                "Sunset" => Color.FromRgb(0xD9, 0xA3, 0x77),
                _ => Color.FromRgb(0xD6, 0xB1, 0x9B)
            };

            var deskColor = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x87, 0x5C, 0x47),
                "Sunset" => Color.FromRgb(0xB9, 0x7E, 0x5C),
                _ => Color.FromRgb(0xB9, 0x84, 0x60)
            };

            var lampColor = effectiveMode switch
            {
                "Night" => Color.FromRgb(0xFF, 0xE2, 0x8B),
                _ => Color.FromRgb(0xF2, 0xC9, 0xB9)
            };

            WallPanel.Background = new SolidColorBrush(wallColor);
            FloorPanel.Background = new SolidColorBrush(floorColor);
            WindowGlass.Fill = new SolidColorBrush(windowSky);
            DeskTop.Fill = new SolidColorBrush(deskColor);
            DeskFront.Fill = new SolidColorBrush(Color.FromRgb(0x8E, 0x5E, 0x49));
            LampShade.Fill = new SolidColorBrush(lampColor);
            MusicStation.Fill = effectiveMode == "Night" ? new SolidColorBrush(Color.FromRgb(0xF8, 0xD8, 0xEB)) : new SolidColorBrush(Color.FromRgb(0xF1, 0xC7, 0xDA));
            StatusText.Text = $"Lighting: {effectiveMode}";
        }

        private void RenderBooks()
        {
            ShelfObjectsCanvas.Children.Clear();
            var files = _folderLayoutManager.ReadCopiedFiles(FolderLayoutManager.BookshelfFolderName);
            double x = 0;
            double y = 0;

            foreach (var file in files)
            {
                var tile = new Border
                {
                    Width = 72,
                    Height = 110,
                    Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xD9, 0xDA)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xB6, 0x7D, 0x85)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(4),
                    Tag = file.Path
                };

                var title = Path.GetFileNameWithoutExtension(file.FileName);
                var label = new TextBlock
                {
                    Text = title.Length > 12 ? title.Substring(0, 11) + "..." : title,
                    Foreground = new SolidColorBrush(Colors.DarkSlateGray),
                    FontWeight = FontWeights.Bold,
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 60,
                    TextAlignment = TextAlignment.Center
                };

                tile.Child = label;
                Canvas.SetLeft(tile, x);
                Canvas.SetTop(tile, y);
                tile.MouseLeftButtonDown += Book_Click;
                ShelfObjectsCanvas.Children.Add(tile);

                x += 84;
                if (x > 280)
                {
                    x = 0;
                    y += 95;
                }
            }
        }

        private void RenderDeskItems()
        {
            DeskObjectsCanvas.Children.Clear();
            var files = _folderLayoutManager.ReadCopiedFiles(FolderLayoutManager.DeskFolderName);
            double x = 0;
            double y = 0;

            foreach (var file in files)
            {
                var item = new Border
                {
                    Width = 120,
                    Height = 62,
                    Background = new SolidColorBrush(Color.FromRgb(0xF9, 0xF0, 0xF2)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0x88, 0x96)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Tag = file.Path
                };

                var text = new TextBlock
                {
                    Text = GetShortLabel(file.FileName),
                    Foreground = new SolidColorBrush(Colors.Black),
                    FontSize = 11,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 100,
                    Margin = new Thickness(8),
                    TextAlignment = TextAlignment.Center
                };

                item.Child = text;
                Canvas.SetLeft(item, x);
                Canvas.SetTop(item, y);
                item.MouseLeftButtonDown += DeskItem_Click;
                DeskObjectsCanvas.Children.Add(item);

                x += 140;
                if (x > 650)
                {
                    x = 0;
                    y += 80;
                }
            }
        }

        private void RenderBoardItems()
        {
            BoardObjectsCanvas.Children.Clear();
            var files = _folderLayoutManager.ReadCopiedFiles(FolderLayoutManager.BulletinBoardFolderName);
            int count = 0;

            foreach (var file in files)
            {
                var lower = file.Path.ToLowerInvariant();
                if (!lower.EndsWith(".jpg") && !lower.EndsWith(".jpeg") && !lower.EndsWith(".png") && !lower.EndsWith(".bmp") && !lower.EndsWith(".gif"))
                {
                    continue;
                }

                var image = new Image
                {
                    Source = new BitmapImage(new Uri(file.Path, UriKind.Absolute)),
                    Width = 120,
                    Height = 120,
                    Stretch = Stretch.UniformToFill,
                    Margin = new Thickness(10),
                    Tag = file.Path
                };

                BoardObjectsCanvas.Children.Add(image);
                Canvas.SetLeft(image, (count % 3) * 130 + 10);
                Canvas.SetTop(image, (count / 3) * 130 + 10);
                count++;
            }
        }

        private void Book_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string path)
            {
                OpenPath(path, "Opened file from the bookshelf.");
            }
        }

        private void DeskItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string path)
            {
                OpenPath(path, "Opened desk file.");
            }
        }

        private void OpenPath(string path, string successText)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                StatusText.Text = successText;
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not open the file: " + ex.Message;
            }
        }

        private void HandleSafeCopy(string sourcePath, string folderName, string label)
        {
            if (!File.Exists(sourcePath))
            {
                StatusText.Text = "The dropped file no longer exists.";
                return;
            }

            try
            {
                var destination = _folderLayoutManager.CopySafely(sourcePath, folderName);
                StatusText.Text = $"Copied to {label}: {destination}";

                if (folderName == FolderLayoutManager.BookshelfFolderName)
                {
                    RenderBooks();
                }
                else if (folderName == FolderLayoutManager.DeskFolderName)
                {
                    RenderDeskItems();
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not copy the file: " + ex.Message;
            }
        }

        private void SaveBoardPhoto(string sourcePath)
        {
            if (!File.Exists(sourcePath))
            {
                StatusText.Text = "The selected photo does not exist.";
                return;
            }

            try
            {
                var destination = _folderLayoutManager.CopySafely(sourcePath, FolderLayoutManager.BulletinBoardFolderName);
                RenderBoardItems();
                StatusText.Text = "Saved bulletin board photo: " + destination;
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not save the photo: " + ex.Message;
            }
        }

        private static string GetShortLabel(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            return name.Length > 18 ? name.Substring(0, 15) + "..." : name;
        }
    }

    public class StoredFileItem
    {
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }

    public class StoredPhoto
    {
        public string Path { get; set; } = string.Empty;
    }

    public class RoomPreferences
    {
        public string LightingMode { get; set; } = "Automatic";
        public string PetChoice { get; set; } = "None";
        public string DeskColor { get; set; } = "Blush";
    }
}
