using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WeirdWallpaperGenerator.Services.Serialization
{
    class JsonSerializationService
    {
        string _dateFormatString => "dd.MM.yyyy";

        public object Deserialize(string filePath, Type type)
        {
            using (StreamReader file = File.OpenText(Path.GetFullPath(filePath)))
            {
                JsonSerializer serializer = new JsonSerializer() 
                {
                    DateFormatString = _dateFormatString
                };
                return serializer.Deserialize(file, type);
            }
        }

        public void Serialize(string filePath, object obj)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                var settings = new JsonSerializerSettings()
                {
                    DateFormatString = _dateFormatString,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                    Formatting = Formatting.Indented
                    //NullValueHandling = NullValueHandling.Ignore
                };
                string json = JsonConvert.SerializeObject(obj, settings);
                fs.Write(new UTF8Encoding(true).GetBytes(json));
            }
        }
    }
}
