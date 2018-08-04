#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using System.IO;
	using System.Text;
	using System.Security.Cryptography;
	using Tools = AutoLauncher.Utility.Tools;

	public static class VersionBuilder : object
	{
		private static bool CheckFileExtension(FileInfo info)
		{
			if (info.Extension == ".res")
				return false;

			if (info.Extension == ".meta")
				return false;

			if (info.Extension == ".crc")
				return false;

			return true;
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

		//取得檔案的MD5編碼
		private static string GetMd5File(string path, string name)
		{
			FileInfo info = new FileInfo(path + name);
			if ((info.Extension == ".unity3d") || (info.Extension == ".zip"))
				return GetMd5Unity(path, "." + name);

			byte[] data = Tools.Load(path, name);
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			data = md5.ComputeHash(data);
			StringBuilder sb = new StringBuilder(32);
			for (int i = 0; i < data.Length; i++)
				sb.Append(data[i].ToString("x").PadLeft(2, '0'));

			return sb.ToString();
		}

		//取得Bundle的MD5編號
		private static string GetMd5Unity(string path, string name)
		{
			FileInfo info = new FileInfo(path + name);
			byte[] data = Tools.Load(path, name.Replace(info.Extension, ".crc"));

			if (data == null)
				return string.Empty;

			//取出Json字串
			string json = System.Text.UTF8Encoding.UTF8.GetString(data);
			//Json反序列化
			rCRC crc = Tools.DeserializeObject<rCRC>(json);
			return crc.CRC.ToString();
		}

		private static void BuildVersion(string lang, string path, string versionName)
		{
			//檢查目錄是否存在
			if (Directory.Exists(path) == true)
			{
				List<rRes> container = new List<rRes>();
				//取得目錄底下的Data檔案
				string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
				for (int i = 0; i < files.Length; i++)
				{
					FileInfo info = new FileInfo(files[i]);
					if (!CheckFileExtension(info))
						continue;

					string filePath = Path.GetDirectoryName(files[i]).Replace("\\", "/");
					rRes data = new rRes();
					data.Version = Setting.Ver;
					data.FileName = info.Name;
					data.Path = GetDataPath(filePath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");
					data.FileSize = info.Length;
					data.MD5Code = GetMd5File(filePath + "/", info.Name);
					container.Add(data);
				}
				//整合資料
				rRes[] resData = container.ToArray();
				//轉成Json
				string json = Tools.SerializeObject(resData);
				//寫入檔案
				Tools.Save(Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + lang + "/" + "Versions" + "/", versionName, System.Text.UTF8Encoding.UTF8.GetBytes(json));
			}
		}

		//處理ResData
		public static void HandleVersion(string lang, bool isShow = true) 
		{
			if (Setting.VersionItems == null || Setting.VersionItems.Count == 0)
			{
				string path = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + lang + "/";
				string name = "Main.res";

				if (path == "")
				{
					if (isShow == true)
						EditorUtility.DisplayDialog("VersionData", "Build VersionData Path Err!", "OK");
					return;
				}

				if (name == "")
				{
					if (isShow == true)
						EditorUtility.DisplayDialog("VersionData", "Build VersionData Name Err!", "OK");
					return;
				}

				BuildVersion(lang, path, name);

				if (isShow == true)
					EditorUtility.DisplayDialog("VersionData", "Build " + name + " complete!", "OK");
				
				Debug.Log(string.Format("{0} build over", name));
			}
			else
			{
				for (int i = 0; i < Setting.VersionItems.Count; i++)
				{
					string[] paths = Setting.VersionItems[i].value.Split('/');
					string path = Application.dataPath + "/" + string.Format(string.Join("/", paths, 1, paths.Length - 1), lang);
					string name = Setting.VersionItems[i].ver;

					if (path == "")
					{
						if (isShow == true)
							EditorUtility.DisplayDialog("VersionData", "Build VersionData Path Err!", "OK");
						return;
					}

					if (name == "")
					{
						if (isShow == true)
							EditorUtility.DisplayDialog("VersionData", "Build VersionData Name Err!", "OK");
						return;
					}

					BuildVersion(lang, path, name);
					Debug.Log(string.Format("{0} build over", name));
				}

				if (isShow == true)
					EditorUtility.DisplayDialog("VersionData", "Build all complete!", "OK");
			}
			AssetDatabase.Refresh();
		}
	}
}
#endif
