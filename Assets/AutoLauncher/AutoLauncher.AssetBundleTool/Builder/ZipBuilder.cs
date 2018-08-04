#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using System.IO;
	using ICSharpCode.SharpZipLib.Core;
	using ICSharpCode.SharpZipLib.Zip;
	using ICSharpCode.SharpZipLib.Checksums;
	using Tools = AutoLauncher.Utility.Tools;

	public static class ZipBuilder : object
	{
		//檢查包Zip路徑
		private static bool CheckZipPath(string path)
		{
			if (path == string.Empty || path == null)
				return false;

			if (path.Contains(Application.dataPath + "/" + Setting.InputAssetsFolder) == false)
				return false;
			
			string langPath = GetLangPath(path);
			string zipPath = string.Format("{0}/{1}", "Assets", GetDataPath(path, Application.dataPath + "/"));

			for (int i = 0; i < Setting.ZipPathCount; i++)
			{
				if (zipPath == string.Format(Setting.ZipPathItems[i].value, langPath))
					return true;
			}

			return false;
		}

		//檢查是否是資料夾
		private static bool CheckIsFolder(string path)
		{
			FileAttributes attr = File.GetAttributes(@path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}

		//取得資料路徑
		private static string GetDataPath(string path, string replace)
		{
			string dataPath = path.Replace(replace, "");
			string fileName = Path.GetFileName(path);
			string extension = Path.GetExtension(path);
			if (extension != "")
			{
				string folder = fileName.Replace(extension, "") + "/";
				dataPath = dataPath.Replace(folder + fileName, "");
			}
			else
			{
				dataPath = dataPath + "/";
			}
			return dataPath;
		}

		//取得語系路徑
		public static string GetLangPath(string path)
		{
			string[] values = System.Enum.GetNames(typeof(eLanguage));
			foreach (string value in values)
			{
				if (path.Contains(value + "/") == true)
					return value;
			}
			return string.Empty;
		}

		private static void CreateCrcFile(string path, string fileName, uint code)
		{
			string json = Tools.SerializeObject(new rCRC(fileName + ".zip", code));
			Tools.Save(path, "." + fileName + ".crc", System.Text.UTF8Encoding.UTF8.GetBytes(json));
			File.SetAttributes(path + "." + fileName + ".crc", FileAttributes.Hidden);
		}

		private static uint CreateZipFile(string path, string fileName, string pwd, List<string> container)
		{
			Crc32 crc = new Crc32();
			using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(path + fileName + ".zip")))
			{
				zipStream.Password = pwd;
				for (int i = 0; i < container.Count; i++)
				{
					FileInfo info = new FileInfo(container[i]);
					ZipEntry zipEntry = new ZipEntry(Path.GetFileName(container[i]));
					zipEntry.DateTime = info.CreationTime;
					zipStream.PutNextEntry(zipEntry);

					using (FileStream fileStream = File.OpenRead(container[i]))
					{
						byte[] buffer = new byte[fileStream.Length];
						crc.Update(buffer);
						StreamUtils.Copy(fileStream, zipStream, buffer);
						fileStream.Close();
					}
				}
			}
			container.Clear();
			return (uint)crc.Value;
		}

		//建立Zip
		private static void BuildZip(List<string> files, string path, string pwd = "")
		{
			string zipPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetDataPath(path, Application.dataPath + "/" + Setting.InputAssetsFolder + "/");
			string zipName = Path.GetFileNameWithoutExtension(path) + "{0}";

			if (!Directory.Exists(zipPath))
				Directory.CreateDirectory(zipPath);
			
			List<string> container = new List<string>();
			for (int i = 0; i < files.Count; i++)
			{
				container.Add(files[i]);

				if (container.Count != 10)
					continue;

				int count = Mathf.CeilToInt(i / 10f);
				string fileName = string.Format(zipName, count.ToString("D2"));

				uint crc = CreateZipFile(zipPath, fileName, pwd, container);
				CreateCrcFile(zipPath, fileName, crc);
			}

			if (container.Count != 0)
			{
				int count = Mathf.CeilToInt(files.Count / 10f);
				string fileName = string.Format(zipName, count.ToString("D2"));

				uint crc = CreateZipFile(zipPath, fileName, pwd, container);
				CreateCrcFile(zipPath, fileName, crc);
			}
		}

		//處理Zip
		public static void HandleZip(string directPath, BuildTarget target, bool isShow = true)
		{
			string[] paths = directPath.Split('/');
			string path = Application.dataPath + "/" + string.Join("/", paths, 1, paths.Length - 1);
			string tmp = string.Empty;

			//檢查路徑
			if (CheckZipPath(path) == false)
			{
				if (isShow == true)
					EditorUtility.DisplayDialog("HandleZip", "Build error and check your path!", "OK");
				return;
			}

			//檢查該路徑是否為資料夾
			if (CheckIsFolder(path) == true)
			{
				List<string> files = new List<string>();
				for (int i = 0; i < Setting.ZipTypeCount; i++)
				{
					string[] fileArray = Directory.GetFiles(path, Setting.ZipTypeItems[i].value, SearchOption.AllDirectories);
					for (int j = 0; j < fileArray.Length; j++)
					{
						tmp = fileArray[j];
						tmp = tmp.Replace("\\", "/");
						files.Add(tmp);
					}
				}
				BuildZip(files, path);
			}

			if (isShow == true)
				EditorUtility.DisplayDialog("HandleZip", "Build complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
