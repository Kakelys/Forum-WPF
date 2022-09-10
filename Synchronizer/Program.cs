using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Synchronizer;


var mainFile = "lang.xaml";
int choose = 0;
/*
Console.WriteLine("Welcome to Synchronizer, want to get start? \n" +
                  "1 - yea \n" +
                  "0 - exit \n");

choose = int.Parse(Console.Read().ToString());
switch (choose)
{
    case 0:
        return;
    case 1:
        break;
    default:
        return;
}


Console.WriteLine($"Is {mainFile} file with main language or you want to change it?\n" +
                  "1 - main \n" +
                  "2 - change \n" +
                  "0 - exit \n");

choose = int.Parse(Console.Read().ToString());
switch (choose)
{
    case 0:
        return;
    case 1:
        break;
    case 2:
        return;
    default:
        return;
}
*/


try
{
    


    Synchronizer.Synchronizer.SetMainFile(mainFile);

    var tempXml = new XmlDocument();
    string fileName = "lang.ru-RU.xaml";
    tempXml.Load(fileName);
    Synchronizer.Synchronizer.Synchronize(Synchronizer.Synchronizer.MainLang,  tempXml, fileName);







}
catch(Exception e)
{
    Console.WriteLine(e.Message);
}

