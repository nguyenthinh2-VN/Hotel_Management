using MySql.Data.MySqlClient;

namespace HotelManagement.Database
{
    public class DatabaseConnection
    {
        private static string connectionString = "Server=localhost;Database=hotel_management;Uid=root;Pwd=123456;";
        private static DatabaseConnection? instance;
        private MySqlConnection? connection;

        private DatabaseConnection()
        {
            try
            {
                connection = new MySqlConnection(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to database: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static DatabaseConnection Instance
        {
            get
            {
                instance ??= new DatabaseConnection();
                return instance;
            }
        }

        public MySqlConnection GetConnection()
        {
            return connection ?? throw new Exception("Database connection not initialized");
        }

        public void OpenConnection()
        {
            try
            {
                if (connection?.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening connection: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (connection?.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing connection: {ex.Message}", "Database Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
