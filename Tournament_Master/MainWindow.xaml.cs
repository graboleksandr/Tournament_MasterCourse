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
            // Метод UpdateActiveButtonHighlight звідси можна прибрати, 
            // бо тепер усім керує подія MainFrame_Navigated
        }

        private void UpdateActiveButtonHighlight(Button activeButton)
        {
            var buttons = new[] { BtnHome, BtnParticipants, BtnTeams, BtnSchedule, BtnLeaderboard, BtnSettings, BtnArchive };

            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                if (btn == activeButton)
                {
                    btn.Background = Application.Current.Resources["HoverColor"] as Brush ?? new SolidColorBrush(Color.FromRgb(240, 240, 240));
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

        private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            var cb = sender as ComboBox;
            string path = cb.SelectedIndex == 1 ? "Resources/Languages/en-US.xaml" : "Resources/Languages/uk-UA.xaml";
            ApplyResource("Languages/", path);
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            var cb = sender as ComboBox;
            string path = cb.SelectedIndex == 1 ? "Resources/Themes/DarkTheme.xaml" : "Resources/Themes/LightTheme.xaml";
            ApplyResource("Themes/", path);
        }

        private void ApplyResource(string folderFilter, string fullPath)
        {
            try
            {
                var uri = new Uri(fullPath, UriKind.Relative);
                ResourceDictionary newDict = Application.LoadComponent(uri) as ResourceDictionary;
                var oldDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains(folderFilter));
                if (oldDict != null) Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                Application.Current.Resources.MergedDictionaries.Add(newDict);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Resource Error: {ex.Message}"); }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsLogoutAction) return;
            Application.Current.Shutdown();
        }
    }
}