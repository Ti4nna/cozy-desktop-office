using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CozyDesktopOffice
{
    public partial class MainWindow : Window
    {
        private readonly FolderLayoutManager _folderLayoutManager = new();
        private readonly DesktopIntegration _desktopIntegration = new();
        private bool _desktopEnabled;
        private readonly ObservableCollection<StoredFileItem> _books = new();
        private readonly ObservableCollection<StoredFileItem> _deskItems = new();
        private readonly ObservableCollection<StoredPhoto> _boardPhotos = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
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
                    StatusText.Text = "Desktop mode enabled. Icons remain active behind the room.";
                }
                else
                {
                    _desktopEnabled = false;
                    StatusText.Text = "Desktop mode could not attach to the desktop shell. The normal app window remains available.";
                    MessageBox.Show("Desktop mode failed. The app is still usable in its normal window, but the desktop wallpaper host could not be attached on this machine.", "Desktop mode fallback", MessageBoxButton.OK, MessageBoxImage.Information);
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
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files|*.*",
                Multiselect = true,
                Title = "Select photos to add to your bulletin board"
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
                AddDroppedFile(file, "Desk");
            }
        }

        private void LoadSettings()
        {
            var prefs = _folderLayoutManager.LoadPreferences();
            if (prefs == null)
            {
                return;
            }

            switch (prefs.LightingMode)
            {
                case "Day": LightingModeCombo.SelectedIndex = 1; break;
                case "Sunset": LightingModeCombo.SelectedIndex = 2; break;
                case "Night": LightingModeCombo.SelectedIndex = 3; break;
                default: LightingModeCombo.SelectedIndex = 0; break;
            }

            LightingModeCombo.SelectionChanged += LightingModeCombo_SelectionChanged;
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
                "Night" => Color.FromRgb(0x1F, 0x22, 0x34),
                "Sunset" => Color.FromRgb(0xF8, 0xD7, 0xD8),
                _ => Color.FromRgb(0xFCE, 0xAE, 0xEF)
            };

            var windowSky = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x1C, 0x2D, 0x54),
                "Sunset" => Color.FromRgb(0xF6, 0xBE, 0x8A),
                _ => Color.FromRgb(0xB9, 0xD9, 0xF0)
            };

            var floor = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x5A, 0x3C, 0x2F),
                "Sunset" => Color.FromRgb(0xD3, 0x9E, 0x72),
                _ => Color.FromRgb(0xD7, 0xB4, 0x9A)
            };

            var desk = effectiveMode switch
            {
                "Night" => Color.FromRgb(0x8A, 0x61, 0x46),
                "Sunset" => Color.FromRgb(0xB8, 0x76, 0x52),
                _ => Color.FromRgb(0xB6, 0x7F, 0x5B)
            };

            var lamp = effectiveMode switch
            {
                "Night" => Color.FromRgb(0xFF, 0xE0, 0x7A),
                _ => Color.FromRgb(0xF5, 0xC4, 0xB7)
            };

            WallPanel.Background = new SolidColorBrush(wallColor);
            FloorPanel.Background = new SolidColorBrush(floor);
            WindowGlass.Fill = new SolidColorBrush(windowSky);
            WindowFrame.Background = new SolidColorBrush(Color.FromRgb(0xF7, 0xE8, 0xD9));
            DeskTop.Fill = new SolidColorBrush(desk);
            DeskFront.Fill = new SolidColorBrush(Color.FromRgb(0x8F, 0x60, 0x48));
            LampShade.Fill = new SolidColorBrush(lamp);
            MusicStation.Fill = effectiveMode == "Night" ? new SolidColorBrush(Color.FromRgb(0xF8, 0xD6, 0xE7)) : new SolidColorBrush(Color.FromRgb(0xF2, 0xCA, 0xDA));
            StatusText.Text = $"Lighting: {effectiveMode}";
        }

        private void RenderBooks()
        {
            ShelfObjectsCanvas.Children.Clear();
            var shelfEntries = _folderLayoutManager.ReadCopiedFiles(FolderLayoutManager.BookshelfFolderName);
            double x = 0;
            double y = 0;
            foreach (var book in shelfEntries)
            {
                var border = new Border
                {
                    Width = 70,
                    Height = 110,
                    Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xD7, 0xDA)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xAE, 0x7C, 0x7C)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(4),
                    Tag = book.Path
                };

                var label = new TextBlock
                {
                    Text = Path.GetFileNameWithoutExtension(book.FileName).Length > 12
                        ? Path.GetFileNameWithoutExtension(book.FileName).Substring(0, 10) + "..."
                        : Path.GetFileNameWithoutExtension(book.FileName),
                    Foreground = new SolidColorBrush(Colors.DarkSlateGray),
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = 60,
                    FontSize = 11
                };

                border.Child = label;
                Canvas.SetLeft(border, x);
                Canvas.SetTop(border, y);
                border.MouseLeftButtonDown += Book_Click;
                ShelfObjectsCanvas.Children.Add(border);

                x += 80;
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
                var block = new Border
                {
                    Width = 120,
                    Height = 60,
                    Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xEDE, 0xF0)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xC7, 0x84, 0x94)),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(8),
                    Tag = file.Path
                };

                var text = new TextBlock
                {
                    Text = Path.GetFileNameWithoutExtension(file.FileName).Length > 18 ? Path.GetFileNameWithoutExtension(file.FileName).Substring(0, 16) + "..." : Path.GetFileNameWithoutExtension(file.FileName),
                    Foreground = new SolidColorBrush(Colors.Black),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 11,
                    TextAlignment = TextAlignment.Center,
                    Width = 100,
                    Margin = new Thickness(8)
                };

                block.Child = text;
                Canvas.SetLeft(block, x);
                Canvas.SetTop(block, y);
                block.MouseLeftButtonDown += DeskItem_Click;
                DeskObjectsCanvas.Children.Add(block);

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
            foreach (var file in files)
            {
                if (file.Path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || file.Path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                    || file.Path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || file.Path.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                {
                    var image = new Image
                    {
                        Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(file.Path, UriKind.Absolute)),
                        Width = 120,
                        Height = 120,
                        Stretch = Stretch.UniformToFill,
                        Tag = file.Path,
                        Margin = new Thickness(12)
                    };

                    BoardObjectsCanvas.Children.Add(image);
                    Canvas.SetLeft(image, (BoardObjectsCanvas.Children.Count % 3) * 120 + 10);
                    Canvas.SetTop(image, (BoardObjectsCanvas.Children.Count / 3) * 110 + 10);
                }
            }
        }

        private void Book_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string path)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
                    StatusText.Text = "Opened file from the bookshelf.";
                }
                catch (Exception ex)
                {
                    StatusText.Text = "Could not open the file: " + ex.Message;
                }
            }
        }

        private void DeskItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string path)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
                    StatusText.Text = "Opened desk file.";
                }
                catch (Exception ex)
                {
                    StatusText.Text = "Could not open the file: " + ex.Message;
                }
            }
        }

        private void AddDroppedFile(string sourcePath, string targetFolder)
        {
            if (!File.Exists(sourcePath))
            {
                StatusText.Text = "The dropped file no longer exists.";
                return;
            }

            try
            {
                var destination = _folderLayoutManager.CopySafely(sourcePath, targetFolder);
                StatusText.Text = "Copied to: " + destination;

                if (targetFolder == FolderLayoutManager.BookshelfFolderName)
                {
                    RenderBooks();
                }
                else if (targetFolder == FolderLayoutManager.DeskFolderName)
                {
                    RenderDeskItems();
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not copy file: " + ex.Message;
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
                StatusText.Text = "Photo saved to the bulletin board folder: " + destination;
            }
            catch (Exception ex)
            {
                StatusText.Text = "Could not save the photo: " + ex.Message;
            }
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
        public string DeskTheme { get; set; } = "Blush";
        public string PetChoice { get; set; } = "None";
    }
}
