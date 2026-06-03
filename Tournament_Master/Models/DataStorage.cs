using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Linq;
using Tournament_Master.Models;

namespace Tournament_Master
{
    public static class DataStorage
    {
        public static ObservableCollection<Participant> AllParticipants { get; set; } = new ObservableCollection<Participant>();
        public static ObservableCollection<Team> AllTeams { get; set; } = new ObservableCollection<Team>();
        public static ObservableCollection<Match> AllMatches { get; set; } = new ObservableCollection<Match>();
        public static ObservableCollection<Tournament> SavedTournaments { get; set; } = new ObservableCollection<Tournament>();

        public static string CurrentUser { get; set; }
        public static string CurrentTournamentName { get; set; } = "Мій Турнір";
        public static bool IsTeamMode { get; set; } = false;
        public static Tournament ActiveTournament { get; set; } = null;

        public static void ClearData()
        {
            AllParticipants.Clear();
            AllTeams.Clear();
            AllMatches.Clear();
            SavedTournaments.Clear();
            CurrentUser = null;
            CurrentTournamentName = "Мій Турнір";
            IsTeamMode = false;
            ActiveTournament = null;
        }

        public static void SetupNewTournamentSession(Tournament tournament)
        {
            ActiveTournament = tournament;
            CurrentTournamentName = tournament.Title;
            IsTeamMode = (tournament.Mode == TournamentMode.Team);

            AllParticipants.Clear();
            AllTeams.Clear();
            AllMatches.Clear();

            ActiveTournament.Participants = AllParticipants;
            ActiveTournament.Teams = AllTeams;
            ActiveTournament.Matches = AllMatches;

            if (!SavedTournaments.Any(t => t.Title == tournament.Title && t.Date == tournament.Date))
            {
                SavedTournaments.Add(tournament);
            }
            SaveAll();
        }

        public static void LoadSelectedTournamentSession(Tournament tournament)
        {
            if (tournament == null) return;

            ActiveTournament = tournament;
            CurrentTournamentName = tournament.Title;
            IsTeamMode = (tournament.Mode == TournamentMode.Team);

            AllParticipants.Clear();
            foreach (var p in tournament.Participants ?? new ObservableCollection<Participant>()) AllParticipants.Add(p);

            AllTeams.Clear();
            foreach (var t in tournament.Teams ?? new ObservableCollection<Team>()) AllTeams.Add(t);

            AllMatches.Clear();
            foreach (var m in tournament.Matches ?? new ObservableCollection<Match>()) AllMatches.Add(m);
        }

        public static string GenerateRandomTeams(int value, bool isByTeamsCount)
        {
            if (AllParticipants == null || AllParticipants.Count < 2)
                return "Необхідно мати хоча б 2 учасників!";

            if (value <= 0) return "Вкажіть коректне числове значение!";

            var shuffledPlayers = AllParticipants.OrderBy(p => Guid.NewGuid()).ToList();
            AllTeams.Clear();

            int targetTeamsCount = 0;

            if (isByTeamsCount)
            {
                targetTeamsCount = value;
                if (shuffledPlayers.Count < targetTeamsCount)
                    return $"Учасників ({shuffledPlayers.Count}) менше, ніж необхідна кількість команд ({targetTeamsCount})!";
            }
            else
            {
                int playersPerTeam = value;
                if (playersPerTeam > shuffledPlayers.Count)
                    return "Розмір команди не може бути більшим за загальну кількість учасників!";

                targetTeamsCount = shuffledPlayers.Count / playersPerTeam;
                if (targetTeamsCount == 0) targetTeamsCount = 1;
            }

            for (int i = 0; i < targetTeamsCount; i++)
            {
                AllTeams.Add(new Team
                {
                    TeamName = $"Команда {i + 1}",
                    Members = new List<Participant>()
                });
            }

            for (int i = 0; i < shuffledPlayers.Count; i++)
            {
                AllTeams[i % targetTeamsCount].Members.Add(shuffledPlayers[i]);
            }

            SaveAll();
            return "SUCCESS";
        }

