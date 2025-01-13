using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

CookieContainer cookies = new CookieContainer();
HttpClientHandler handler = new HttpClientHandler();
HttpClient client = new HttpClient(handler);
handler.CookieContainer = cookies;

string login = "";

bool LoginOnServer(string? username, string? password){
    if (username == null || username.Length == 0 || password == null || password.Length == 0)
        {return false;}
    
    string request = "login?login=" + username + "&password=" + password;
    var responce = client.PostAsync(request, null).Result;
    if (responce.IsSuccessStatusCode){
        System.Console.WriteLine("You are logged in");
        IEnumerable<Cookie> responce_Cookies = cookies.GetAllCookies();
        foreach (Cookie cookie in responce_Cookies){
            System.Console.WriteLine(cookie.Name + ": " + cookie.Value);
        }
        login += username;
        return true;
    }
    else{
        System.Console.WriteLine("There isn't this account in database");
        return false;
    }
}

bool SignUpOnServer(string? username, string? password){
    if (username == null || username.Length == 0 || password == null || password.Length == 0)
        {return false;}

    string request = "signup?login=" + username + "&password=" + password;
    var responce = client.PostAsync(request, null).Result;
    if (responce.IsSuccessStatusCode){
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        return true;
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        return false;
    }
}

bool AddContactInHandbook(string last_Name, string first_Name, string telephon, string email, string country, string role){
    string request = "/add_contact";

    var json_data = new {
        Last_name = last_Name,
        First_name = first_Name,
        Telephon = telephon,
        Email = email,
        Country = country,
        Role = role
    };

    string json_Body = JsonSerializer.Serialize(json_data);
    var content = new StringContent(json_Body, Encoding.UTF8, "application/json");

    var responce = client.PostAsync(request, content).Result;
    if (responce.IsSuccessStatusCode){
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        return true;
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        return false;
    }
}

List<Person> Deserialize(string request){
    string fs = "", ls = "", t = "", e = "", c = "", r = "";
    List<Person> answer = new List<Person>();
    Person moment = new Person();
    for (int i = 0; i < request.Length; i++){
        if (request[i] == '{'){
            i += 15;
            while (request[i] != '\"'){
                fs += request[i];
                i++;
            }
            moment.First_name = fs;
            i += 15;
            while (request[i] != '\"'){
                ls += request[i];
                i++;
            }
            moment.Last_name = ls;
            i += 14;
            while (request[i] != '\"'){
                t += request[i];
                i++;
            }
            moment.Telephon = t;
            i += 11;
            while (request[i] != '\"'){
                e += request[i];
                i++;
            }
            moment.Email = e;
            i += 13;
            while (request[i] != '\"'){
                c += request[i];
                i++;
            }
            moment.Country = c;
            i += 10;
            while (request[i] != '\"'){
                r += request[i];
                i++;
            }
            moment.Role = r;
            answer.Add(moment);
            moment = new Person();
            fs = ""; ls = ""; t = ""; e = ""; c = ""; r = "";
        }
    }
    return answer;
}

void CheckAllContactsInHandbook (){
    string request = "/check_contacts";

    var responce = client.GetAsync(request).Result;
    if (responce.IsSuccessStatusCode){
        var content = responce.Content.ReadAsStringAsync().Result;
        List<Person> pokaz = Deserialize(content);
        System.Console.WriteLine("   Last_Name\t First_name\t Telephon\t Email\t\t Country\t Role");
        for (int i = 0; i < pokaz.Count; i++){
            System.Console.WriteLine($"{i+1}  {pokaz[i].Last_name}\t {pokaz[i].First_name}\t\t {pokaz[i].Telephon}\t {pokaz[i].Email}\t\t {pokaz[i].Country}\t\t {pokaz[i].Role}");
        }
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
    }
}

