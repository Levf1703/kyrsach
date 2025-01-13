using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

HBWebAdapter hb = new HBWebAdapter();

app.MapGet("/", () => "Welcome to our telephone book!");
app.MapPost("/login", (string login, string password, HttpContext context) => hb.LogIn(login, password, context));
app.MapPost("/signup", (string login, string password) => hb.SignUp(login, password));
app.MapGet("/show_history", [Authorize] (HttpContext context) => hb.ShowHistory(context));
app.MapDelete("/delete_history", [Authorize] (HttpContext context) => hb.DeleteHistory(context));
app.MapPatch("/change_password", [Authorize] (HttpContext context, [FromBody] User user) => hb.ChangePasswordUser(context, user));
app.MapPost("/add_contact", [Authorize] ([FromBody] Person person, HttpContext context) => hb.AddContact(person, context));
app.MapGet("/check_contact", [Authorize] (HttpContext context, string surname) => hb.CheckContact(context, surname));
app.MapGet("/check_contacts", [Authorize] (HttpContext context) => hb.CheckContacts(context));
app.MapPatch("/update_contact", [Authorize] (HttpContext context, [FromBody] NewPerson person) => hb.UpDateContact(context, person));
app.MapDelete("/delete_contact", [Authorize] (HttpContext context, string surname) => hb.Delete_Contact(context, surname));
app.MapDelete("/delete_contact_tel", [Authorize] (HttpContext context, string telephon) => hb.Delete_Contact_Tel(context, telephon));


app.Run();


public class HBWebAdapter{
    private OptionsBook ob = new OptionsBook();

    public async Task<IResult> LogIn(string login, string password, HttpContext context){
        if (!ob.Login(login, password)){
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Unauthorized();
        }
        else{
            var claims = new List<Claim> {new Claim(ClaimTypes.Name, login)};
            var ClaimsIdentity = new ClaimsIdentity(claims, "Cookies");
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(ClaimsIdentity));
            return Results.Ok(context.Request.Cookies[".AspNetCore.Cookies"]);
        }        
    }

    public IResult SignUp(string login, string password){
        if (ob.SighUp(login, password))
            return Results.Ok("User " + login + " registered successfully");
        else
            return Results.BadRequest("Falied to register user " + login);
    }

    public IResult AddContact(Person contact, HttpContext context){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;
        
        bool answer = ob.Add_Contact(login, contact);
        if (answer)
            return Results.Ok("You added new contact");
        else
            return Results.BadRequest("Failed to add new contact");
    }   

    public IResult CheckContact(HttpContext context, string surname){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;
        List<Person> count = ob.Check_Contact(login, surname);
        if (count.Count == 0)
            return Results.NotFound("This contact not found");
        else
            return Results.Ok(count);
    }

    public IResult CheckContacts(HttpContext context){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;
            List<Person> output = ob.Check_Contacts(login);
            return Results.Ok(output);
    }

    public IResult UpDateContact(HttpContext context, NewPerson person){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;
        
        bool count = ob.Update_Contact(login, person);
        if (!count){
            List<Person> answer = ob.Check_Contact(login, person.Last_name);
            if (answer.Count > 1 && (person.Telephon == "" || person.Telephon == " "))
                return Results.Ok(answer);
            if (answer.Count == 0)
                return Results.NotFound("This contact not found");
            return Results.BadRequest("This contact wasn't successfully updated");
        }
        else{
            return Results.Ok("This contact was successfully updated");
        }
    }

    public IResult Delete_Contact(HttpContext context, string surname){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;
        bool count = ob.Delete_Contact(login, surname);
        if (!count){
            List<Person> answer = ob.Check_Contact(login, surname);
            if (answer.Count > 1)
                return Results.Ok(answer);
            if (answer.Count == 0)
                return Results.NotFound("This contact not found");
            return Results.BadRequest("This contact wasn't successfully deleted");
        }
        else{
            return Results.Ok("This contact was successfully deleted");
        }
    }

    public IResult Delete_Contact_Tel(HttpContext context, string phone){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;
        bool count = ob.Delete_Contact_Tel(login, phone);
        if (count)
            return Results.Ok("This contact was successfully deleted");
        else
            return Results.BadRequest("Failed to delete");
    }

    public IResult ShowHistory(HttpContext context){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;

        return Results.Ok(ob.Check_History(login));
    }

    public IResult DeleteHistory(HttpContext context){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;

        if (ob.Delete_History(login))
            return Results.Ok("You clear your history request"); 
        else
            return Results.Conflict("Failed to delete history request");
    }

    public IResult ChangePasswordUser(HttpContext context, User user){
        string login = "";
        if (context.User.Identity == null)
            return Results.Unauthorized();
        else
            login = context.User.Identity.Name;

        if (ob.Change_Password(user))
            return Results.Ok("You have changed your password");
        else
            return Results.BadRequest("Failed to change password");
    }
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
    public string First_name { get; set; }
    public string New_First_name { get; set; }
    public string Last_name { get; set; }
    public string New_Last_name { get; set; }
    public string Telephon { get; set; }
    public string New_Telephon { get; set; }
    public string Email { get; set; }
    public string New_Email { get; set; }
    public string Country { get; set; }
    public string New_Country { get; set; }
    public string Role { get; set; }
    public string New_Role { get; set; }
}

