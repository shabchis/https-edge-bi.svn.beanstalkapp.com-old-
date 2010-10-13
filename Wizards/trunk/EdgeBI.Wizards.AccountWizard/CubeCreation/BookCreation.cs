using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;

namespace EdgeBI.Wizards.AccountWizard.CubeCreation
{
    public class BookCreation
    {
        public void BookUpdate(string cubeName, string source, string destination, 
            string panoramaAppRefreshFile, string appXmlFilePath, string cubeAddress,
            string cubeDb, string calcMembersNewUser, string calcMembersNewActivations,
            string newUserToReplace, string newActivationsToReplace, Dictionary<string, string> strings)
        {
            //string source = string.Empty;
            //string destination = string.Empty;
            //source = @"\\gidi-pc\d$\Program Files\Panorama\E-BI\books\BO PPC VS BCK";
            //destination = @"\\gidi-pc\d$\Program Files\Panorama\E-BI\books\BO PPC VS BCK_backup";
            backupBookFolder(source, destination, newUserToReplace, newActivationsToReplace);
            updateBookFiles(destination, cubeName, cubeAddress, cubeDb, calcMembersNewUser, calcMembersNewActivations, newUserToReplace, newActivationsToReplace, strings);
            updateApplicationFileWithBook(cubeName, appXmlFilePath);
            refreshPanorama(panoramaAppRefreshFile);
        }

