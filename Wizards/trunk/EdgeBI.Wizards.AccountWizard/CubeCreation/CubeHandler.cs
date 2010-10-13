using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using Microsoft.AnalysisServices;
using System.IO;
using Microsoft.AnalysisServices.AdomdClient;

namespace EdgeBI.Wizards.AccountWizard.CubeCreation
{
    public class CubeHandler
    {
        //protected Database _database;
        protected string XmlFromCube(Cube cb, string fileName)
        {
            string currDir = Directory.GetCurrentDirectory();
            //Dimension productDimension = _database.Dimensions.GetByName("Product");
            System.Xml.XmlTextWriter xmlWriter = new System.Xml.XmlTextWriter(fileName, System.Text.Encoding.UTF8);
            xmlWriter.Formatting = System.Xml.Formatting.Indented;
            xmlWriter.Indentation = 2;
            try
            {
                //Scripter.WriteStartBatch(xmlWriter, true); // true = transactional
                Scripter.WriteStartParallel(xmlWriter);
                IMajorObject mo = null;
                mo = cb;
                //ScriptInfo f = new ScriptInfo(mo, ScriptAction.Create, ScriptOptions.Default, true);

                Scripter.WriteCreate(xmlWriter, null, mo, true, false);
                
                Scripter.WriteEndParallel(xmlWriter);
            }
            finally
            {
                xmlWriter.Close();
            }
            StringBuilder newFile = new StringBuilder();
            string temp = string.Empty;
            string[] file = File.ReadAllLines(currDir + "\\" + fileName);
            foreach (string line in file)
            {
                if (line.ToLower().Contains("parallel") || line.ToLower().Contains("create>"))
                {

                    //do nothing
                }
                else
                {
                        newFile.Append(line + "\n");
                }
            }
            
            File.WriteAllText(currDir + "\\" + fileName, newFile.ToString());
            return newFile.ToString();

        }
        protected Cube getCubeObject(Database db, string cubeTemplateId)
        {
            Cube cb = new Cube();

            //Server server = new Server();
            //string connectionString = "Datasource=" + dataSource;
            //string databaseName = dbName;
            //string fileName = xmlFileName;
            //server.Connect(connectionString);
            //_database = server.Databases.GetByName(databaseName);
            cb = db.Cubes[cubeTemplateId];

            return cb;
        }
        protected void CreateCubeFromXml(string xmlString, string dataSource)
        {
            //string currDir = Directory.GetCurrentDirectory();
            //TextReader tr = File.OpenText(currDir + "\\" + fileName);
            //string xmla = tr.ReadToEnd();
            //tr.Close();
            AdomdConnection cn = new AdomdConnection("Data Source=" + dataSource);
            cn.Open();
            AdomdCommand cmd = cn.CreateCommand();
            cmd.CommandText = xmlString;
            cmd.ExecuteNonQuery();
            cn.Close();
        }
    }
}