void CheckContactInHandbook(string ln){
    string request = "/check_contact?surname=" + ln;

    var responce = client.GetAsync(request).Result;
    if (responce.IsSuccessStatusCode){
        var content = responce.Content.ReadAsStringAsync().Result;
        List<Person> pokaz = Deserialize(content);
        if (pokaz.Count == 1){
            System.Console.WriteLine("Last_Name\t First_name\t Telephon\t Email\t Country\t Role");
            System.Console.WriteLine($"{pokaz[0].Last_name}\t {pokaz[0].First_name}\t {pokaz[0].Telephon}\t {pokaz[0].Email}\t {pokaz[0].Country}\t {pokaz[0].Role}");
        }
        else{
            System.Console.WriteLine($"Found {pokaz.Count} contacts with this surname");
            System.Console.WriteLine("   Last_Name\t First_name\t Telephon\t Email\t\t Country\t Role");
            for (int i = 0; i < pokaz.Count; i++){
                System.Console.WriteLine($"{i+1}  {pokaz[i].Last_name}\t {pokaz[i].First_name}\t\t {pokaz[i].Telephon}\t {pokaz[i].Email}\t\t {pokaz[i].Country}\t\t {pokaz[i].Role}");
            }
            System.Console.WriteLine();
            System.Console.WriteLine("Enter the number of the contact you want to view: ");
            string choice = Console.ReadLine();
            int a = 0;
            if (int.TryParse(choice, out a)){
                if (a < 0 || a > pokaz.Count){
                    System.Console.WriteLine("Invalid enter");
                }
                else{
                    System.Console.WriteLine("Last_Name\t First_name\t Telephon\t Email\t\t Country\t Role");
                    System.Console.WriteLine($"{pokaz[a-1].Last_name}\t {pokaz[a-1].First_name}\t\t {pokaz[a-1].Telephon}\t {pokaz[a-1].Email}\t\t {pokaz[a-1].Country}\t\t {pokaz[a-1].Role}");
                }
            }
            else{
                System.Console.WriteLine("Invalid enter");
            }
        }
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
    }
}

