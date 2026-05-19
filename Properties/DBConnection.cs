using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;


namespace PracticaApp
{
    internal class DBConnection
    {
        public const string ConnectionString = "server=127.0.0.1; port=3306; user=root; password=slava2008; database=FitProDB;";
        static string DBConnect = ConnectionString;
        static public MySqlDataAdapter msDataAdapter;
        static MySqlConnection myconnect;
        static public MySqlCommand msCommand;


        public static bool ConnectionDB()
        {
            try
            {
                myconnect = new MySqlConnection(DBConnect);
                myconnect.Open();
                msCommand = new MySqlCommand();
                msCommand.Connection = myconnect;
                msDataAdapter = new MySqlDataAdapter(msCommand);
                return true;
            }
            catch
            {
                MessageBox.Show("Error connection with DataBase!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

           }
        public static void CloseDB()
        {
            myconnect.Close();
        }
        public static MySqlConnection GetConnection()
        { return myconnect; 
        
        }
        
        }
    }





            

