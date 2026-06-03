using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tournament_Master.Models;

namespace Tournament_Master.Views
{
    public partial class ParticipantsPage : Page
    {
        private Tournament _currentTournament;

        public ParticipantsPage() : this(DataStorage.ActiveTournament)
        {
        }

        public ParticipantsPage(Tournament tournament)
        {
            InitializeComponent();

            _currentTournament = tournament ?? DataStorage.ActiveTournament;

            if (_currentTournament != null)
            {
                if (_currentTournament.Participants == null)
                    _currentTournament.Participants = new ObservableCollection<Participant>();

                if (_currentTournament.Teams == null)
                    _currentTournament.Teams = new ObservableCollection<Team>();

                DataStorage.AllParticipants = _currentTournament.Participants;
                DataStorage.AllTeams = _currentTournament.Teams;
            }

            RefreshUI();
        }

        private void RefreshUI()
        {
            if (ParticipantsCards == null) return;

            ObservableCollection<Participant> workingList = DataStorage.AllParticipants;
            if (workingList == null) return;

            string filter = TxtSearch?.Text?.ToLower() ?? "";

            var filteredList = string.IsNullOrEmpty(filter)
                ? workingList.ToList()
                : workingList.Where(p => p.FirstName.ToLower().Contains(filter) || p.LastName.ToLower().Contains(filter)).ToList();

            if (CmbSort != null)
            {
                switch (CmbSort.SelectedIndex)
                {
                    case 1:
                        filteredList = filteredList.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
                        break;
                    case 2:
                        filteredList = filteredList.OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ToList();
                        break;
                }
            }

            ParticipantsCards.ItemsSource = null;
            ParticipantsCards.ItemsSource = filteredList;

            if (LblStats != null && RunCount != null)
            {
                RunCount.Text = $" {filteredList.Count}";
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Participant participant)
            {
                DataStorage.AllParticipants.Remove(participant);
                DataStorage.SaveAll();
                RefreshUI();
                ShowMainStatus("Учасника успішно видалено", isError: false);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshUI();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshUI();
        }

        private void BtnToSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (DataStorage.AllParticipants == null || DataStorage.AllParticipants.Count < 2)
            {
                ShowMainStatus("Додайте хоча б 2 учасників перед тим, як генерувати розклад!", isError: true);
                return;
            }

            if (DataStorage.IsTeamMode)
            {
                var teamsPage = new TeamsPage();
                this.NavigationService.Navigate(teamsPage);
            }
            else
            {
                var schedulePage = new SchedulePage(_currentTournament);
                this.NavigationService.Navigate(schedulePage);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*",
                Title = "Оберіть текстовий файл з учасниками"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string selectedFilePath = openFileDialog.FileName;
                    var importedParticipants = Tournament_Master.Services.FileService.ImportParticipantsFromTxt(selectedFilePath);

                    if (importedParticipants == null || importedParticipants.Count == 0)
                    {
                        ShowMainStatus("Файл порожній або не містить коректного тексту.", isError: true);
                        return;
                    }

                    int addedCount = 0;
                    foreach (var participant in importedParticipants)
                    {
                        bool exists = DataStorage.AllParticipants.Any(p =>
                            p.FirstName.Equals(participant.FirstName, StringComparison.OrdinalIgnoreCase) &&
                            p.LastName.Equals(participant.LastName, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                        {
                            DataStorage.AllParticipants.Add(participant);
                            addedCount++;
                        }
                    }

                    if (addedCount > 0)
                    {
                        DataStorage.SaveAll();
                        RefreshUI();
                    }

                    ShowMainStatus($"Імпорт завершено! Додано нових гравців: {addedCount}", isError: false);
                }
                catch (Exception ex)
                {
                    ShowMainStatus($"Помилка імпорту: {ex.Message}", isError: true);
                }
            }
        }

        private void BtnOpenPanel_Click(object sender, RoutedEventArgs e)
        {
            if (TxtPanelError != null) TxtPanelError.Visibility = Visibility.Collapsed;

            DoubleAnimation anim = new DoubleAnimation(300, TimeSpan.FromMilliseconds(300));
            AddPanel.BeginAnimation(WidthProperty, anim);
            TxtFirstName.Focus();
        }

        private void BtnClosePanel_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            AddPanel.BeginAnimation(WidthProperty, anim);
        }