        private void refreshPanorama(string panoramaAppRefreshFile)
        {
            System.Diagnostics.Process Proc = new System.Diagnostics.Process();
            Proc.StartInfo.FileName = panoramaAppRefreshFile;
            Proc.Start();
            Proc.Kill();
        }
        private void updateApplicationFileWithBook(string CubeName, string appXmlFilePath)
        {
            StringBuilder nodeString = new StringBuilder();
            string xmlString = string.Empty;
            string source = appXmlFilePath;
            XmlDocument xd = new XmlDocument();
            XmlTextReader reader = null;
            XmlNode xn = null, tempNode = null;
            int counter = 0;
            int index;

            reader = new XmlTextReader(source);
            reader.Read();
            xmlString = reader.ReadOuterXml();
            index = xmlString.LastIndexOf("</Item");
            index = xmlString.IndexOf('>',index);
            index++;


            xd.LoadXml(xmlString);
            xn = xd.SelectSingleNode("//Item1");
            if (xn == null)
                throw new Exception("Applications.xml file is incorrect or not exist.");
            while (xn != null)
            {
                tempNode = xn;
                xn = xn.NextSibling;
                counter++;
            }
            counter++;

            nodeString.Append("<Item");
            nodeString.Append(counter);
            nodeString.Append(">");
            nodeString.Append("<Properties Name=\"");
            nodeString.Append(CubeName);
            nodeString.Append("\" Path=\"D:\\Program Files\\Panorama\\E-BI\\Books\\");
            nodeString.Append(CubeName);
            nodeString.Append("\\schema.xml\" Description=\"\" Flags=\"3\" DefPerm=\"0\" /><Roles><Properties Value=\"Pn0102{}\" /></Roles>");
            nodeString.Append("</Item");
            nodeString.Append(counter);
            nodeString.Append(">");
            xmlString = xmlString.Insert(index, nodeString.ToString());

            //xn.InnerXml = nodeString.ToString();

            reader.Close();
            File.WriteAllText(source, xmlString);
        }
        private void updateBookFiles(string destination, string CubeName, string cubeAddress, string cubeDb, 
            string calcMembersNewUser, string calcMembersNewActivations, string newUserToReplace,
            string newActivationsToReplace, Dictionary<string, string> strings)
        {
            DirectoryInfo Destination = new DirectoryInfo(destination);
            //XmlDocument xd = new XmlDocument();
            string xmlString = string.Empty;
            //string dataString = string.Empty;
            string cubeName = string.Empty;
            string newBookDetails = string.Empty;


            cubeName = CubeName;
            //XmlNode xn = null;
            //XmlTextReader reader = null;

            newBookDetails = "<Cube><Properties CubeAddress=\"";
            newBookDetails += cubeAddress;
            newBookDetails += "\" CubeName=\"";
            newBookDetails += cubeName;
            newBookDetails += "\" CubeDB=\"";
            newBookDetails += cubeDb;
            newBookDetails += "\"/>";

            foreach (FileInfo fi in Destination.GetFiles())
            {
                //skipping Schema.xml & Properties.xml files
                if (fi.Name.ToLower() == "schema.xml" || fi.Name.ToLower() == "properties.xml")
                    continue;
                xmlString = File.ReadAllText(Path.Combine(destination, fi.Name));
                xmlString = xmlString.Replace("<Cube><Properties CubeAddress=\"hal\" CubeName=\"BO PPC Cube\" CubeDB=\"easynet_UDM\" />",newBookDetails);
                xmlString = xmlString.Replace("<Cube><Properties CubeAddress=\"hal\" CubeName=\"EasyNet-New Cube\" CubeDB=\"easynet_UDM\" />",newBookDetails);
                xmlString = xmlString.Replace("<Cube><Properties CubeAddress=\"hal\" CubeName=\"Content Cube\" CubeDB=\"easynet_UDM\" />",newBookDetails);
                xmlString = xmlString.Replace("<Cube><Properties CubeAddress=\"hal\" CubeName=\"BO PPC Cube\" CubeDB=\"easynet_UDM\"/>", newBookDetails);
                xmlString = xmlString.Replace("<Cube><Properties CubeAddress=\"hal\" CubeName=\"EasyNet-New Cube\" CubeDB=\"easynet_UDM\"/>", newBookDetails);
                xmlString = xmlString.Replace("<Cube><Properties CubeAddress=\"hal\" CubeName=\"Content Cube\" CubeDB=\"easynet_UDM\"/>", newBookDetails);


                string sPattern = calcMembersNewUser;
                int index = 0;
                while (index < xmlString.Length)
                {
                    if (index == -1)
                        break;
                    index = xmlString.ToLower().IndexOf(sPattern.ToLower(), index);
                    if (index == -1)
                        break;
                    char ch = xmlString.Substring(index + sPattern.Length, 1)[0];
                    char formerCh = xmlString.Substring(index - 1, 1)[0];
                    if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                    {
                        xmlString = xmlString.Substring(0, index) + newUserToReplace + xmlString.Substring(index + sPattern.Length);
                    }
                }
                index = 0;
                sPattern = calcMembersNewActivations;
                while (index < xmlString.Length)
                {
                    if (index == -1)
                        break;
                    index = xmlString.ToLower().IndexOf(sPattern.ToLower(), index);
                    if (index == -1)
                        break;
                    char ch = xmlString.Substring(index + sPattern.Length, 1)[0];
                    char formerCh = xmlString.Substring(index - 1, 1)[0];
                    if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                    {
                        xmlString = xmlString.Substring(0, index) + newActivationsToReplace + xmlString.Substring(index + sPattern.Length);
                    }
                }

                //xmlString = xmlString.Replace(calcMembersNewUser, newUserToReplace);


                //xmlString = xmlString.Replace(calcMembersNewActivations, newActivationsToReplace);

                IDictionaryEnumerator ie = strings.GetEnumerator();
                while (ie.MoveNext())
                {
                    sPattern = (string)ie.Key;
                    index = 0;
                    while (index < xmlString.Length)
                    {
                        if (index == -1)
                            break;
                        index = xmlString.ToLower().IndexOf(sPattern.ToLower(), index);
                        if (index == -1)
                            break;
                        char ch = xmlString.Substring(index + sPattern.Length, 1)[0];
                        char formerCh = xmlString.Substring(index - 1, 1)[0];
                        if (!char.IsLetter(ch) && !char.IsNumber(ch) && !char.IsLetter(formerCh) && !char.IsNumber(formerCh))
                        {
                            xmlString = xmlString.Substring(0, index) + (string)ie.Value + xmlString.Substring(index + sPattern.Length);
                        }
                    }

                    //xmlString = xmlString.Replace((string)ie.Key, (string)ie.Value);
                }
                File.WriteAllText(Path.Combine(destination, fi.Name), xmlString);            
            }
        }
        private void backupBookFolder(string source, string target, string newRegs, string newActives)
        {
            DirectoryInfo Source = new DirectoryInfo(source);
            DirectoryInfo Target = new DirectoryInfo(target);

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(Target.FullName) == false)
            {
                try
                {
                    Directory.CreateDirectory(Target.FullName);
                }
                catch
                {
                 //   MessageBox.Show("gidi-pc is not accessible. please connect once gidi-pc with administrator and run again.");
                    return;
                }
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in Source.GetFiles())
            {
                if( fi.Name.Equals("2. ROI by Actives.xml") && !newActives.Equals(""))
                    fi.CopyTo(Path.Combine(target.ToString(), "2. ROI by " + newActives + ".xml"), true);
                else if (fi.Name.Equals("2. ROI by Regs.xml") && !newRegs.Equals(""))
                    fi.CopyTo(Path.Combine(target.ToString(), "2. ROI by " + newRegs + ".xml"), true);
                else 
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in Source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    Target.CreateSubdirectory(diSourceSubDir.Name);
                backupBookFolder(diSourceSubDir.FullName, nextTargetSubDir.FullName, newRegs, newActives);
            }
        }
    }
}
