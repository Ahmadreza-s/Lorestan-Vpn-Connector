using System.Collections.Generic;
using System.Linq;
using MahApps.Metro;

namespace LorestanVpnConnector
{
    public static class Skin
    {
        public static App App;
        public static string GetAccent() => Properties.Settings.Default.Accent;

        public static string GetTheme() => Properties.Settings.Default.Theme;

        public static void SetAccentColor(string color)
        {

            Properties.Settings.Default.Accent = color;
            Properties.Settings.Default.Save();
            ThemeManager.ChangeAppStyle(App, ThemeManager.GetAccent(color), ThemeManager.GetAppTheme(GetTheme()));
        }

        public static void SetTheme(string theme)
        {
            Properties.Settings.Default.Theme = theme;
            Properties.Settings.Default.Save();
            ThemeManager.ChangeAppStyle(App, ThemeManager.GetAccent(GetAccent()), ThemeManager.GetAppTheme(theme));
        }

        public static List<string> GetAllAccents() => ThemeManager.Accents.Select(c => c.Name).ToList();
        public static List<string> GetAllThemes() => ThemeManager.AppThemes.Select(c => c.Name).ToList();
    }
}