void DeleteContactInHandbook(string sn){
    string request = "/delete_contact?surname=" + sn;

    var responce = client.DeleteAsync(request).Result;
    var content = responce.Content.ReadAsStringAsync().Result;
    if (responce.StatusCode == HttpStatusCode.OK){
        if (content == "\"This contact was successfully deleted\""){
            System.Console.WriteLine(content);
        }
        else{
            List<Person> pokaz = Deserialize(content);
            System.Console.WriteLine($"Found {pokaz.Count} contacts with this surname");
            System.Console.WriteLine("   Last_Name\t First_name\t Telephon\t Email\t\t Country\t Role");
            for (int i = 0; i < pokaz.Count; i++){
                System.Console.WriteLine($"{i+1}  {pokaz[i].Last_name}\t {pokaz[i].First_name}\t\t {pokaz[i].Telephon}\t {pokaz[i].Email}\t\t {pokaz[i].Country}\t\t {pokaz[i].Role}");
            }
            System.Console.WriteLine();
            System.Console.WriteLine("Enter the number of the contact you want to delete: ");
            string? choice = Console.ReadLine();
            int a = 0;
            if (int.TryParse(choice, out a)){
                if (a < 0 || a > pokaz.Count){
                    System.Console.WriteLine("Invalid enter");
                }
                else{
                    string number = "%2B";
                    number += pokaz[a-1].Telephon.Substring(1);
                    request = "/delete_contact_tel?telephon=" + number;

                    responce = client.DeleteAsync(request).Result;
                    if (responce.StatusCode == HttpStatusCode.OK){
                        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
                    }
                    else{
                        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            
        }
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
    }
}

void UpDateContactInHandbook(string ln, string t, string nln, string nfn, string nt, string ne, string nc, string nr){
    string request = "/update_contact";

    var json_data = new {
        Last_name = ln,
        New_Last_name = nln,
        First_name = "",
        New_First_name = nfn,
        Telephon = t,
        New_Telephon = nt,
        Email = "",
        New_Email = ne,
        Country = "",
        New_Country = nc,
        Role = "",
        New_Role = nr
    };

    string json_Body = JsonSerializer.Serialize(json_data);
    var content = new StringContent(json_Body, Encoding.UTF8, "application/json");

    var responce = client.PatchAsync(request, content).Result;
    var answer = responce.Content.ReadAsStringAsync().Result;

    if (responce.StatusCode == HttpStatusCode.OK){
        if (answer != "\"This contact was successfully updated\""){
            List<Person> pokaz = Deserialize(answer);
            System.Console.WriteLine($"Found {pokaz.Count} contacts with this surname");
            System.Console.WriteLine("   Last_Name\t First_name\t Telephon\t Email\t\t Country\t Role");
            for (int i = 0; i < pokaz.Count; i++){
                System.Console.WriteLine($"{i+1}  {pokaz[i].Last_name}\t {pokaz[i].First_name}\t\t {pokaz[i].Telephon}\t {pokaz[i].Email}\t\t {pokaz[i].Country}\t\t {pokaz[i].Role}");
            }
            System.Console.WriteLine();
            System.Console.WriteLine("Enter the number of the contact you want to delete: ");
            string? choice = Console.ReadLine();
            int a = 0;
            if (int.TryParse(choice, out a)){
                if (a < 0 || a > pokaz.Count){
                    System.Console.WriteLine("Invalid enter");
                }
                else{
                    UpDateContactInHandbook(ln, pokaz[a-1].Telephon, nln, nfn, nt, ne, nc, nr);
                }
            }
        }
        else{
            System.Console.WriteLine(answer);
        }
    }
    else{
        System.Console.WriteLine(answer);
    }
}

void CheckHistoryofUser(){
    string request = "/show_history";

    var responce = client.GetAsync(request).Result;
    if (responce.IsSuccessStatusCode){
        var content = responce.Content.ReadAsStringAsync().Result;
        content = content.Substring(1, content.Length-2);
        int count = 0;
        for (int i = 0; i < content.Length; i++){
            if (content[i] == ',')
                count++;
        }
        string[] input = new string[count];
        input = content.Split(',');
        System.Console.WriteLine("History of your requests:");
        for (int j = 0; j < input.Length; j++)
            System.Console.WriteLine($"{j+1}.   {input[j]}");
        System.Console.WriteLine();
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
    }
}

void DeleteHistoryofUser(){
    string request = "/delete_history";

    var responce = client.DeleteAsync(request).Result;
    if (responce.IsSuccessStatusCode){
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        System.Console.WriteLine();
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        System.Console.WriteLine();
    }
}

void MakeNewPassword(string login, string password){
    string request = "/change_password";

    var json_data = new {
        Login = login,
        Password = password
    };

    string json_Body = JsonSerializer.Serialize(json_data);
    var content = new StringContent(json_Body, Encoding.UTF8, "application/json");

    var responce = client.PatchAsync(request, content).Result;
    if (responce.IsSuccessStatusCode){
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        System.Console.WriteLine();

        request = "login?login=" + login + "&password=" + password;
        responce = client.PostAsync(request, null).Result;
        IEnumerable<Cookie> responce_Cookies = cookies.GetAllCookies();
        System.Console.WriteLine("Your new cookie:");
        foreach (Cookie cookie in responce_Cookies){
            System.Console.WriteLine(cookie.Name + ": " + cookie.Value);
        }
    }
    else{
        System.Console.WriteLine(responce.Content.ReadAsStringAsync().Result);
        System.Console.WriteLine();
    }
}

bool ValidEmail(string mail){
    byte c = 0;
    for (int i = 0; i < mail.Length; i++){
        if (mail[i] == '@')
            c++;
    }
    if (c == 1)
        return true;
    else
        return false;
}




const string DEFAULT_SERVER_URL = "http://localhost:5000";


try{
    client.BaseAddress = new Uri(DEFAULT_SERVER_URL);
    bool exit = false;
    byte _exit = 0;

    while (!exit){
        System.Console.WriteLine("Choose what you want to do (write number):");
        System.Console.WriteLine("1. Sign up");
        System.Console.WriteLine("2. Log in");
        System.Console.WriteLine("3. Exit");
        System.Console.Write("Input: ");
        string? choice = Console.ReadLine();
        string username = "", password = "";
        int action = 0;

        if (int.TryParse(choice, out action)){
            switch (action)
            {
                case 1:
                    Console.Write("Write login: ");
                    username = Console.ReadLine();
                    Console.Write("Write password: ");
                    password = Console.ReadLine();

                    while(SignUpOnServer(username, password) == false){
                        System.Console.WriteLine("Try again!");
                        Console.Write("Write login: ");
                        username = Console.ReadLine();
                        Console.Write("Write password: ");
                        password = Console.ReadLine();
                    }

                    System.Console.WriteLine("Right now you can log in");
                    break;
                case 2:
                    Console.Write("Write your login: ");
                    username = Console.ReadLine();
                    Console.Write("Write your password: ");
                    password = Console.ReadLine();

                    while(LoginOnServer(username, password) == false){
                        System.Console.WriteLine("Try again!");
                        Console.Write("Write your login: ");
                        username = Console.ReadLine();
                        Console.Write("Write your password: ");
                        password = Console.ReadLine();
                    }
                    exit = true;
                    break;
                case 3:
                    exit = true;
                    _exit = 3;
                    break;
                default:
                    Console.WriteLine("Incorrect choice. Plese, chose from list action.");
                    break;
            }
        }
    }
    exit = false;

    while (!exit && _exit != 3){
        System.Console.WriteLine("Choose what you want to do (write number):");
        System.Console.WriteLine("1. Add new contact");
        System.Console.WriteLine("2. Delete contact");
        System.Console.WriteLine("3. Check one contact");
        System.Console.WriteLine("4. Check all contacts");
        System.Console.WriteLine("5. Update information about contact");
        System.Console.WriteLine("6. Check history of requests");
        System.Console.WriteLine("7. Delete history of requests");
        System.Console.WriteLine("8. Change password");
        System.Console.WriteLine("9. Exit");
        System.Console.Write("Input: ");
        string? choice = Console.ReadLine();
        int action = 0;

        if (int.TryParse(choice, out action)){
            switch (action){
                case 1:
                    System.Console.WriteLine("You can add this information about contact: Last Name, First Name, telephon, email, country, role");
                    string? first_Name = null, last_Name = null, telephon = null, email = null, country = null, role = null;
                    while (last_Name == null){
                        Console.Write("Write Last Name your contact (Necessarily): ");
                        last_Name = Console.ReadLine();
                        int len = last_Name.Length;
                        if (Regex.IsMatch(last_Name, "[!-@]+")){
                            last_Name = null;
                        }
                    }
                    while (first_Name == null){
                        Console.Write("Write First Name your contact (Necessarily): ");
                        first_Name = Console.ReadLine();
                        if (Regex.IsMatch(first_Name, "[!-@]+")){
                            first_Name = null;
                        }
                    }
                    int numbers = 0;
                    while (telephon == null || telephon.Length != 12 || int.TryParse(telephon, out numbers) || (char)telephon[0] != '+' || (char)telephon[1] != '7'){
                        Console.Write("Write telephon your contact (Necessarily, Format: +79998887766): ");
                        telephon = Console.ReadLine();
                    }
                    Console.Write("Write email your contact: ");
                    email = Console.ReadLine();
                    if (email == null || email.Length == 0)
                        email = " ";
                    else{
                        while (email.Length < 7 || !ValidEmail(email)){
                            Console.Write("Write email your contact: ");
                            email = Console.ReadLine();
                        }
                    }
                    while (country == null){
                        Console.Write("Write country your contact: ");
                        country = Console.ReadLine();
                        if (country == null || country.Length == 0){
                            country = " ";
                        }
                        if (Regex.IsMatch(country, "[!-@]+")){
                            country = null;
                        }
                    }
                    while (role == null){
                        Console.Write("Write role your contact: ");
                        role = Console.ReadLine();
                        if (role == null || role.Length == 0){
                            role = " ";
                        }
                        if (Regex.IsMatch(role, "[!-@]+")){
                            role = null;
                        }
                    }
                    
                    AddContactInHandbook(last_Name, first_Name, telephon, email, country, role);
                    System.Console.WriteLine();
                    break;
                case 2:
                    System.Console.WriteLine("Enter surname of contact you want to delete: ");
                    string? input = Console.ReadLine();
                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                        System.Console.WriteLine("Invalid enter");
                    }
                    else{
                        DeleteContactInHandbook(input);
                    }
                    break;
                case 3:
                    System.Console.WriteLine("Enter surname of contact you want to check: ");
                    input = Console.ReadLine();
                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                        System.Console.WriteLine("Invalid enter");
                    }
                    else{
                        CheckContactInHandbook(input);
                        System.Console.WriteLine();
                    }
                    break;
                case 4:
                    CheckAllContactsInHandbook();
                    System.Console.WriteLine();
                    break;
                case 5:
                    System.Console.WriteLine("Enter surname of contact you want to update: ");
                    input = Console.ReadLine();
                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                        System.Console.WriteLine("Invalid enter");
                    }
                    else{
                        string sur = "";
                        sur += input;
                        System.Console.WriteLine("Enter what do you want to update in contact information:");
                        System.Console.WriteLine("1. Change Surname");
                        System.Console.WriteLine("2. Change Name");
                        System.Console.WriteLine("3. Change Phone number");
                        System.Console.WriteLine("4. Change Email");
                        System.Console.WriteLine("5. Change Country");
                        System.Console.WriteLine("6. Change Role");
                        System.Console.Write("Input: ");
                        string? choice_ = Console.ReadLine();
                        int action_ = 0;

                        if (int.TryParse(choice_, out action_)){
                            switch (action_){
                                case 1:
                                    System.Console.WriteLine("Enter Changed Surname: ");
                                    input = Console.ReadLine();
                                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                                        System.Console.WriteLine("Invalid enter");
                                    }
                                    else{
                                        UpDateContactInHandbook(sur, "", input, "", "", "", "", "");
                                        System.Console.WriteLine();
                                    }
                                    break;
                                case 2:
                                    System.Console.WriteLine("Enter Changed Name: ");
                                    input = Console.ReadLine();
                                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                                        System.Console.WriteLine("Invalid enter");
                                    }
                                    else{
                                        UpDateContactInHandbook(sur, "", "", input, "", "", "", "");
                                        System.Console.WriteLine();
                                    }
                                    break;
                                case 3:
                                    System.Console.WriteLine("Enter Changed Telephon: ");
                                    input = Console.ReadLine();
                                    if (input == null || input.Length != 12 || int.TryParse(input, out numbers) || (char)input[0] != '+' || (char)input[1] != '7'){
                                        System.Console.WriteLine("Invalid enter");
                                    }
                                    else{
                                        UpDateContactInHandbook(sur, "", "", "", input, "", "", "");
                                        System.Console.WriteLine();
                                    }
                                    break;
                                case 4:
                                    System.Console.WriteLine("Enter Changed Email: ");
                                    input = Console.ReadLine();
                                    if (input == null || input == " " || input.Length < 7 || !ValidEmail(input)){
                                        System.Console.WriteLine("Invalid enter");
                                    }
                                    else{
                                        UpDateContactInHandbook(sur, "", "", "", "", input, "", "");
                                        System.Console.WriteLine();
                                    }
                                    break;
                                case 5:
                                    System.Console.WriteLine("Enter Changed Country: ");
                                    input = Console.ReadLine();
                                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                                        System.Console.WriteLine("Invalid enter");
                                    }
                                    else{
                                        UpDateContactInHandbook(sur, "", "", "", "", "", input, "");
                                        System.Console.WriteLine();
                                    }
                                    break;
                                case 6:
                                    System.Console.WriteLine("Enter Changed Role: ");
                                    input = Console.ReadLine();
                                    if (input == null || input == " " || Regex.IsMatch(input, "[!-@]+")){
                                        System.Console.WriteLine("Invalid enter");
                                    }
                                    else{
                                        UpDateContactInHandbook(sur, "", "", "", "", "", "", input);
                                        System.Console.WriteLine();
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case 6:
                    CheckHistoryofUser();
                    break;
                case 7:
                    DeleteHistoryofUser();
                    break;
                case 8:
                    System.Console.WriteLine("Enter New Password: ");
                    input = Console.ReadLine();
                    if (input == null || input == " "){
                        System.Console.WriteLine("Invalid enter");
                    }
                    else{
                        MakeNewPassword(login, input);
                    }
                    break;
                case 9:
                    exit = true;
                    break;
                default:
                    break;
            }
        }
        
    }
}
catch (Exception exp){
    System.Console.WriteLine("Error " + exp.Message);
}

public struct User
{
    public string Login {get; set; }
    public string Password {get; set; }
}

public struct Person{
    public string First_name { get; set; }
    public string Last_name { get; set; }
    public string Telephon { get; set; }
    public string Email { get; set; }
    public string Country { get; set; }
    public string Role { get; set; }
}

public struct NewPerson{
    public string New_First_name { get; set; }
    public string New_Last_name { get; set; }
    public string New_Telephon { get; set; }
    public string New_Email { get; set; }
    public string New_Country { get; set; }
    public string New_Role { get; set; }
}