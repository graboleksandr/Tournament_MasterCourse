using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Tournament_Master.Views
{
    /// <summary>
    /// Сторінка налаштувань додатка.
    /// Забезпечує керування візуальними темами та мовними пакетами через ResourceDictionaries.
    /// </summary>
    public partial class SettingsPage : Page
    {
        /// <summary> Список доступних тем оформлення. </summary>
        private List<string> _themes = new List<string> { "Світла / Light", "Темна / Dark" };

        /// <summary> Список доступних мов інтерфейсу. </summary>
        private List<string> _languages = new List<string> { "Українська", "English" };

        /// <summary> Прапор, що вказує, чи завершено ініціалізацію компонентів (запобігає спрацюванню подій при завантаженні). </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Ініціалізує новий екземпляр сторінки <see cref="SettingsPage"/>.
        /// Налаштовує випадаючі списки та встановлює поточні значення на основі активних ресурсів.
        /// </summary>
        public SettingsPage()
        {
            InitializeComponent();
            ThemeSelector.ItemsSource = _themes;
            LanguageSelector.ItemsSource = _languages;

            // Визначаємо поточний стан на основі вже завантажених словників
            SetCurrentState();

            _isInitialized = true;
        }

        /// <summary>
        /// Аналізує <see cref="Application.Current.Resources.MergedDictionaries"/> для визначення 
        /// поточної теми та мови, щоб відобразити їх у селекторах (ComboBox).
        /// </summary>
        private void SetCurrentState()
        {
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            // Пошук словника теми
            var currentTheme = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));
            if (currentTheme != null)
                ThemeSelector.SelectedIndex = currentTheme.Source.OriginalString.Contains("DarkTheme") ? 1 : 0;

            // Пошук словника локалізації
            var currentLang = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Languages/"));
            if (currentLang != null)
                LanguageSelector.SelectedIndex = currentLang.Source.OriginalString.Contains("en-US") ? 1 : 0;
        }

        /// <summary>
        /// Обробник зміни теми. Викликає метод застосування ресурсу для вибраного файлу XAML.
        /// </summary>
        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            string themePath = ThemeSelector.SelectedIndex == 1
                ? "Resources/Themes/DarkTheme.xaml"
                : "Resources/Themes/LightTheme.xaml";

            ApplyResource("Themes/", themePath);
        }

        /// <summary>
        /// Обробник зміни мови. Викликає метод застосування ресурсу для вибраного файлу локалізації.
        /// </summary>
        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            string langPath = LanguageSelector.SelectedIndex == 1
                ? "Resources/Languages/en-US.xaml"
                : "Resources/Languages/uk-UA.xaml";

            ApplyResource("Languages/", langPath);
        }

        /// <summary>
        /// Універсальний метод для заміни або додавання словників ресурсів у глобальну колекцію додатка.
        /// </summary>
        /// <param name="folderFilter">Ключове слово в шляху (напр. "Themes/"), за яким шукається старий словник для заміни.</param>
        /// <param name="fullPath">Відносний шлях до нового XAML-файлу ресурсів.</param>
        private void ApplyResource(string folderFilter, string fullPath)
        {
            try
            {
                var uri = new Uri(fullPath, UriKind.Relative);
                ResourceDictionary newDict = Application.LoadComponent(uri) as ResourceDictionary;

                // Отримуємо доступ до глобальних MergedDictionaries додатка
                var mergedDicts = Application.Current.Resources.MergedDictionaries;

                // Шукаємо існуючий словник даного типу (тема або мова)
                var oldDict = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains(folderFilter));

                if (oldDict != null)
                {
                    // Замінюємо існуючий словник новим за тим самим індексом для збереження пріоритетів
                    int index = mergedDicts.IndexOf(oldDict);
                    mergedDicts[index] = newDict;
                }
                else
                {
                    // Якщо словника такого типу ще немає — просто додаємо його
                    mergedDicts.Add(newDict);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження ресурсів: {ex.Message}");
            }
        }
    }
}