        public static string GenerateMatchSchedule()
        {
            var competitors = IsTeamMode
                ? AllTeams.Select(t => t.TeamName).ToList()
                : AllParticipants.Select(p => p.FullName).ToList();

            if (competitors.Count < 2)
                return "Необхідно мінімум 2 учасники/команди для створення сітки матчів!";

            AllMatches.Clear();

            if (competitors.Count % 2 != 0)
            {
                competitors.Add("Вільний раунд");
            }

            int numCompetitors = competitors.Count;
            int numRounds = numCompetitors - 1;
            int matchesPerRound = numCompetitors / 2;

            for (int round = 0; round < numRounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    int home = (round + match) % (numCompetitors - 1);
                    int away = (numCompetitors - 1 - match + round) % (numCompetitors - 1);

                    if (match == 0) away = numCompetitors - 1;

                    string comp1 = competitors[home];
                    string comp2 = competitors[away];

                    if (comp1 == "Вільний раунд" || comp2 == "Вільний раунд") continue;

                    AllMatches.Add(new Match
                    {
                        Round = $"Раунд {round + 1}",
                        Team1 = comp1,
                        Team2 = comp2,
                        Score1 = 0,
                        Score2 = 0
                    });
                }
            }

            SaveAll();
            return "SUCCESS";
        }

