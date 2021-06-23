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
        static string path;
        private static StreamWriter FileWriter;
        public static List<Setting> SettingList = new();
        public static List<Newtonsoft.Json.Linq.JObject> JsonList = new();
        static SettingsVariables()
        {
            LoadSettings();
        }
        
        public static void LoadSettings()
        {
            path = Environment.CurrentDirectory + "\\Settings.json";
            if (!File.Exists(path))
            {
                FileWriter = File.CreateText(path);

                //JsonList.Add(new Setting() { name = "height", value = 60 });
                using (FileWriter = File.CreateText(path))
                {
                    Newtonsoft.Json.JsonSerializer Serializer = new();
                    Serializer.Serialize(FileWriter, JsonList);
                }
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
                        
                        height = SettingList.Find(x => x.name.Contains("height")).value;
                    }
                    else
                    {
                        Debug.WriteLine("Empty");
                        Newtonsoft.Json.Linq.JObject SettingJson = new(
                            new Newtonsoft.Json.Linq.JProperty("name", "height"),
                            new Newtonsoft.Json.Linq.JProperty("value", 60));

                        JsonList.Add(SettingJson);
                        Debug.WriteLine(SettingJson);
                        using (FileWriter = File.CreateText(path))
                        {
                            Newtonsoft.Json.JsonSerializer Serializer = new();
                            Serializer.Serialize(FileWriter, JsonList);
                        }
                        Debug.WriteLine("Hej!");
                        SettingList.Add(new Setting() { name = "height", value = 60 });
                        
                        height = SettingList.Find(x => x.name.Contains("height")).value;
                        Debug.WriteLine("Height is set to: " + height);
                    }
                }
            }
        }

        public static void SaveSettings(double height)
        {
            Newtonsoft.Json.Linq.JObject SettingJson_Height = new(
                            new Newtonsoft.Json.Linq.JProperty("name", "height"),
                            new Newtonsoft.Json.Linq.JProperty("value", height));

            JsonList.Clear();
            JsonList.Add(SettingJson_Height);
            using (FileWriter = File.CreateText(path))
            {
                Newtonsoft.Json.JsonSerializer Serializer = new();
                Serializer.Serialize(FileWriter, JsonList);
            }
        }

    }

    
}
