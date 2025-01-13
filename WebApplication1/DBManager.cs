using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.Sqlite;

public class DBManager{
    private SqliteConnection? connection = null;

    private string HashPassword(string password){
        using (var algoritm = SHA256.Create()){
            var bytes_hash = algoritm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }
    public bool ConnectToBD(string path){
        Console.WriteLine("Connection to DB");

        try
        {
            connection = new SqliteConnection("Data Source=" + path);
            connection.Open();

            if (connection.State != System.Data.ConnectionState.Open){
                Console.WriteLine("Failed");
                return false;
            }
            Console.WriteLine("Done!");
            return true;
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public void Disconnect(){
        if (null == connection)
            return;
        
        if (connection.State != System.Data.ConnectionState.Open)
            return;

        connection.Close();

        Console.WriteLine("Disconnect from DB");
    }

    public bool CheckConnection(){
        if (null == connection)
            return false;

        if (connection.State != System.Data.ConnectionState.Open)
            return false;

        return true;
    }

    public bool AddUser(string login, string password){
        try{
            if (CheckConnection()){
                string Request = "SELECT Login FROM users WHERE Login='" + login + "'";
                var command = new SqliteCommand(Request, connection);
                var reader = command.ExecuteReader();

                int result = 0;
                if (reader.HasRows)
                    result = 1;
                else
                    result = 0;
                
                if (result == 0){
                    Request = "INSERT INTO users (Login, Password) VALUES ('" + login + "', '" + HashPassword(password) + "')";
                    command = new SqliteCommand(Request, connection);
                    result = command.ExecuteNonQuery();

                    if (1 == result)
                        return true;
                    else
                        return false;
                }
                else{
                    System.Console.WriteLine("Program has had user with login like you yet");
                    return false;
                }
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool CheckUser(string login, string password){
        try{
            if (CheckConnection()){
                string Request = "SELECT Login,Password FROM users WHERE Login='" + login + "' AND Password='" + HashPassword(password) + "'";
                var command = new SqliteCommand(Request, connection);

                var reader = command.ExecuteReader();

                if (reader.HasRows)
                        return true;
                    else
                        return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool CreateTable(string login){
        try{
            if (CheckConnection()){
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE " + login +"(Last_Name TEXT NOT NULL, First_Name TEXT NOT NULL, Telephon TEXT NOT NULL UNIQUE, Email TEXT, Country TEXT, Role TEXT)";
                int result = command.ExecuteNonQuery();
                if (result >= 1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool CreateHistoryTable(string login){
        try{
            if (CheckConnection()){
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "CREATE TABLE History" + login +"(Action TEXT NOT NULL)";
                int result = command.ExecuteNonQuery();
                if (result >= 1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool WriteAction(string login, string action){
        try{
            if (CheckConnection()){
                string Request = "INSERT INTO History" + login + " (Action) VALUES ('" + action + "')";
                var command = new SqliteCommand(Request, connection);

                int result_1 = command.ExecuteNonQuery();
                if (1 == result_1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool AddContact(Person person, string login){
        try{
            if (CheckConnection()){
                string Request = "INSERT INTO " + login + " (Last_Name, First_Name, Telephon, Email, Country, Role) VALUES ('" + person.Last_name + "', '" + person.First_name + "', '" + person.Telephon + "', '" + person.Email + "', '" + person.Country + "', '" + person.Role + "')";
                var command = new SqliteCommand(Request, connection);

                int result_1 = command.ExecuteNonQuery();

                if (1 == result_1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public List<Person> CheckContact(string login){
        List<Person> list = new List<Person>();
        try{
            if (CheckConnection()){
                string Request = "SELECT * FROM " + login;
                var command = new SqliteCommand(Request, connection);
                
                var reader = command.ExecuteReader();

                if (reader.HasRows) // если есть данные
                {
                    while (reader.Read()) // построчно считываем данные
                    {
                        Person men = new Person();
                        object ln = reader.GetValue(0);
                        men.Last_name = (string)ln;
                        object fn = reader.GetValue(1);
                        men.First_name = (string)fn;
                        object tel = reader.GetValue(2);
                        men.Telephon = (string)tel;
                        object em = reader.GetValue(3);
                        men.Email = (string)em;
                        object con = reader.GetValue(4);
                        men.Country = (string)con;
                        object ro = reader.GetValue(5);
                        men.Role = (string)ro;
                        list.Add(men);
                    }
                }
                else
                    return list;
                
                return list;
            }
            else{
                Console.WriteLine("Failed");
                return list;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return list;
        }
    }

    public List<string> CheckHistory(string login){
        List<string> list = new List<string>();
        try{
            if (CheckConnection()){
                string Request = "SELECT * FROM History" + login;
                var command = new SqliteCommand(Request, connection);

                var reader = command.ExecuteReader();

                if (reader.HasRows) // если есть данные
                {
                    while (reader.Read()) // построчно считываем данные
                    {
                        object action = reader.GetValue(0);
                        list.Add((string)action);
                    }
                }
                else
                    return list;

                return list;
            }
            else{
                Console.WriteLine("Failed");
                return list;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return list;
        }
    }

    public bool UpdateContact(string login, NewPerson person){
        try{
            if (CheckConnection()){
                int result = 0;
                string Request = "";
                if (person.New_Last_name != " " && person.New_Last_name != ""){
                    if (person.Telephon == "" || person.Telephon == " ")
                        Request = "UPDATE " + login + " SET Last_Name = '" + person.New_Last_name + "' WHERE Last_Name = '" + person.Last_name + "'";
                    else
                        Request = "UPDATE " + login + " SET Last_Name = '" + person.New_Last_name + "' WHERE Last_Name = '" + person.Last_name + "' AND Telephon = '" + person.Telephon + "'";
                    var command = new SqliteCommand(Request, connection);
                    result += command.ExecuteNonQuery();
                }
                if (person.New_First_name != " " && person.New_First_name != ""){
                    if (person.Telephon == "" || person.Telephon == " ")
                        Request = "UPDATE " + login + " SET First_Name = '" + person.New_First_name + "' WHERE Last_Name = '" + person.Last_name + "'";
                    else
                        Request = "UPDATE " + login + " SET First_Name = '" + person.New_First_name + "' WHERE Last_Name = '" + person.Last_name + "' AND Telephon = '" + person.Telephon + "'";
                    var command = new SqliteCommand(Request, connection);
                    result += command.ExecuteNonQuery();
                }
                if (person.New_Telephon != " " && person.New_Telephon != ""){
                    if (person.Telephon == "" || person.Telephon == " ")
                        Request = "UPDATE " + login + " SET Telephon = '" + person.New_Telephon + "' WHERE Last_Name = '" + person.Last_name + "'";
                    else
                        Request = "UPDATE " + login + " SET Telephon = '" + person.New_Telephon + "' WHERE Last_Name = '" + person.Last_name + "' AND Telephon = '" + person.Telephon + "'";
                    var command = new SqliteCommand(Request, connection);
                    result += command.ExecuteNonQuery();
                }
                if (person.New_Email != " " && person.New_Email != ""){
                    if (person.Telephon == "" || person.Telephon == " ")
                        Request = "UPDATE " + login + " SET Email = '" + person.New_Email + "' WHERE Last_Name = '" + person.Last_name + "'";
                    else
                        Request = "UPDATE " + login + " SET Email = '" + person.New_Email + "' WHERE Last_Name = '" + person.Last_name + "' AND Telephon = '" + person.Telephon + "'";
                    var command = new SqliteCommand(Request, connection);
                    result += command.ExecuteNonQuery();
                }
                if (person.New_Country != " " && person.New_Country != ""){
                    if (person.Telephon == "" || person.Telephon == " ")
                        Request = "UPDATE " + login + " SET Country = '" + person.New_Country + "' WHERE Last_Name = '" + person.Last_name + "'";
                    else
                        Request = "UPDATE " + login + " SET Country = '" + person.New_Country + "' WHERE Last_Name = '" + person.Last_name + "' AND Telephon = '" + person.Telephon + "'";
                    var command = new SqliteCommand(Request, connection);
                    result += command.ExecuteNonQuery();
                }
                if (person.New_Role != " " && person.New_Role != ""){
                    if (person.Telephon == "" || person.Telephon == " ")
                        Request = "UPDATE " + login + " SET Role = '" + person.New_Role + "' WHERE Last_Name = '" + person.Last_name + "'";
                    else
                        Request = "UPDATE " + login + " SET Role = '" + person.New_Role + "' WHERE Last_Name = '" + person.Last_name + "' AND Telephon = '" + person.Telephon + "'";
                    var command = new SqliteCommand(Request, connection);
                    result += command.ExecuteNonQuery();
                }
                if (result == 0){
                    return false;
                }
                return true;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool DeleteContact(string login, string surname){
        try{
            if (CheckConnection()){
                string Request = "DELETE FROM " + login + " WHERE Last_Name = '" + surname + "'";
                var command = new SqliteCommand(Request, connection);
                int result = command.ExecuteNonQuery();
                if (result == 1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool DeleteContactTel(string login, string phone){
        try{
            if (CheckConnection()){
                string Request = "DELETE FROM " + login + " WHERE Telephon = '" + phone + "'";
                var command = new SqliteCommand(Request, connection);
                int result = command.ExecuteNonQuery();
                if (result == 1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }

    public bool DeleteHistory (string login){
        try{
            if (CheckConnection()){
                string Request = "DELETE FROM History" + login;
                var command = new SqliteCommand(Request, connection);
                int result = command.ExecuteNonQuery();
                if (result >= 1)
                    return true;
                else
                    return false;
            }
            else{
                Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
        
    }

    public bool ChangePassword(string login, string pass){
        try{
            if (CheckConnection()){
                string Request = "UPDATE users SET Password = '" + HashPassword(pass) + "' WHERE Login = '" + login + "'";
                var command = new SqliteCommand(Request, connection);
                int result = command.ExecuteNonQuery();
                if (result == 1)
                    return true;
                else
                    return false;
            }
            else{
                System.Console.WriteLine("Failed");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
    }
}