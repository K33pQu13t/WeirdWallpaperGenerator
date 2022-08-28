using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WeirdWallpaperGenerator.Helpers
{
    public static class DescriptionHelper
    {
        public static string GetDescription<T>(string fieldName)
        {
            try
            {
                FieldInfo fieldInfo = typeof(T).GetField(
                    fieldName, 
                    BindingFlags.Instance | 
                    BindingFlags.Public | 
                    BindingFlags.NonPublic | 
                    BindingFlags.Static);
                object[] descriptionAttrs;
                if (fieldInfo == null)
                {
                    descriptionAttrs = typeof(T).GetCustomAttributes(typeof(DescriptionAttribute), false);
                }
                else
                {
                    descriptionAttrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                }
                DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs.First();
                return description.Description;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
