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
		public static string GetSHA1Checksum(string filePath)
        {

			using (var sha1 = SHA1.Create())
			{
				using (var stream = File.OpenRead(filePath))
				{
					var hash = sha1.ComputeHash(stream);
					return BitConverter.ToString(hash).Replace("-", "").ToLower();
				}
			}
		}

		/// <summary>
		/// the issue is what github places some information to content's byte array
		/// https://alblue.bandlem.com/2011/08/git-tip-of-week-objects.html
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns>sha1 to compare with github api</returns>
		public static string GetSHA1ChecksumGithub(string filePath)
        {
			using (var sha1 = SHA1.Create())
			{
				using (var stream = File.OpenRead(filePath))
				{
					MemoryStream memoryStream = new MemoryStream();
					stream.CopyTo(memoryStream);
					byte[] bytes = memoryStream.ToArray();

					var hash = sha1.ComputeHash(
						Encoding.UTF8.GetBytes($"blob {bytes.Length}{char.MinValue}").Concat(bytes).ToArray());
					return BitConverter.ToString(hash).Replace("-", "").ToLower();
				}
			}
		}

		//public static string GetSHA1ChecksumFromFolder(string folderPath, string[] excludeFiles = null)
  //      {
		//	string[] paths = Directory.GetFiles(folderPath);
		//	if (excludeFiles != null)
  //          {
		//		paths = paths.Where(x => !excludeFiles.Contains(Path.GetFileName(x))).ToArray();
		//	}

		//	string result = string.Empty;
		//	foreach(var path in paths)
  //          {
		//		result += GetSHA1Checksum(path);
  //          }

		//	using (var sha1 = SHA1.Create())
  //          {
		//		var hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(result));
		//		return BitConverter.ToString(hash).Replace("-", "").ToLower();
		//	}
		//}

		public static Dictionary<string, string> GetSHA1ChecksumFromFolder(string folderPath, string[] excludeFiles = null)
        {
			Dictionary<string, string> checksums = new Dictionary<string, string>();
			string[] files = Directory.GetFiles(folderPath);
			foreach (var file in files)
			{
				string filename = Path.GetFileName(file);
				if (excludeFiles.Contains(filename))
					continue;
				string hash = GetSHA1Checksum(Path.GetFullPath(file));
				checksums.Add(hash, filename);
			}

			return checksums;
		}
	}
}
