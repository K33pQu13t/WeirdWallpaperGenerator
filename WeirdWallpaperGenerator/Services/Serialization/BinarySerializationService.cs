using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WeirdWallpaperGenerator.Services.Serialization
{
    public class BinarySerializationService
    {
        readonly BinaryFormatter _formatter;
        public BinarySerializationService()
        {
            _formatter = new BinaryFormatter();
        }

        public void Serialize(string filePath, object obj)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                _formatter.Serialize(fs, obj);
            }
        }

        public object Deserialize(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return _formatter.Deserialize(fs);

            }
        }
    }
}
