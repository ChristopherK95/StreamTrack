using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace WpfApp1
{
    static class SettingsVariables
    {

        public static double height;
        public static int fontSize;
        public static string themeColor;
        public static string themeColor2;
        public static string fontColor;
        public static string fontColor2;
        public static string authKey = "empty";
        private static string path;

        private static StreamWriter FileWriter;
        public static List<Setting> SettingList = new();
        public static List<JObject> JsonList = new();
        static SettingsVariables()
        {
            SettingsStartUp();
        }

        public static void SettingsStartUp()
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TwitchTrack");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TwitchTrack", "Settings.json");
            if (!File.Exists(path))
            {
                LoadDefault();
            }
            else
            {
                using StreamReader fileReader = File.OpenText(path);
                string jsonString = fileReader.ReadToEnd();
                fileReader.Close();
                if (jsonString != "empty")
                {
                    SettingList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Setting>>(jsonString);
                    if (SettingList != null && SettingList.Count == 7)
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
                    JsonList.Add(new JObject(new JProperty("name", "fontColor"), new JProperty("value", "#ffffff")));
                    JsonList.Add(new JObject(new JProperty("name", "fontColor2"), new JProperty("value", "#cdcdcd")));
                    JsonList.Add(new JObject(new JProperty("name", "authKey"), new JProperty("value", "empty")));

                    using (FileWriter = File.CreateText(path))
                    {
                        Newtonsoft.Json.JsonSerializer Serializer = new();
                        Serializer.Serialize(FileWriter, JsonList);
                    }
                    FileWriter.Close();

                    SettingList.Add(new Setting() { name = "height", value = 65 });
                    SettingList.Add(new Setting() { name = "fontSize", value = 2 });
                    SettingList.Add(new Setting() { name = "themeColor", value = "#1c2026" });
                    SettingList.Add(new Setting() { name = "themeColor2", value = "#30343a" });
                    SettingList.Add(new Setting() { name = "fontColor", value = "#ffffff" });
                    SettingList.Add(new Setting() { name = "fontColor2", value = "#cdcdcd" });
                    SettingList.Add(new Setting() { name = "authKey", value = "empty" });

                    height = 65;
                    fontSize = 2;
                    themeColor = "#1c2026";
                    themeColor2 = "#30343a";
                    fontColor = "#ffffff";
                    fontColor2 = "#cdcdcd";
                    authKey = "empty";
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
            JsonList.Add(new JObject(new JProperty("name", "authKey"), new JProperty("value", authKey)));

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
                SettingList.Find(x => x.name.Contains("fontColor2")) != null &&
                SettingList.Find(x => x.name.Contains("authKey")) != null)
            {
                height = SettingList.Find(x => x.name.Contains("height")).value;
                fontSize = (int)SettingList.Find(x => x.name.Contains("fontSize")).value;
                themeColor = SettingList.Find(x => x.name.Contains("themeColor")).value;
                themeColor2 = SettingList.Find(x => x.name.Contains("themeColor2")).value;
                fontColor = SettingList.Find(x => x.name.Contains("fontColor")).value;
                fontColor2 = SettingList.Find(x => x.name.Contains("fontColor2")).value;
                authKey = SettingList.Find(x => x.name.Contains("authKey")).value;
            }
            else
            {
                LoadDefault();
            }
        }

        public static void LoadDefault()
        {
            JsonList.Clear();
            JsonList.Add(new JObject(new JProperty("name", "height"), new JProperty("value", 65)));
            JsonList.Add(new JObject(new JProperty("name", "fontSize"), new JProperty("value", 2)));
            JsonList.Add(new JObject(new JProperty("name", "themeColor"), new JProperty("value", "#1c2026")));
            JsonList.Add(new JObject(new JProperty("name", "themeColor2"), new JProperty("value", "#30343a")));
            JsonList.Add(new JObject(new JProperty("name", "fontColor"), new JProperty("value", "#ffffff")));
            JsonList.Add(new JObject(new JProperty("name", "fontColor2"), new JProperty("value", "#cdcdcd")));
            JsonList.Add(new JObject(new JProperty("name", "authKey"), new JProperty("value", "empty")));

            if (SettingList != null)
            {
                SettingList.Clear();
            }
            else
            {
                SettingList = new();
            }
            SettingList.Add(new Setting() { name = "height", value = 65 });
            SettingList.Add(new Setting() { name = "fontSize", value = 2 });
            SettingList.Add(new Setting() { name = "themeColor", value = "#1c2026" });
            SettingList.Add(new Setting() { name = "themeColor2", value = "#30343a" });
            SettingList.Add(new Setting() { name = "fontColor", value = "#ffffff" });
            SettingList.Add(new Setting() { name = "fontColor2", value = "#cdcdcd" });
            SettingList.Add(new Setting() { name = "authKey", value = "empty" });

            height = 65;
            fontSize = 2;
            themeColor = "#1c2026";
            themeColor2 = "#30343a";
            fontColor = "#ffffff";
            fontColor2 = "#cdcdcd";
            authKey = "empty";

            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
            FileWriter.Close();
        }
    }
}
