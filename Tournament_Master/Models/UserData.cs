using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;



namespace Tournament_Master.Models

{

    public class UserData

    {

        public string Login { get; set; }

        public string PasswordHash { get; set; }

        public string FirstName { get; set; }   // Ім'я

        public string LastName { get; set; }    // Прізвище

        public string Gender { get; set; }      // Стать

    }

}