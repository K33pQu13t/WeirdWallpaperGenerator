using System;
using System.Collections.Generic;
using System.Text;

namespace WeirdWallpaperGenerator.Services.Serialization
{
    public interface ISerializationService
    {
        public void Serialize(string filePath, object obj);
        public object Deserialize(string filePath);
    }
}
