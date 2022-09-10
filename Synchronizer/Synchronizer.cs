using System.Text.RegularExpressions;
using System.Xml;

namespace Synchronizer;

public class Synchronizer
{
    private static List<string> FileNames = new();

    private static string mainFileName;
    public static XmlDocument MainLang = new();

    public static void Synchronize(XmlDocument main,XmlDocument toSynchronize, string fileName = "test.xaml")
    {
        var temp = GetStringNodes(main);
        var mainNodeList = temp.nodeList;
        var mainCommentList = temp.commentList;

        temp = GetStringNodes(toSynchronize);
        var synchNodeList = temp.nodeList;
        var synchCommentList = temp.commentList;

        

        XmlNode importNode;
        if (synchNodeList.Count == 0)
        {
            for (var i = 0; i < mainNodeList.Count; i++)
            {
                importNode = toSynchronize["ResourceDictionary"].OwnerDocument.ImportNode(mainNodeList[i], true);

                toSynchronize["ResourceDictionary"].AppendChild(importNode);
            }
        }

        for (int i = 0, j = 0; i < mainNodeList.Count;i++,j++)
        {

            if (j < synchNodeList.Count)
            {
                switch (mainNodeList[i].Name)
                {
                    case "v:String" when CheckSame(mainNodeList[i], synchNodeList[j]):
                    {
                        if (mainNodeList[i].Attributes.GetNamedItem("x:Key").Value !=
                            synchNodeList[j].Attributes.GetNamedItem("x:Key").Value)
                        {
                            synchNodeList.Insert(j, mainNodeList[i]);
                        }

                        break;
                    }
                    case "v:String" when synchNodeList[j].Name=="#comment":
                    {
                        if (mainCommentList.FirstOrDefault(n=>n.OuterXml == synchNodeList[j].OuterXml)!=null)
                        {
                            synchNodeList.Insert(j, mainNodeList[i]);
                        }
                        else
                        {
                            i--;
                            continue;
                        }

                        break;
                    }
                    case "#comment" when CheckSame(mainNodeList[i], synchNodeList[j]):
                    {
                        if (mainNodeList[i].OuterXml == synchNodeList[j].OuterXml)
                        {
                            continue;
                        }
                        else
                        {
                            if (synchCommentList.FirstOrDefault(n => n.OuterXml == mainNodeList[i].OuterXml) != null)
                            {
                                i--;
                            }
                            else
                            {
                                synchNodeList.Insert(j, mainNodeList[i]);
                            }
                        }

                        break;
                    }
                    case "#comment" when synchNodeList[j].Name == "v:String" && synchCommentList.FirstOrDefault(n => n.OuterXml == mainNodeList[i].OuterXml) == null:
                        synchNodeList.Insert(j, mainNodeList[i]);
                        break;
                }
            }
            else
            {
                synchNodeList.Add(mainNodeList[i]);
            }


        }

        var xml = new XmlDocument();
        importNode = xml.ImportNode(main["ResourceDictionary"], false);
        xml.AppendChild(importNode);

        for (var i = 0; i < synchNodeList.Count; i++)
        {
            importNode = xml.DocumentElement.OwnerDocument.ImportNode(synchNodeList[i], true);
            xml.DocumentElement.AppendChild(importNode);
        }

        xml.Save(fileName);
    }

    private static bool CheckSame(XmlNode first, XmlNode second)
    {
        return first.Name == second.Name;
    }

    private static (List<XmlNode> nodeList, List<XmlNode> commentList) GetStringNodes(XmlDocument doc)
    {
        var nodeList = new List<XmlNode>();
        var commentList = new List<XmlNode>();
        var temp = doc["ResourceDictionary"].ChildNodes;
        for (int i = 0; i < temp.Count; i++)
        {
            switch (temp[i].Name)
            {
                case "#comment":
                    nodeList.Add(temp[i]);
                    commentList.Add(temp[i]);
                    break;
                case "v:String":
                    nodeList.Add(temp[i]);
                    break;
                default: continue;
            }
        }

        return (nodeList, commentList);
    }

    public static void SetMainFile(string mainFile)
    {
        if (!File.Exists(mainFile))
            throw new Exception("Main File Doesn't Exists");

        mainFileName = mainFile;
        MainLang.Load(mainFileName);

        LoadNames();
    }

    private static void LoadNames()
    {
        var d = new DirectoryInfo("./");

        var regex = new Regex(@"^lang.\S+.xaml$");
        FileNames = d
            .GetFiles("")
            .Where(f => regex.IsMatch(f.Name))
            .Select(f => f.Name).ToList();
    }

    
}