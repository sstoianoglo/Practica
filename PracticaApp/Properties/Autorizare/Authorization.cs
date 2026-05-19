using System;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace PracticaApp.Properties.Autorizare
{
    internal class Authorization
    {
        static public string? Role = null, Surname = null, User = null;

        static public string NormalizePhone(string phone)
        {
            StringBuilder normalizedPhone = new StringBuilder();

            foreach (char symbol in phone.Trim())
            {
                if (char.IsDigit(symbol))
                    normalizedPhone.Append(symbol);
            }

            return normalizedPhone.ToString();
        }

        static public void Authorization1(string login, string password)
        {
            try
            {
                string conString = "server=127.0.0.1;port=3306;user=root;password=slava2008;database=FitProDB;";

                using (MySqlConnection con = new MySqlConnection(conString))
                {
                    con.Open();

                    string query = @"
                        SELECT roles.RoleName
                        FROM accounts
                        INNER JOIN roles ON accounts.id_role = roles.id_role
                        WHERE accounts.Login = @login
                        AND accounts.Password = @password";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@login", login.Trim());
                        cmd.Parameters.AddWithValue("@password", password.Trim());

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            Role = result.ToString();
                            User = login.Trim();
                        }
                        else
                        {
                            Role = null;
                            User = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Role = null;
                User = null;
                MessageBox.Show(ex.Message, "Error connection with DataBase");
            }
        }

        static public string AuthorizationName(string login)
        {
            return login.Trim();
        }

        static public bool AuthorizationByGoogleEmail(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            if (normalizedEmail == "")
            {
                Role = null;
                User = null;
                return false;
            }

            try
            {
                using MySqlConnection con = new MySqlConnection(DBConnection.ConnectionString);
                con.Open();

                string query = @"
                    SELECT accounts.Login, roles.RoleName
                    FROM accounts
                    INNER JOIN roles ON accounts.id_role = roles.id_role
                    WHERE LOWER(accounts.Email) = @email
                    LIMIT 1";

                using MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@email", normalizedEmail);

                using MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string roleName = reader["RoleName"]?.ToString()?.Trim() ?? "";
                    string login = reader["Login"]?.ToString()?.Trim() ?? "";

                    if (roleName == "")
                    {
                        Role = null;
                        User = null;
                        return false;
                    }

                    Role = roleName;
                    User = login == "" ? normalizedEmail : login;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error connection with DataBase");
            }

            Role = null;
            User = null;
            return false;
        }

        static public bool AuthorizationByEmailAndPassword(string email, string password)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();
            string normalizedPassword = password.Trim();

            if (normalizedEmail == "" || normalizedPassword == "")
            {
                Role = null;
                User = null;
                return false;
            }

            try
            {
                using MySqlConnection con = new MySqlConnection(DBConnection.ConnectionString);
                con.Open();

                string query = @"
                    SELECT accounts.Login, roles.RoleName
                    FROM accounts
                    INNER JOIN roles ON accounts.id_role = roles.id_role
                    WHERE LOWER(accounts.Email) = @email
                    AND accounts.Password = @password
                    LIMIT 1";

                using MySqlCommand cmd = new MySqlCommand(query, con);
                cmd.Parameters.AddWithValue("@email", normalizedEmail);
                cmd.Parameters.AddWithValue("@password", normalizedPassword);

                using MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string roleName = reader["RoleName"]?.ToString()?.Trim() ?? "";
                    string login = reader["Login"]?.ToString()?.Trim() ?? "";

                    if (roleName == "")
                    {
                        Role = null;
                        User = null;
                        return false;
                    }

                    Role = roleName;
                    User = login == "" ? normalizedEmail : login;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error connection with DataBase");
            }

            Role = null;
            User = null;
            return false;
        }

        static public bool AccountExistsByEmail(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();

            if (normalizedEmail == "")
                return false;

            try
            {
                using MySqlConnection con = new MySqlConnection(DBConnection.ConnectionString);
                con.Open();

                using MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE LOWER(Email) = @email", con);
                command.Parameters.AddWithValue("@email", normalizedEmail);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error connection with DataBase");
                return false;
            }
        }

        static public bool CreateUserFromGoogleProfile(GoogleUserProfile profile, out string errorMessage)
        {
            errorMessage = "";
            string email = profile.Email.Trim();

            if (email == "")
            {
                errorMessage = "Google account does not contain an email address.";
                return false;
            }

            try
            {
                using MySqlConnection con = new MySqlConnection(DBConnection.ConnectionString);
                con.Open();

                using (MySqlCommand emailCheckCommand = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE LOWER(Email) = @email", con))
                {
                    emailCheckCommand.Parameters.AddWithValue("@email", email.ToLowerInvariant());

                    if (Convert.ToInt32(emailCheckCommand.ExecuteScalar()) > 0)
                    {
                        errorMessage = "Account already exists. Please log in.";
                        return false;
                    }
                }

                string login = CreateUniqueGoogleLogin(con, profile);

                string insertQuery = @"
                    INSERT INTO accounts
                    (Login, Password, Email, Phone, id_role)
                    VALUES
                    (@login, @password, @email, @phone, 2)";

                using MySqlCommand insertCommand = new MySqlCommand(insertQuery, con);
                insertCommand.Parameters.AddWithValue("@login", login);
                insertCommand.Parameters.AddWithValue("@password", "");
                insertCommand.Parameters.AddWithValue("@email", email);
                insertCommand.Parameters.AddWithValue("@phone", "");
                insertCommand.ExecuteNonQuery();

                Role = "user";
                User = login;
                return true;
            }
            catch (Exception ex)
            {
                Role = null;
                User = null;
                errorMessage = ex.Message;
                return false;
            }
        }

        static private string CreateUniqueGoogleLogin(MySqlConnection connection, GoogleUserProfile profile)
        {
            string baseLogin = profile.Name.Trim();

            if (baseLogin == "")
                baseLogin = profile.Email.Split('@')[0];

            string login = baseLogin;
            int suffix = 1;

            while (LoginExists(connection, login))
            {
                suffix++;
                login = $"{baseLogin}{suffix}";
            }

            return login;
        }

        static private bool LoginExists(MySqlConnection connection, string login)
        {
            using MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM accounts WHERE Login = @login", connection);
            command.Parameters.AddWithValue("@login", login);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        static public bool AuthorizationByUserPhone(string phone)
        {
            string normalizedPhone = NormalizePhone(phone);

            if (normalizedPhone == "")
            {
                Role = null;
                User = null;
                return false;
            }

            try
            {
                string conString = "server=127.0.0.1;port=3306;user=root;password=slava2008;database=FitProDB;";

                using (MySqlConnection con = new MySqlConnection(conString))
                {
                    con.Open();

                    string query = @"
                        SELECT accounts.Login, accounts.Phone, roles.RoleName
                        FROM accounts
                        INNER JOIN roles ON accounts.id_role = roles.id_role
                        WHERE LOWER(roles.RoleName) = 'user'";

                    using (MySqlCommand cmd = new MySqlCommand(query, con))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string accountPhone = reader["Phone"]?.ToString() ?? "";
                            string normalizedAccountPhone = NormalizePhone(accountPhone);

                            if (normalizedAccountPhone == normalizedPhone)
                            {
                                Role = reader["RoleName"]?.ToString();
                                User = reader["Login"]?.ToString();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error connection with DataBase");
            }

            Role = null;
            User = null;
            return false;
        }
    }
}
