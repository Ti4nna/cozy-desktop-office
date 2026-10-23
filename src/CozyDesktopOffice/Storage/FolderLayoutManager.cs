using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CozyDesktopOffice
{
    public class RoomPreferences
    {
        public string LightingMode { get; set; } = "Automatic";
        public string PetChoice { get; set; } = "None";
        public string DeskColor { get; set; } = "Blush";
        public bool LaunchAtStartupEnabled { get; set; }
        public List<string> DrawerCategories { get; set; } = new()
        {
            "School",
            "Personal",
            "Content Creation",
            "Important Documents"
        };
    }

    public class FolderLayoutManager
    {
        public const string BaseFolderName = "CozyDesktopOffice";
        public const string BookshelfFolderName = "Bookshelf";
        public const string DeskFolderName = "Desk";
        public const string BulletinBoardFolderName = "BulletinBoard";
        public const string DrawersFolderName = "Drawers";
        public const string SettingsFolderName = "settings";

        public static readonly string[] DefaultDrawerCategories =
        {
            "School",
            "Personal",
            "Content Creation",
            "Important Documents"
        };

        public string BasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), BaseFolderName);

        public void EnsureStructure()
        {
            var root = BasePath;
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(Path.Combine(root, BookshelfFolderName));
            Directory.CreateDirectory(Path.Combine(root, DeskFolderName));
            Directory.CreateDirectory(Path.Combine(root, BulletinBoardFolderName));
            Directory.CreateDirectory(Path.Combine(root, DrawersFolderName));
            Directory.CreateDirectory(Path.Combine(root, SettingsFolderName));

            EnsureDefaultDrawerFolders();
        }

        public void EnsureDefaultDrawerFolders()
        {
            var drawerRoot = Path.Combine(BasePath, DrawersFolderName);
            Directory.CreateDirectory(drawerRoot);

            foreach (var category in DefaultDrawerCategories)
            {
                Directory.CreateDirectory(Path.Combine(drawerRoot, category));
            }
        }

        public List<string> GetDrawerCategories()
        {
            EnsureStructure();

            var categories = DefaultDrawerCategories.ToList();
            var drawerRoot = Path.Combine(BasePath, DrawersFolderName);

            if (!Directory.Exists(drawerRoot))
            {
                return categories;
            }

            foreach (var dir in Directory.GetDirectories(drawerRoot))
            {
                var name = Path.GetFileName(dir);
                if (!string.IsNullOrWhiteSpace(name) && !categories.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    categories.Add(name);
                }
            }

            var prefs = LoadPreferences();
            if (prefs != null && prefs.DrawerCategories != null)
            {
                foreach (var custom in prefs.DrawerCategories)
                {
                    if (!string.IsNullOrWhiteSpace(custom) && !categories.Contains(custom, StringComparer.OrdinalIgnoreCase))
                    {
                        categories.Add(custom);
                        Directory.CreateDirectory(Path.Combine(drawerRoot, custom));
                    }
                }
            }

            return categories;
        }

        public string GetDrawerPath(string category)
        {
            var safeName = string.IsNullOrWhiteSpace(category) ? "Custom" : category.Trim();
            var root = Path.Combine(BasePath, DrawersFolderName);
            Directory.CreateDirectory(root);
            var folder = Path.Combine(root, safeName);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public void OpenCozyFolders()
        {
            EnsureStructure();

            foreach (var folder in new[]
                     {
                         BasePath,
                         Path.Combine(BasePath, BookshelfFolderName),
                         Path.Combine(BasePath, DeskFolderName),
                         Path.Combine(BasePath, BulletinBoardFolderName),
                         Path.Combine(BasePath, DrawersFolderName),
                         Path.Combine(BasePath, SettingsFolderName)
                     })
            {
                if (Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folder,
                        UseShellExecute = true
                    });
                }
            }
        }

        public string CopySafely(string sourceFilePath, string destinationFolderName)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentException("Source file path is empty.", nameof(sourceFilePath));
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("The source file does not exist.", sourceFilePath);
            }

            EnsureStructure();

            var folderPath = Path.Combine(BasePath, destinationFolderName);
            Directory.CreateDirectory(folderPath);

            var fileName = Path.GetFileName(sourceFilePath);
            var destinationPath = Path.Combine(folderPath, fileName);
            var extension = Path.GetExtension(fileName);
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var index = 1;

            while (File.Exists(destinationPath))
            {
                destinationPath = Path.Combine(folderPath, $"{baseName} ({index}){extension}");
                index++;
            }

            File.Copy(sourceFilePath, destinationPath, overwrite: false);
            return destinationPath;
        }

        public List<StoredFileItem> ReadCopiedFiles(string folderName)
        {
            var folderPath = Path.Combine(BasePath, folderName);
            if (!Directory.Exists(folderPath))
            {
                return new List<StoredFileItem>();
            }

            return Directory.GetFiles(folderPath)
                .OrderBy(file => file)
                .Select(file => new StoredFileItem
                {
                    FileName = Path.GetFileName(file),
                    Path = file
                })
                .ToList();
        }

        public RoomPreferences? LoadPreferences()
        {
            EnsureStructure();
            var settingsFile = Path.Combine(BasePath, SettingsFolderName, "preferences.json");

            if (!File.Exists(settingsFile))
            {
                return new RoomPreferences();
            }

            var json = File.ReadAllText(settingsFile);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new RoomPreferences();
            }

            try
            {
                return JsonSerializer.Deserialize<RoomPreferences>(json) ?? new RoomPreferences();
            }
            catch
            {
                return new RoomPreferences();
            }
        }

        public void SavePreferences(RoomPreferences preferences)
        {
            EnsureStructure();
            var settingsFolder = Path.Combine(BasePath, SettingsFolderName);
            Directory.CreateDirectory(settingsFolder);

            var settingsFile = Path.Combine(settingsFolder, "preferences.json");
            var json = JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsFile, json);
        }

        public void AddCustomDrawer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var prefs = LoadPreferences() ?? new RoomPreferences();
            var normalized = name.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            if (prefs.DrawerCategories == null)
            {
                prefs.DrawerCategories = new List<string>();
            }

            if (!prefs.DrawerCategories.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                prefs.DrawerCategories.Add(normalized);
            }

            GetDrawerPath(normalized);
            SavePreferences(prefs);
        }
    }
}
