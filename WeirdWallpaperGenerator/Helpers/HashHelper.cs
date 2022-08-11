using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WeirdWallpaperGenerator.Helpers
{
    public static class HashHelper
    {
		public static string GetMD5Checksum(string filename)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(filename))
				{
					var hash = md5.ComputeHash(stream);
					return BitConverter.ToString(hash).Replace("-", "");
				}
			}
		}

		public static string GetMD5ChecksumFromFolder(string folderPath)
        {
			string[] paths = Directory.GetFiles(folderPath).Where(x => Path.GetFileName(x) != "config.json").ToArray();
			string result = string.Empty;
			foreach(var path in paths)
            {
				result += GetMD5Checksum(path);
            }

			using (var md5 = MD5.Create())
            {
				var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(result));
				return BitConverter.ToString(hash).Replace("-", "");
			}
		}
	}
}
