using System;
using System.Linq;
using System.Windows;
using Tournament_Master.Views;

namespace Tournament_Master
{
    /// <summary>
    /// Головний клас додатка, що керує життєвим циклом програми, 
    /// процесом авторизації та динамічною зміною тем оформлення.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Створюємо вікно авторизації вручну
            AuthWindow authWindow = new AuthWindow();
            this.MainWindow = authWindow;
            authWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Гарантуємо збереження даних перед повним закриттям
            if (!string.IsNullOrEmpty(DataStorage.CurrentUser))
            {
                DataStorage.SaveAll();
            }
            base.OnExit(e);
        }

        /// <summary>
        /// Динамічно змінює тему оформлення додатка.
        /// </summary>
        /// <param name="themeName">Назва теми (наприклад, "LightTheme" або "DarkTheme").</param>
        public static void ChangeTheme(string themeName)
        {
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            // Шукаємо існуючу тему
            var oldTheme = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"/Resources/Themes/{themeName}.xaml", UriKind.Relative)
            };

            if (oldTheme != null)
            {
                int index = mergedDicts.IndexOf(oldTheme);
                mergedDicts[index] = newTheme; // Заміна
            }
            else
            {
                mergedDicts.Add(newTheme); // Додавання, якщо не було
            }
        }

        /// <summary>
        /// Динамічно змінює мову додатка.
        /// </summary>
        /// <param name="langCode">Код мови (наприклад, "uk-UA" або "en-US").</param>
        public static void ChangeLanguage(string langCode)
        {
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            // Шукаємо існуючу мову
            var oldLang = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Languages/"));

            var newLang = new ResourceDictionary
            {
                Source = new Uri($"/Resources/Languages/{langCode}.xaml", UriKind.Relative)
            };

            if (oldLang != null)
            {
                int index = mergedDicts.IndexOf(oldLang);
                mergedDicts[index] = newLang; // Заміна
            }
            else
            {
                mergedDicts.Add(newLang); // Додавання, якщо не було
            }
        }
    }
}