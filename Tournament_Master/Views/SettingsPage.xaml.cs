using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Tournament_Master.Views
{
    public partial class SettingsPage : Page
    {
        private List<string> _themes = new List<string> { "Світла / Light", "Темна / Dark" };
        private List<string> _languages = new List<string> { "Українська", "English" };
        private bool _isInitialized = false;

        public SettingsPage()
        {
            InitializeComponent();
            ThemeSelector.ItemsSource = _themes;
            LanguageSelector.ItemsSource = _languages;

            SetCurrentState();
            _isInitialized = true;
        }

        private void SetCurrentState()
        {
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            // Пошук теми без урахування регістру
            var currentTheme = mergedDicts.FirstOrDefault(d => d.Source != null &&
                d.Source.OriginalString.IndexOf("themes/", StringComparison.OrdinalIgnoreCase) >= 0);

            if (currentTheme != null)
            {
                ThemeSelector.SelectedIndex = currentTheme.Source.OriginalString.IndexOf("DarkTheme", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0;
            }

            // Пошук мови без урахування регістру
            var currentLang = mergedDicts.FirstOrDefault(d => d.Source != null &&
                d.Source.OriginalString.IndexOf("languages/", StringComparison.OrdinalIgnoreCase) >= 0);

            if (currentLang != null)
            {
                LanguageSelector.SelectedIndex = currentLang.Source.OriginalString.IndexOf("en-US", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0;
            }
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            // Передаємо чисту назву файлу (без шляхів), як того очікує App.ChangeTheme
            string themeName = ThemeSelector.SelectedIndex == 1 ? "DarkTheme" : "LightTheme";
            App.ChangeTheme(themeName);
        }

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            // Передаємо чистий код мови для App.ChangeLanguage
            string langCode = LanguageSelector.SelectedIndex == 1 ? "en-US" : "uk-UA";
            App.ChangeLanguage(langCode);
        }
    }
}