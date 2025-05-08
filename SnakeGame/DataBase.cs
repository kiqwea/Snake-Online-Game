
using Npgsql;
using MySql.Data.MySqlClient;

namespace MyProject.DataBase{
    public class DBase
    {
        
        
        MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;username=root;password=zloy_malish;database=SnakeGame");
        
        public bool OpenConnection(){
            if(connection.State == System.Data.ConnectionState.Closed){
                connection.Open();
            }
            return true;
        }

        public bool CloseConnection(){
            if(connection.State == System.Data.ConnectionState.Open){
                connection.Close();
            }
            return true;
        }

        public MySqlConnection GetConnection(){
            return connection;
        }
    }
}
