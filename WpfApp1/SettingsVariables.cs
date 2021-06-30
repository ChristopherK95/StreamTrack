using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace WpfApp1
{
    static class SettingsVariables
    {

        public static double height;
        public static int fontSize;
        public static string themeColor = "#1c2026";
        public static string themeColor2 = "#30343a";
        public static string fontColor = "#ffffff";
        public static string fontColor2 = "#cdcdcd";

        static string path;
        private static StreamWriter FileWriter;
        public static List<Setting> SettingList = new();
        public static List<JObject> JsonList = new();
        static SettingsVariables()
        {
            SettingsStartUp();
        }
        
        public static void SettingsStartUp()
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
                            LoadSettings();
                            
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
                        JsonList.Add(new JObject(new JProperty("name", "themeColor"), new JProperty("value", "#1c2026")));
                        JsonList.Add(new JObject(new JProperty("name", "themeColor2"), new JProperty("value", "#30343a")));
                        JsonList.Add(new JObject(new JProperty("name", "fontColor"), new JProperty("value", "#FFFFFF")));
                        JsonList.Add(new JObject(new JProperty("name", "fontColor2"), new JProperty("value", "#cdcdcd")));

                        using (FileWriter = File.CreateText(path))
                        {
                            Newtonsoft.Json.JsonSerializer Serializer = new();
                            Serializer.Serialize(FileWriter, JsonList);
                        }
                        FileWriter.Close();

                        SettingList.Add(new Setting() { name = "height", value = 60 });
                        SettingList.Add(new Setting() { name = "fontSize", value = 2 });
                        SettingList.Add(new Setting() { name = "themeColor", value = "#1c2026" });
                        SettingList.Add(new Setting() { name = "themeColor2", value = "#30343a" });
                        SettingList.Add(new Setting() { name = "fontColor", value = "#FFFFFF" });
                        SettingList.Add(new Setting() { name = "fontColor2", value = "#cdcdcd" });

                        height = 60;
                        fontSize = 2;
                        themeColor = "#1c2026";
                        themeColor2 = "#30343a";
                        fontColor = "#FFFFFF";
                        fontColor2 = "#cdcdcd";
                    }
                }
            }
        }

        public static void SaveSettings()
        {
            JsonList.Clear();
            JsonList.Add(new JObject(new JProperty("name", "height"), new JProperty("value", height)));
            JsonList.Add(new JObject(new JProperty("name", "fontSize"), new JProperty("value", fontSize)));
            JsonList.Add(new JObject(new JProperty("name", "themeColor"), new JProperty("value", themeColor)));
            JsonList.Add(new JObject(new JProperty("name", "themeColor2"), new JProperty("value", themeColor2)));
            JsonList.Add(new JObject(new JProperty("name", "fontColor"), new JProperty("value", fontColor)));
            JsonList.Add(new JObject(new JProperty("name", "fontColor2"), new JProperty("value", fontColor2)));

            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
            FileWriter.Close();
        }

        public static void LoadSettings()
        {
            if (SettingList.Find(x => x.name.Contains("height")) != null &&
                SettingList.Find(x => x.name.Contains("fontSize")) != null &&
                SettingList.Find(x => x.name.Contains("themeColor")) != null &&
                SettingList.Find(x => x.name.Contains("themeColor2")) != null &&
                SettingList.Find(x => x.name.Contains("fontColor")) != null &&
                SettingList.Find(x => x.name.Contains("fontColor2")) != null)
            {
                height = SettingList.Find(x => x.name.Contains("height")).value;
                fontSize = (int)SettingList.Find(x => x.name.Contains("fontSize")).value;
                themeColor = SettingList.Find(x => x.name.Contains("themeColor")).value;
                themeColor2 = SettingList.Find(x => x.name.Contains("themeColor2")).value;
                fontColor = SettingList.Find(x => x.name.Contains("fontColor")).value;
                fontColor2 = SettingList.Find(x => x.name.Contains("fontColor2")).value;
            }
            else
            {
                LoadDefault();
            }
        }

        public static void LoadDefault()
        {
            JsonList.Clear();
            JsonList.Add(new JObject(new JProperty("name", "height"), new JProperty("value", 60)));
            JsonList.Add(new JObject(new JProperty("name", "fontSize"), new JProperty("value", 2)));
            JsonList.Add(new JObject(new JProperty("name", "themeColor"), new JProperty("value", "#1c2026")));
            JsonList.Add(new JObject(new JProperty("name", "themeColor2"), new JProperty("value", "#30343a")));
            JsonList.Add(new JObject(new JProperty("name", "fontColor"), new JProperty("value", "#FFFFFF")));
            JsonList.Add(new JObject(new JProperty("name", "fontColor2"), new JProperty("value", "#cdcdcd")));

            SettingList.Clear();
            SettingList.Add(new Setting() { name = "height", value = 60 });
            SettingList.Add(new Setting() { name = "fontSize", value = 2 });
            SettingList.Add(new Setting() { name = "themeColor", value = "#1c2026" });
            SettingList.Add(new Setting() { name = "themeColor2", value = "#30343a" });
            SettingList.Add(new Setting() { name = "fontColor", value = "#FFFFFF" });
            SettingList.Add(new Setting() { name = "fontColor2", value = "#cdcdcd" });

            height = 60;
            fontSize = 2;
            themeColor = "#1c2026";
            themeColor2 = "#30343a";
            fontColor = "#FFFFFF";
            fontColor2 = "#cdcdcd";

            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
            FileWriter.Close();
        }

    }

    
}
