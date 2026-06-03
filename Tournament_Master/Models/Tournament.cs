using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Tournament_Master.Models
{
    public enum TournamentMode
    {
        None,
        Single, // Одиночний
        Team    // Командний
    }

    public class Tournament : INotifyPropertyChanged
    {
        private string _title;
        private bool _isEditing;
        private ObservableCollection<Participant> _participants = new ObservableCollection<Participant>();
        private ObservableCollection<Team> _teams = new ObservableCollection<Team>();
        private ObservableCollection<Match> _matches = new ObservableCollection<Match>();

        public string Creator { get; set; }
        public string Icon { get; set; }
        public string Date { get; set; }
        public string Info { get; set; }
        public string TournamentType { get; set; }

        // ВИПРАВЛЕНО: Додано властивість, якої не вистачало для DataStorage.cs
        public string StorageFilePath { get; set; }

        public TournamentMode Mode { get; set; } = TournamentMode.None;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Participant> Participants
        {
            get => _participants;
            set { _participants = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Team> Teams
        {
            get => _teams;
            set { _teams = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Match> Matches
        {
            get => _matches;
            set { _matches = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public bool IsEditing
        {
            get => _isEditing;
            set { _isEditing = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}