        private static string GetUserFolderPath()
        {
            string user = string.IsNullOrEmpty(CurrentUser) ? "Guest" : CurrentUser;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersData", user);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        // --- РОЗУМНЕ ЗБЕРЕЖЕННЯ З ПІДТРИМКОЮ АДМІНІСТРАТОРА ---
        public static void SaveAll()
        {
            if (string.IsNullOrEmpty(CurrentUser)) CurrentUser = "Guest";

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string folder = GetUserFolderPath();

                if (ActiveTournament != null)
                {
                    ActiveTournament.Title = CurrentTournamentName;

                    if (string.IsNullOrEmpty(ActiveTournament.Creator))
                        ActiveTournament.Creator = CurrentUser;

                    ActiveTournament.Participants = new ObservableCollection<Participant>(AllParticipants);
                    ActiveTournament.Teams = new ObservableCollection<Team>(AllTeams);
                    ActiveTournament.Matches = new ObservableCollection<Match>(AllMatches);

                    var existing = SavedTournaments.FirstOrDefault(t => t.Title == ActiveTournament.Title && t.Date == ActiveTournament.Date);
                    if (existing != null)
                    {
                        int index = SavedTournaments.IndexOf(existing);
                        SavedTournaments[index] = ActiveTournament;
                    }
                    else
                    {
                        SavedTournaments.Add(ActiveTournament);
                    }
                }

                // Гарантуємо, що кожен турнір має прописаний шлях перед збереженням
                foreach (var t in SavedTournaments)
                {
                    if (string.IsNullOrEmpty(t.StorageFilePath))
                    {
                        string creator = string.IsNullOrEmpty(t.Creator) ? CurrentUser : t.Creator;
                        t.StorageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersData", creator, "tournaments.json");
                    }
                }

                // Розподіляємо збереження залежно від ролі
                if (CurrentUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    // Групуємо всі поточні турніри за їхніми рідними файлами
                    var groups = SavedTournaments.GroupBy(t => t.StorageFilePath);
                    var savedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var group in groups)
                    {
                        string dir = Path.GetDirectoryName(group.Key);
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        File.WriteAllText(group.Key, JsonSerializer.Serialize(group.ToList(), options));
                        savedPaths.Add(group.Key);
                    }

                    // Перевіряємо, чи адмін видалив останні турніри якихось користувачів (щоб очистити їхні файли)
                    string rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersData");
                    if (Directory.Exists(rootDir))
                    {
                        foreach (var uFolder in Directory.GetDirectories(rootDir))
                        {
                            string tPath = Path.Combine(uFolder, "tournaments.json");
                            if (File.Exists(tPath) && !savedPaths.Contains(tPath))
                            {
                                // Якщо у списку більше немає турнірів для цього файлу — очищаємо його
                                File.WriteAllText(tPath, JsonSerializer.Serialize(new List<Tournament>(), options));
                            }
                        }
                    }
                }
                else
                {
                    // Звичайний користувач записує дані виключно у свій файл
                    string tPath = Path.Combine(folder, "tournaments.json");
                    File.WriteAllText(tPath, JsonSerializer.Serialize(SavedTournaments, options));
                }

                // Запис активної робочої сесії поточного користувача
                File.WriteAllText(Path.Combine(folder, "participants.json"), JsonSerializer.Serialize(AllParticipants, options));
                File.WriteAllText(Path.Combine(folder, "teams.json"), JsonSerializer.Serialize(AllTeams, options));
                File.WriteAllText(Path.Combine(folder, "matches.json"), JsonSerializer.Serialize(AllMatches, options));

                var settings = new Dictionary<string, string>
                {
                    { "TournamentName", CurrentTournamentName },
                    { "IsTeamMode", IsTeamMode.ToString() }
                };
                File.WriteAllText(Path.Combine(folder, "settings.json"), JsonSerializer.Serialize(settings, options));
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        // --- РОЗУМНЕ ЗАВАНТАЖЕННЯ З ПІДТРИМКОЮ АДМІНІСТРАТОРА ---
        public static void LoadAll()
        {
            if (string.IsNullOrEmpty(CurrentUser)) CurrentUser = "Guest";

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                string folder = GetUserFolderPath();

                AllParticipants.Clear();
                AllTeams.Clear();
                AllMatches.Clear();
                SavedTournaments.Clear();

                if (CurrentUser.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    string rootDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UsersData");
                    if (Directory.Exists(rootDir))
                    {
                        foreach (var uFolder in Directory.GetDirectories(rootDir))
                        {
                            string tPath = Path.Combine(uFolder, "tournaments.json");
                            if (File.Exists(tPath))
                            {
                                var data = JsonSerializer.Deserialize<ObservableCollection<Tournament>>(File.ReadAllText(tPath), options);
                                if (data != null)
                                {
                                    foreach (var item in data)
                                    {
                                        if (!SavedTournaments.Any(x => x.Title == item.Title && x.Date == item.Date))
                                        {
                                            item.StorageFilePath = tPath; // Запам'ятовуємо джерело файлу
                                            SavedTournaments.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    string tPath = Path.Combine(folder, "tournaments.json");
                    if (File.Exists(tPath))
                    {
                        var data = JsonSerializer.Deserialize<ObservableCollection<Tournament>>(File.ReadAllText(tPath), options);
                        if (data != null)
                        {
                            foreach (var item in data)
                            {
                                item.StorageFilePath = tPath;
                                SavedTournaments.Add(item);
                            }
                        }
                    }
                }

                // Завантаження поточних сесійних файлів з папки поточного користувача
                string pPath = Path.Combine(folder, "participants.json");
                if (File.Exists(pPath))
                {
                    var data = JsonSerializer.Deserialize<ObservableCollection<Participant>>(File.ReadAllText(pPath), options);
                    if (data != null) foreach (var item in data) AllParticipants.Add(item);
                }

                string teamPath = Path.Combine(folder, "teams.json");
                if (File.Exists(teamPath))
                {
                    var data = JsonSerializer.Deserialize<ObservableCollection<Team>>(File.ReadAllText(teamPath), options);
                    if (data != null) foreach (var item in data) AllTeams.Add(item);
                }

                string mPath = Path.Combine(folder, "matches.json");
                if (File.Exists(mPath))
                {
                    var data = JsonSerializer.Deserialize<ObservableCollection<Match>>(File.ReadAllText(mPath), options);
                    if (data != null) foreach (var item in data) AllMatches.Add(item);
                }

                string sPath = Path.Combine(folder, "settings.json");
                if (File.Exists(sPath))
                {
                    var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(sPath), options);
                    if (settings != null)
                    {
                        if (settings.TryGetValue("TournamentName", out string name))
                            CurrentTournamentName = name;

                        if (settings.TryGetValue("IsTeamMode", out string teamMode) && bool.TryParse(teamMode, out bool isTeam))
                            IsTeamMode = isTeam;
                    }
                }

                if (SavedTournaments.Count > 0 && ActiveTournament == null)
                {
                    ActiveTournament = SavedTournaments.LastOrDefault();
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}