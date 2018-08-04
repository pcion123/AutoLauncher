#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.Net;
	using System.IO;
	using AutoLauncher.Enum;
	using Tools = AutoLauncher.Utility.Tools;

	public static class FTP : object
	{
		//FTP檔頭
		private const string HEADER = "ftp://";

		//檢查上傳路徑
		private static bool CheckOutputPath(string path)
		{
			if (path == "")
				return false;

			if (!path.Contains(Application.dataPath + "/" + Setting.OutputAssetsFolder))
				return false;

			return true;
		}

		//檢查是否需要上傳
		private static bool CheckNeedUpload(rRes data, List<rRes> list)
		{
			if (list == null)
				return true;

			for (int i = 0; i < list.Count; i++)
			{
				if (data.Path != list[i].Path)
					continue;

				if (data.FileName != list[i].FileName)
					continue;

				if (data.MD5Code == list[i].MD5Code)
					return false;
				else
					return true;
			}

			return true;
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

		//檢查FTP是否有該目錄
		private static bool CheckDirExist(string user, string pwd, string path)
		{
			System.Uri uri = new System.Uri(path);
			try
			{
				// Get the object used to communicate with the server.
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
				request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
				request.Timeout = 60000;

				// This example assumes the FTP site uses anonymous logon.
				request.Credentials = new NetworkCredential(user, pwd);

				FtpWebResponse response = (FtpWebResponse)request.GetResponse();
				response.Close();

				System.Threading.Thread.Sleep(1000);

				return true;
			}
			catch (WebException e)
			{
				Debug.LogWarning(e.Message);
				return false;
			}
		}

		//取得FTP路徑
		public static string GetLocation(string ip)
		{
			switch (Setting.BuildType)
			{
				case BuildTarget.StandaloneWindows:
					return HEADER + ip + "/Windows/";
				case BuildTarget.iOS:
					return HEADER + ip + "/Ios/";
				case BuildTarget.Android:
					return HEADER + ip + "/Android/";
			}
			return string.Empty;
		}

		//取得語系路徑
		private static string GetLangPath(eLanguage lang, bool isSlash = false)
		{
			return isSlash ? lang.ToString() + "/" : lang.ToString();
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

		//取得上傳列表
		private static List<rRes> GetUploadList(eLanguage lang)
		{
			List<rRes> list = new List<rRes>();
			byte[] data = null;
			rRes[] resData = null;
			string json = string.Empty;
			string path = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetLangPath(lang, true) + "Versions/";
			if (Setting.VersionItems == null || Setting.VersionItems.Count == 0)
			{
				data = Tools.Load(path, "Main.res", Setting.EncryptionKeyValue);
				if (data != null)
				{
					json = System.Text.UTF8Encoding.UTF8.GetString(data);
					resData = Tools.DeserializeObject<rRes[]>(json);
					for (int i = 0; i < resData.Length; i++)
						list.Add(resData[i]);
				}
			}
			else
			{
				for (int i = 0; i < Setting.VersionItems.Count; i++)
				{
					data = Tools.Load(path, Setting.VersionItems[i].ver, Setting.EncryptionKeyValue);
					if (data != null)
					{
						json = System.Text.UTF8Encoding.UTF8.GetString(data);
						resData = Tools.DeserializeObject<rRes[]>(json);
						for (int j = 0; j < resData.Length; j++)
							list.Add(resData[j]);
					}
				}
			}
			return list;
		}

		//取得下載列表
		private static List<rRes> GetDownloadList(eLanguage lang)
		{
			List<rRes> list = new List<rRes>();
			byte[] data = null;
			rRes[] resData = null;
			string json = string.Empty;
			string path = Application.dataPath + "/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(lang, true) + "Versions/";
			if (Setting.VersionItems == null || Setting.VersionItems.Count == 0)
			{
				data = Tools.Load(path, "Main.res", Setting.EncryptionKeyValue);
				if (data != null)
				{
					json = System.Text.UTF8Encoding.UTF8.GetString(data);
					resData = Tools.DeserializeObject<rRes[]>(json);
					for (int i = 0; i < resData.Length; i++)
						list.Add(resData[i]);
				}
			}
			else
			{
				for (int i = 0; i < Setting.VersionItems.Count; i++)
				{
					data = Tools.Load(path, Setting.VersionItems[i].ver, Setting.EncryptionKeyValue);
					if (data != null)
					{
						json = System.Text.UTF8Encoding.UTF8.GetString(data);
						resData = Tools.DeserializeObject<rRes[]>(json);
						for (int j = 0; j < resData.Length; j++)
							list.Add(resData[j]);
					}
				}
			}
			return list;
		}

		//建立FTP目錄
		private static void MakeDir(string user, string pwd, string path) 
		{
			string name = Path.GetFileName(path);
			path = path.Replace(name, "");
			if (path.EndsWith("/"))
				path = path.Remove(path.Length - 1);

			string data = path.Replace(GetLocation(Setting.IP), "");
			string[] strs = data.Split('/');

			for (int i = 0; i < strs.Length; i++)
			{
				string zPath = GetLocation(Setting.IP);
				for (int j = 0; j <= i; j++)
					zPath = zPath + strs[j] + "/";

				if (CheckDirExist(user, pwd, zPath))
					continue;

				if (zPath.EndsWith("/"))
					zPath = zPath.Remove(zPath.Length - 1);

				System.Uri uri = new System.Uri(zPath);
				try
				{
					// Get the object used to communicate with the server.
					FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
					request.Method = WebRequestMethods.Ftp.MakeDirectory;
					request.Timeout = 60000;

					// This example assumes the FTP site uses anonymous logon.
					request.Credentials = new NetworkCredential(user, pwd);

					FtpWebResponse response = (FtpWebResponse)request.GetResponse();
					response.Close();

					System.Threading.Thread.Sleep(1000);
				}
				catch (WebException e)
				{
					Debug.LogError(string.Format("Make Path: {0} Err! WebException: {1}", path, e.ToString()));
				}
			}
		}

		//上傳檔案到FTP
		private static void Upload(string user, string pwd, string path, byte[] data)
		{
			if (!CheckDirExist(user, pwd, path))
				MakeDir(user, pwd, path);

			System.Uri uri = new System.Uri(path);
			try
			{
				// Get the object used to communicate with the server.
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
				request.Method = WebRequestMethods.Ftp.UploadFile;
				request.Timeout = 60000;

				// This example assumes the FTP site uses anonymous logon.
				request.Credentials = new NetworkCredential(user, pwd);

				// Copy the contents of the file to the requestrequest stream.
				request.ContentLength = data.Length;
				Stream requestStream = request.GetRequestStream();

				requestStream.Write(data, 0, data.Length);
				requestStream.Close();

				FtpWebResponse response = (FtpWebResponse)request.GetResponse();

				Debug.Log(string.Format("Upload {0} complete status {1}", path, response.StatusDescription));

				response.Close();
			}
			catch (WebException e)
			{
				throw new System.Exception(e.Message);
			}
		}

		//處理上傳資料
		public static void HandleUpload(string path, bool isShow = true)
		{
			string zPath = Application.dataPath + "/" + path.Remove(0, 7);
			string user = Setting.User;
			string pwd = Setting.PWD;
			string ip = Setting.IP;
			byte[] data;

			if (CheckOutputPath(zPath) == false)
			{
				if (isShow == true)
					EditorUtility.DisplayDialog("Upload", "Check your path!", "OK");
				return;
			}

			try
			{
				if (CheckIsFolder(zPath) == true)
				{
					string[] files = Directory.GetFiles(zPath, "*.*", SearchOption.AllDirectories);

					for (int i = 0; i < files.Length; i++)
					{
						zPath = files[i].Replace("\\", "/");

						if (Path.GetExtension(zPath) == ".meta")
							continue;

						if (Path.GetExtension(zPath) == ".crc")
							continue;

						EditorUtility.DisplayProgressBar(string.Format("Uploading {0}/{1}", i + 1, files.Length), GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/"), (float)(i + 1 / files.Length));

						data = File.ReadAllBytes(zPath);
						zPath = GetLocation(ip) + GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

						Upload(user, pwd, zPath, data);
					}
				}
				else
				{
					zPath = zPath.Replace("\\", "/");

					if (Path.GetExtension(zPath) == ".meta")
						return;

					if (Path.GetExtension(zPath) == ".crc")
						return;

					EditorUtility.DisplayProgressBar(string.Format("Uploading {0}/{1}", 1, 1), GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/"), 0f);

					data = File.ReadAllBytes(zPath);
					zPath = GetLocation(ip) + GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

					Upload(user, pwd, zPath, data);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
			}

			EditorUtility.ClearProgressBar();

			if (isShow == true)
				EditorUtility.DisplayDialog("Upload", "Upload all complete!", "OK");
		}

		//處理上傳資料
		public static void HandleUploadEx(eLanguage lang, bool isShow = true)
		{
			string user = Setting.User;
			string pwd = Setting.PWD;
			string ip = Setting.IP;

			List<rRes> list1 = GetUploadList(lang);
			List<rRes> list2 = GetDownloadList(lang);

			for (int i = 0; i < list1.Count; i++)
			{
				string zPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + list1[i].Path + list1[i].FileName;

				if (CheckNeedUpload(list1[i], list2) == false)
					continue;

				byte[] data = File.ReadAllBytes(zPath);

				zPath = GetLocation(ip) + GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

				try
				{
					Upload(user, pwd, zPath, data);
				}
				catch (System.Exception e)
				{
					Debug.LogError("Upload " + zPath + " Err! " + e.Message);
					return;
				}
			}

			HandleUpload(string.Format("Assets/{0}/{1}", Setting.OutputAssetsFolder, GetLangPath(lang, true) + "Versions/"), false);

			if (isShow)
				EditorUtility.DisplayDialog("Upload", "Upload all complete!", "OK");
		}
	}
}
#endif
