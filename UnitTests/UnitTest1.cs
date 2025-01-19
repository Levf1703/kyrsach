using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

[TestClass]
public class UnitTest1
{
        OptionsBook opb = new OptionsBook();
        DBManager deb = new DBManager();
        
    [TestMethod]
    public void TestCheckContact()
    {

        List<Person> example = opb.Check_Contact("Tusur", "Zanko");

        Assert.IsTrue(example.Count == 2);
        Assert.IsTrue(example[0].Last_name == "Zanko");
        Assert.IsTrue(example[1].Last_name == "Zanko");
    }

    [TestMethod]
    public void TestCheckAllContacts()
    {
        List<Person> example = opb.Check_Contacts("Tusur");

        Assert.IsTrue(example.Count == 11);
        Assert.IsTrue(example[0].Last_name == "Boriskin");
        Assert.IsTrue(example[0].Last_name[0] <= example[1].Last_name[0]);
    }

    [TestMethod]
    public void TestAddContact()
    {
        Person men = new Person();
        men.Last_name = "Ivanov";
        men.First_name = "Ivan";
        men.Telephon = "+79998887766";
        men.Email = "";
        men.Country = "Russia";
        men.Role = "";
        
        bool example = opb.Add_Contact("Tusur", men);

        Assert.IsTrue(example == false);
    }

    [TestMethod]
    public void TestUpdateContact()
    {
        NewPerson men = new NewPerson();
        men.Last_name = "Kemerov";
        men.New_Last_name = "";
        men.First_name = "";
        men.New_First_name = "";
        men.Telephon = "";
        men.New_Telephon = "";
        men.Email = "";
        men.New_Email = "";
        men.Country = "";
        men.New_Country = "USA";
        men.Role = "";
        men.New_Role = "";
        
        bool example = opb.Update_Contact("Tusur", men);

        Assert.IsTrue(example);
    }

    [TestMethod]
    public void TestDeleteContact()
    {
        bool example = opb.Delete_Contact("Tusur", "Ivanov");

        Assert.IsTrue(example);
    }

    [TestMethod]
    public void TestDeleteContactTel()
    {
        bool example = opb.Delete_Contact_Tel("Tusur", "+79235550920");

        Assert.IsTrue(example);
    }

    [TestMethod]
    public void TestFindContact()
    {

        List<Person> example = opb.Find_Contact("Tusur", "Z");

        Assert.IsTrue(example.Count == 1);
        Assert.IsTrue(example[0].Last_name == "Zanko");
    }

    [TestMethod]
    public void TestCheckHistory()
    {
        List<string> example = opb.Check_History("Tusur");

        Assert.IsTrue(example.Count == 3);
    }

    [TestMethod]
    public void TestDeleteHistory()
    {
        bool example = opb.Delete_History("Tusur");

        Assert.IsTrue(example);
    }

    [TestMethod]
    public void TestChangePassword()
    {
        User user = new User();
        user.Login = "Tusur";
        user.Password = "Tusur";

        bool example = opb.Change_Password(user);

        Assert.IsTrue(example);
    }
}