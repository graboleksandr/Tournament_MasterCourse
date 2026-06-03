using System;

namespace Tournament_Master.Models
{
    public class Match
    {
        public string Round { get; set; }
        public string Team1 { get; set; } // Назва команди або FullName гравця
        public string Team2 { get; set; } // Назва команди або FullName гравця

        // ВИПРАВЛЕНО: Зроблено тип int? (nullable), щоб підтримувати незіграні матчі зі значенням null
        public int? Score1 { get; set; }
        public int? Score2 { get; set; }

        /// <summary>
        /// Показує, чи внесено фінальний рахунок матчу.
        /// Захищає від того, щоб незіграні матчі рахувалися як нічия 0:0.
        /// </summary>
        public bool IsFinished { get; set; }

        /// <summary>
        /// Гарне відображення результату в DataGrid.
        /// Якщо матч не зіграно — виведе "- : -", якщо зіграно — реальний рахунок.
        /// </summary>
        public string Result => IsFinished ? $"{Score1} : {Score2}" : "- : -";
    }
}