using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Tournament_Master.Models;

namespace Tournament_Master.Services
{
    public static class FileService
    {
        private static readonly string FileName = "database.json";

        // ВИПРАВЛЕНО: Метод тепер повертає List<Participant>, усуваючи помилку CS0815
        public static List<Participant> ImportParticipantsFromTxt(string filePath)
        {
            var importedList = new List<Participant>();

            if (!File.Exists(filePath)) return importedList;

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Розділяємо рядок за пробілами або комами на Ім'я та Прізвище
                    var parts = line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                    {
                        importedList.Add(new Participant
                        {
                            FirstName = parts[0].Trim(),
                            LastName = parts[1].Trim()
                        });
                    }
                    else if (parts.Length == 1)
                    {
                        importedList.Add(new Participant
                        {
                            FirstName = parts[0].Trim(),
                            LastName = string.Empty
                        });
                    }
                }
                // Логіку збереження SaveData() видалено звідси, 
                // оскільки її тепер безпечно виконує сторінка після перевірки на дублікати.
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка при імпорті з TXT: {ex.Message}");
            }

            return importedList;
        }

        public static void SaveData()
        {
            try
            {
                var data = new
                {
                    Participants = DataStorage.AllParticipants,
                    Teams = DataStorage.AllTeams
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);

                File.WriteAllText(FileName, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка збереження: {ex.Message}");
            }
        }

        public static void LoadData()
        {
            if (!File.Exists(FileName)) return;

            try
            {
                string json = File.ReadAllText(FileName);
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (data != null)
                {
                    if (data.ContainsKey("Participants"))
                    {
                        var participants = JsonSerializer.Deserialize<List<Participant>>(data["Participants"].GetRawText());
                        DataStorage.AllParticipants.Clear();
                        foreach (var p in participants) DataStorage.AllParticipants.Add(p);
                    }

                    if (data.ContainsKey("Teams"))
                    {
                        var teams = JsonSerializer.Deserialize<List<Team>>(data["Teams"].GetRawText());
                        DataStorage.AllTeams.Clear();
                        foreach (var t in teams) DataStorage.AllTeams.Add(t);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Помилка завантаження: {ex.Message}");
            }
        }
    }
}