        private void BtnSaveNewParticipant_Click(object sender, RoutedEventArgs e)
        {
            // 1. Скидаємо попередній стан помилки
            TxtPanelError.Visibility = Visibility.Collapsed;
            TxtPanelError.Text = string.Empty;

            // Очищаємо текст від випадкових пробілів
            string firstName = TxtFirstName.Text?.Trim() ?? string.Empty;
            string lastName = TxtLastName.Text?.Trim() ?? string.Empty;

            // 2. Валідація: порожні поля
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                ShowValidationError("m_ErrRequiredFields");
                return;
            }

            // 3. Валідація: мінімальна довжина
            if (firstName.Length < 2 || lastName.Length < 2)
            {
                ShowValidationError("m_ErrTooShort");
                return;
            }

            // 4. Валідація: дозволені літери та знаки
            var nameRegex = new Regex(@"^[A-Za-zА-Яа-яЁёЇїІіЄєҐґ'\s-]+$");
            if (!nameRegex.IsMatch(firstName) || !nameRegex.IsMatch(lastName))
            {
                ShowValidationError("m_ErrInvalidChars");
                return;
            }

            // ==========================================
            // ЗБЕРЕЖЕННЯ
            // ==========================================
            var newParticipant = new Participant
            {
                FirstName = firstName,
                LastName = lastName
            };

            DataStorage.AllParticipants.Add(newParticipant);
            DataStorage.SaveAll();
            RefreshUI();

            // Показуємо статус успіху вгорі головного екрана
            ShowMainStatus("Учасника успішно додано", isError: false);

            // ==========================================
            // UX ПОКРАЩЕННЯ ДЛЯ ШВИДКОГО ДОДАВАННЯ
            // ==========================================
            // 5. Очищаємо текстові поля для нового введення
            TxtFirstName.Text = string.Empty;
            TxtLastName.Text = string.Empty;

            // Переносимо фокус (курсор) назад на поле імені, 
            // щоб можна було відразу вводити наступного гравця з клавіатури
            TxtFirstName.Focus();

            // Рядок BtnClosePanel_Click(sender, e); ВИДАЛЕНО, 
            // тому панель більше не закривається сама!
        }

        /// <summary>
        /// Допоміжний метод для виведення помилки на панель з урахуванням локалізації
        /// </summary>
        private void ShowValidationError(string resourceKey)
        {
            // Витягуємо рядок зі словника ресурсів, який активний в даний момент
            if (TryFindResource(resourceKey) is string localizedError)
            {
                TxtPanelError.Text = localizedError;
            }
            else
            {
                TxtPanelError.Text = "Validation Error!"; // Фолбек, якщо ключ не знайдено
            }

            TxtPanelError.Visibility = Visibility.Visible;
        }

        // МЕТОД ДЛЯ СУЧАСНОГО ВИВЕДЕННЯ ПОВІДОМЛЕНЬ НА ЕКРАН З ЕФЕКТОМ ЗГАСАННЯ
        private void ShowMainStatus(string message, bool isError)
        {
            if (TxtMainStatus == null) return;

            TxtMainStatus.Text = message;

            // Підбираємо колір: червоний для помилок, зелений для успіху
            TxtMainStatus.Foreground = isError
                ? new SolidColorBrush(Color.FromRgb(211, 47, 47))  // #D32F2F
                : new SolidColorBrush(Color.FromRgb(56, 142, 60));  // #388E3C

            TxtMainStatus.Visibility = Visibility.Visible;
            TxtMainStatus.BeginAnimation(OpacityProperty, null); // Скидаємо стару анімацію, якщо вона активна
            TxtMainStatus.Opacity = 1.0;

            // Створюємо плавне зникнення через 4 секунди відображення
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(500),
                BeginTime = TimeSpan.FromSeconds(4)
            };
            fadeOut.Completed += (s, e) => TxtMainStatus.Visibility = Visibility.Collapsed;
            TxtMainStatus.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}