public class OptionsBook
{
    DBManager manag = new DBManager();
    const string DB_Path = "/home/andrey/code/users.db";
    public bool Login(string log, string pass){
        manag.ConnectToBD(DB_Path);
        if (manag.CheckUser(log, pass)){
            manag.WriteAction(log, "Login on site");
            manag.Disconnect();
            return true;
        }
        else{
            manag.Disconnect();
            return false;
        }
    }
    public bool SighUp(string log, string pass){
        manag.ConnectToBD(DB_Path);
        if (manag.AddUser(log, pass)){
            manag.CreateTable(log);
            manag.CreateHistoryTable(log);
            manag.WriteAction(log, "Signup");
            manag.Disconnect();
            return true;
        }
        else{
            manag.Disconnect();
            return false;
        }
    }
    public List<Person> Check_Contact(string log, string sn){
        manag.ConnectToBD(DB_Path);
        List<Person> pep = manag.CheckContact(log);
        List<Person> count = Duplicate(pep, sn);
        if (count.Count == 0)
            manag.WriteAction(log, $"Failed to check contact with surname: {sn}");
        else
            manag.WriteAction(log, $"Checked contact");
        manag.Disconnect();
        return count;
    }
    public List<Person> Check_Contacts(string log){
        manag.ConnectToBD(DB_Path);
        List<Person> pep = manag.CheckContact(log);
        List<Person> count = SortList(pep, 0, pep.Count-1);
        manag.WriteAction(log, "Checked all contacts");
        manag.Disconnect();
        return count;
    }
    public bool Add_Contact(string log, Person kent){
        manag.ConnectToBD(DB_Path);
        List<Person> pep = manag.CheckContact(log);
        List<Person> count = Duplicate(pep, kent.Last_name);
        if (count.Count == 0){
            if (manag.AddContact(kent, log)){
                manag.WriteAction(log, "Added new contact");
                manag.Disconnect();
                return true;
            }
            else{
                manag.WriteAction(log, "Failed to add new contact");
                manag.Disconnect();
                return false;
            }
        }
        else{
            int c = 0;
            for (int i = 0; i < count.Count; i++){
                if (count[i].Telephon == kent.Telephon)
                c++;
            }
            if (c == 0){
                if (manag.AddContact(kent, log)){
                    manag.WriteAction(log, "Added new contact");
                    manag.Disconnect();
                    return true;
                }
                else{
                    manag.WriteAction(log, "Failed to add new contact");
                    manag.Disconnect();
                    return false;
                }
            }
            else{
                manag.WriteAction(log, "Failed to add new contact");
                manag.Disconnect();
                return false;
            }
        }
    }
    public bool Update_Contact(string log, NewPerson kent){
        manag.ConnectToBD(DB_Path);
        List<Person> pep = manag.CheckContact(log);
        List<Person> count = Duplicate(pep, kent.Last_name);
        if (count.Count == 0){
            manag.WriteAction(log, "Failed to update contact");
            manag.Disconnect();
            return false;
        }
        if (count.Count == 1){
            if (manag.UpdateContact(log, kent)){
                manag.WriteAction(log, "Successfully updated contact");
                manag.Disconnect();
                return true;
            }
            else{
                manag.WriteAction(log, "Failed to update contact");
                manag.Disconnect();
                return false;
            }
        }
        if (kent.Telephon[0] == '+'){
            if (manag.UpdateContact(log, kent)){
                manag.WriteAction(log, "Successfully updated contact");
                manag.Disconnect();
                return true;
            }
            else{
                manag.WriteAction(log, "Failed to update contact");
                manag.Disconnect();
                return false;
            }
        }
        manag.Disconnect();
        return false;
    }
    public bool Delete_Contact(string log, string sn){
        manag.ConnectToBD(DB_Path);
        List<Person> pep = manag.CheckContact(log);
        List<Person> count = Duplicate(pep, sn);
        if (count.Count == 0){
            manag.WriteAction(log, "Failed to delete contact");
            manag.Disconnect();
            return false;
        }
        if (count.Count == 1){
            if (manag.DeleteContact(log, sn)){
                manag.WriteAction(log, "Successfully deleted contact");
                manag.Disconnect();
                return true;
            }
            else{
                manag.WriteAction(log, "Failed to delete contact");
                manag.Disconnect();
                return false;
            }
        }
        manag.Disconnect();
        return false;
    }
    public bool Delete_Contact_Tel(string log, string tel){
        manag.ConnectToBD(DB_Path);
        if (manag.DeleteContactTel(log, tel)){
            manag.WriteAction(log, "Successfully deleted contact");
            manag.Disconnect();
            return true;
        }
        else{
            manag.WriteAction(log, "Failed to delete contact");
            manag.Disconnect();
            return false;
        }
    }
    public List<string> Check_History(string log){
        manag.ConnectToBD(DB_Path);
        List<string> answer = manag.CheckHistory(log);
        manag.Disconnect();
        return answer;
    }
    public bool Delete_History(string log){
        manag.ConnectToBD(DB_Path);
        if (manag.DeleteHistory(log)){
            manag.WriteAction(log, "Clear history");
            manag.Disconnect();
            return true;
        }
        else{
            manag.WriteAction(log, "Failed to clear history");
            manag.Disconnect();
            return false;
        }
    }
    public bool Change_Password(User u){
        manag.ConnectToBD(DB_Path);
        if (manag.ChangePassword(u.Login, u.Password)){
            manag.WriteAction(u.Login, "Changed password");
            manag.Disconnect();
            return true;
        }
        else{
            manag.WriteAction(u.Login, "Failed to change password");
            manag.Disconnect();
            return false;
        }
    }
    public List<Person> Duplicate(List<Person> list, string lasnam){
        list = SortList(list, 0, list.Count-1);
        Person answer = BinarySearch1(list, lasnam);
        if (answer.Last_name == lasnam){
            List<Person> answer_1 = new List<Person>();
            answer_1.Add(answer);
            while (answer.Last_name != null){
                list.Remove(answer);
                answer = BinarySearch1(list, lasnam);
                if (answer.Last_name != null){
                    answer_1.Add(answer);
                }
            }
            return answer_1;
        }
        else{
            List<Person> net = new List<Person>();
            return net;
        }
    }
    public List<Person> SortList(List<Person> people, int first, int last){
        int index = 0;
        int l_hold = first;
        int r_hold = last;
        Person pivot_1 = people[first];
        string pivot = people[first].Last_name;
        while (first < last)
        {
            while ((CompareName(people[last].Last_name, pivot, ">") == true) && (first < last))
                last--;
            if (first != last)
            {
                people[first] = people[last];
                first++;
            }
            while ((CompareName(people[first].Last_name, pivot, "<") == true) && (first < last))
                first++;
            if (first != last)
            {
                people[last] = people[first];
                last--;
            }
        }
        people[first] = pivot_1;
        index = first;
        first = l_hold;
        last = r_hold;
        if (first < index)
            SortList(people, first, index - 1);
        if (last > index)
            SortList(people, index + 1, last);
        return people;
    }
    public Person BinarySearch1(List<Person> list, string lasnam){
        int low = 0, high = list.Count-1;
        int center = (int)((low + high)/2);
        string middle = list[center].Last_name;

        if (lasnam == middle){
            return list[center];
        }
        else{
            while (high - low != 1){
                center = (int)((low + high)/2);
                middle = list[center].Last_name;
                if (lasnam == middle){
                    return list[center];
                }
                if (CompareName(lasnam, middle, "<") == true){
                    high = center;
                }
                if (CompareName(lasnam, middle, ">") == true){
                    low = center;
                }
            }
            if (list[low].Last_name == lasnam){
                return list[low];
            }
            if (list[high].Last_name == lasnam){
                return list[high];
            }
            Person net = new Person();
            return net;
        }
    }
    public bool CompareName(string name1, string name2, string operate){
        int i = 0;
        while (i < Math.Min(name1.Length, name2.Length)){
            if (operate == ">"){
                if ((char)name1[i] > (char)name2[i]){
                    return true;
                }
                if ((char)name1[i] < (char)name2[i]){
                    return false;
                }
                i++;
            }
            else {
                if ((char)name1[i] > (char)name2[i]){
                    return false;
                }
                if ((char)name1[i] < (char)name2[i]){
                    return true;
                }
                i++;
            }
        }
        return false;
    }
}