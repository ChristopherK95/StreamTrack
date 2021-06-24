using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    static class SettingsVariables
    {

        public static double height;
        public static int fontSize;
        static string path;
        private static StreamWriter FileWriter;
        public static List<Setting> SettingList = new();
        public static List<JObject> JsonList = new();
        static SettingsVariables()
        {
            LoadSettings();
        }
        
        public static void LoadSettings()
        {
            path = Environment.CurrentDirectory + "\\Settings.json";
            if (!File.Exists(path))
            {
                LoadDefault();
            }
            else
            {
                using (StreamReader fileReader = File.OpenText(path))
                {
                    string jsonString = fileReader.ReadToEnd();
                    fileReader.Close();
                    if (jsonString != "")
                    {
                        SettingList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Setting>>(jsonString);

                        if(SettingList.Count == 2)
                        {
                            try
                            {
                                height = SettingList.Find(x => x.name.Contains("height")).value;
                                fontSize = (int)SettingList.Find(x => x.name.Contains("fontSize")).value;
                                
                            }
                            catch
                            {
                                LoadDefault();
                            }
                            
                        }
                        else
                        {
                            LoadDefault();
                        }
                    }
                    else
                    {
                        JsonList.Add(new JObject(new JProperty("name", "height"), new JProperty("value", 60)));
                        JsonList.Add(new JObject(new JProperty("name", "fontSize"), new JProperty("value", 2)));
                        
                        using (FileWriter = File.CreateText(path))
                        {
                            Newtonsoft.Json.JsonSerializer Serializer = new();
                            Serializer.Serialize(FileWriter, JsonList);
                        }
                        FileWriter.Close();

                        SettingList.Add(new Setting() { name = "height", value = 60 });
                        SettingList.Add(new Setting() { name = "fontSize", value = 2 });

                        height = SettingList.Find(x => x.name.Contains("height")).value;
                        fontSize = SettingList.Find(x => x.name.Contains("fontSize")).value;
                    }
                }
            }
        }

        public static void SaveSettings()
        {
            JObject SettingJson_Height = new(
                new JProperty("name", "height"),
                new JProperty("value", height));
            JObject SettingJson_FontSize = new(
                new JProperty("name", "fontSize"),
                new JProperty("value", fontSize));

            JsonList.Clear();
            JsonList.Add(SettingJson_Height);
            JsonList.Add(SettingJson_FontSize);
            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
            FileWriter.Close();
        }

        public static void LoadDefault()
        {
            JsonList.Clear();
            JsonList.Add(new JObject(new JProperty("name", "height"), new JProperty("value", 60)));
            JsonList.Add(new JObject(new JProperty("name", "fontSize"), new JProperty("value", 2)));

            SettingList.Clear();
            SettingList.Add(new Setting() { name = "height", value = 60 });
            SettingList.Add(new Setting() { name = "fontSize", value = 2 });

            height = SettingList.Find(x => x.name.Contains("height")).value;
            fontSize = SettingList.Find(x => x.name.Contains("fontSize")).value;

            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
            FileWriter.Close();
        }

    }

    
}
