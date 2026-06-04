using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tournament_Master.Models;
using Tournament_Master.Views;

namespace Tournament_Master
{
    public partial class MainWindow : Window
    {
        // --- ПОЛЯ ДЛЯ ЗБЕРЕЖЕННЯ ЕКЗЕМПЛЯРІВ СТОРІНОК ---
        private HomePage _homePage;
        private ParticipantsPage _participantsPage;
        private SchedulePage _schedulePage;
        private SettingsPage _settingsPage;
        private TeamsPage _teamsPage;
        private ArchivePage _archivePage;
        private LeaderboardPage _leaderboardPage;

        private bool _isInitialized = false;
        public bool IsLogoutAction = false;

        public MainWindow()
        {
            InitializeComponent();

            DataStorage.ActiveTournament = null;
            RefreshPages();

            // ПІДПИСКА НА ПОДІЮ НАВІГАЦІЇ ФРЕЙМУ
            MainFrame.Navigated += MainFrame_Navigated;

            // Стартова сторінка
            NavigateToPage(_homePage, BtnHome);

            _isInitialized = true;
        }

        public void RefreshPages()
        {
            _homePage = new HomePage();
            _participantsPage = new ParticipantsPage();
            _schedulePage = new SchedulePage(DataStorage.ActiveTournament);
            _settingsPage = new SettingsPage();
            _teamsPage = new TeamsPage();
            _archivePage = new ArchivePage();
            _leaderboardPage = new LeaderboardPage();
        }

        // Автоматичне відстеження сторінки, яка завантажилась у фрейм
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content == null) return;

            // Визначаємо, яка сторінка зараз відкрилася, і підсвічуємо відповідну кнопку
            switch (e.Content)
            {
                case HomePage _:
                    UpdateActiveButtonHighlight(BtnHome);
                    break;
                case ParticipantsPage _:
                    UpdateActiveButtonHighlight(BtnParticipants);
                    break;
                case TeamsPage _:
                    UpdateActiveButtonHighlight(BtnTeams);
                    break;
                case SchedulePage _:
                    UpdateActiveButtonHighlight(BtnSchedule);
                    break;
                case LeaderboardPage _:
                    UpdateActiveButtonHighlight(BtnLeaderboard);
                    break;
                case ArchivePage _:
                    UpdateActiveButtonHighlight(BtnArchive);
                    break;
                case SettingsPage _:
                    UpdateActiveButtonHighlight(BtnSettings);
                    break;
            }
        }

        private void NavigateToPage(Page page, Button targetButton)
        {
            if (page == null) return;
            MainFrame.Navigate(page);
        }

        /// <summary>
        /// Керує підсвіткою активної кнопки у меню.
        /// </summary>
        private void UpdateActiveButtonHighlight(Button activeButton)
        {
            var buttons = new[] { BtnHome, BtnParticipants, BtnTeams, BtnSchedule, BtnLeaderboard, BtnSettings, BtnArchive };

            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                if (btn == activeButton)
                {
                    // ВИПРАВЛЕНО: Замість прямого присвоєння бруша, створюємо динамічне посилання.
                    // Тепер кнопка буде миттєво змінювати свій колір при зміні теми!
                    btn.SetResourceReference(Button.BackgroundProperty, "HoverColor");
                }
                else
                {
                    btn.Background = Brushes.Transparent;
                }
            }
        }

        // --- ОБРОБНИКИ КЛІКІВ МЕНЮ ---
        private void BtnHome_Click(object sender, RoutedEventArgs e) => NavigateToPage(_homePage, BtnHome);

        private void BtnParticipants_Click(object sender, RoutedEventArgs e)
        {
            _participantsPage = new ParticipantsPage();
            NavigateToPage(_participantsPage, BtnParticipants);
        }

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            _schedulePage = new SchedulePage(DataStorage.ActiveTournament);
            NavigateToPage(_schedulePage, BtnSchedule);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(_settingsPage, BtnSettings);

        private void BtnTeams_Click(object sender, RoutedEventArgs e)
        {
            _teamsPage = new TeamsPage();
            NavigateToPage(_teamsPage, BtnTeams);
        }

        private void BtnArchive_Click(object sender, RoutedEventArgs e)
        {
            _archivePage = new ArchivePage();
            NavigateToPage(_archivePage, BtnArchive);
        }

        private void BtnNavigateLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            var leaderboard = new LeaderboardPage();
            NavigateToPage(leaderboard, BtnLeaderboard);
        }

        public void StartNewTournament()
        {
            DataStorage.ActiveTournament = null;
            DataStorage.AllParticipants.Clear();
            DataStorage.AllMatches.Clear();

            RefreshPages();
            NavigateToPage(_participantsPage, BtnParticipants);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try { DataStorage.SaveAll(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error during auto-save: {ex.Message}"); }
            Application.Current.Shutdown();
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        // --- ВИПРАВЛЕНО: Селектори (якщо вони є на MainWindow) тепер викликають централізовані методи App ---
        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || !(sender is ComboBox cb)) return;
            string langCode = cb.SelectedIndex == 1 ? "en-US" : "uk-UA";
            App.ChangeLanguage(langCode);
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || !(sender is ComboBox cb)) return;
            string themeName = cb.SelectedIndex == 1 ? "DarkTheme" : "LightTheme";
            App.ChangeTheme(themeName);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsLogoutAction) return;
            Application.Current.Shutdown();
        }
    }
}