#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace AutoLauncher
{
	public static class FTP : object
	{
		//FTP檔頭
		private const string HEADER = "ftp://";

		//檢查上傳路徑
		private static bool CheckOutputPath (string vPath)
		{
			if (vPath == "")
				return false;

			if (vPath.Contains(Application.dataPath + "/" + Setting.OutputAssetsFolder) == false)
				return false;

			return true;
		}

		//檢查是否需要上傳
		private static bool CheckNeedUpload (rResData vData, List<rResData> vList)
		{
			if (vList == null)
				return true;

			for (int i = 0; i < vList.Count; i++)
			{
				if (vData.Path != vList[i].Path)
					continue;

				if (vData.FileName != vList[i].FileName)
					continue;

				if (vData.MD5Code == vList[i].MD5Code)
					return false;
				else
					return true;
			}

			return true;
		}

		//檢查是否是資料夾
		private static bool CheckIsFolder (string vPath)
		{
			FileAttributes vAttr = File.GetAttributes(@vPath);
			if ((vAttr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}

		//檢查FTP是否有該目錄
		private static bool CheckDirExist (string vUser, string vPWD, string vPath)
		{
			System.Uri vUri = new System.Uri(vPath);
			try
			{
				// Get the object used to communicate with the server.
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(vUri);
				request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;
				request.Timeout = 60000;

				// This example assumes the FTP site uses anonymous logon.
				request.Credentials = new NetworkCredential(vUser, vPWD);

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
		public static string GetLocation (string vIP)
		{
			switch (Setting.BuildType)
			{
				case BuildTarget.StandaloneWindows:
					return HEADER + vIP + "/Windows/";
				case BuildTarget.iOS:
					return HEADER + vIP + "/Ios/";
				case BuildTarget.Android:
					return HEADER + vIP + "/Android/";
			}
			return string.Empty;
		}

		//取得語系路徑
		private static string GetLangPath (eLanguage vLang, bool vIsSlash = false)
		{
			return vIsSlash ? vLang.ToString() + "/" : vLang.ToString();
		}

		//取得資料路徑
		private static string GetDataPath (string vPath, string vReplace)
		{
			string vDataPath = vPath.Replace(vReplace, "");
			string vFileName = Path.GetFileName(vPath);
			string vExtension = Path.GetExtension(vPath);
			if (vExtension != "")
			{
				string vFolder = vFileName.Replace(vExtension, "") + "/";
				vDataPath = vDataPath.Replace(vFolder + vFileName, "");
			}
			else
			{
				vDataPath = vDataPath + "/";
			}
			return vDataPath;
		}

		//取得上傳列表
		private static List<rResData> GetUploadList (eLanguage vLang)
		{
			List<rResData> vList = new List<rResData>();
			byte[] vData = null;
			rResData[] vResData = null;
			string vJson = string.Empty;

			string vPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetLangPath(vLang, true) + "Versions/";

			vData = Tools.Load(vPath, "Version.res", Setting.EncryptionKeyValue);

			if (vData != null)
			{
				vJson = System.Text.UTF8Encoding.UTF8.GetString(vData);
				vResData = Tools.DeserializeObject<rResData[]>(vJson);

				for (int i = 0; i < vResData.Length; i++)
					vList.Add(vResData[i]);
			}

			return vList;
		}

		//取得下載列表
		private static List<rResData> GetDownloadList (eLanguage vLang)
		{
			List<rResData> vList = new List<rResData>();
			byte[] vData = null;
			rResData[] vResData = null;
			string vJson = string.Empty;

			string vPath = Application.dataPath + "/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(vLang, true) + "Versions/";

			vData = Tools.Load(vPath, "Version.res");

			if (vData != null)
			{
				vJson = System.Text.UTF8Encoding.UTF8.GetString(vData);
				vResData = Tools.DeserializeObject<rResData[]>(vJson);

				for (int i = 0; i < vResData.Length; i++)
					vList.Add(vResData[i]);
			}

			return vList;
		}

		//建立FTP目錄
		private static void MakeDir (string vUser, string vPWD, string vPath) 
		{
			string vName = Path.GetFileName(vPath);

			vPath = vPath.Replace(vName, "");

			if (vPath.EndsWith("/") == true)
			{
				vPath = vPath.Remove(vPath.Length - 1);
			}

			string vData = vPath.Replace(GetLocation(Setting.IP), "");
			string[] vStrAry = vData.Split('/');

			for (int i = 0; i < vStrAry.Length; i++)
			{
				string zPath = GetLocation(Setting.IP);

				for (int j = 0; j <= i; j++)
					zPath = zPath + vStrAry[j] + "/";

				if (CheckDirExist(vUser, vPWD, zPath) == true)
					continue;

				if (zPath.EndsWith("/") == true)
					zPath = zPath.Remove(zPath.Length - 1);

				System.Uri vUri = new System.Uri(zPath);
				try
				{
					// Get the object used to communicate with the server.
					FtpWebRequest request = (FtpWebRequest)WebRequest.Create(vUri);
					request.Method = WebRequestMethods.Ftp.MakeDirectory;
					request.Timeout = 60000;

					// This example assumes the FTP site uses anonymous logon.
					request.Credentials = new NetworkCredential(vUser, vPWD);

					FtpWebResponse response = (FtpWebResponse)request.GetResponse();
					response.Close();

					System.Threading.Thread.Sleep(1000);
				}
				catch (WebException e)
				{
					Debug.LogError(string.Format("Make Path: {0} Err! WebException: {1}", vPath, e.ToString()));
				}
			}
		}

		//上傳檔案到FTP
		private static void Upload (string vUser, string vPWD, string vPath, byte[] vData)
		{
			if (CheckDirExist(vUser, vPWD, vPath) == false)
				MakeDir(vUser, vPWD, vPath);

			System.Uri vUri = new System.Uri(vPath);
			try
			{
				// Get the object used to communicate with the server.
				FtpWebRequest request = (FtpWebRequest)WebRequest.Create(vUri);
				request.Method = WebRequestMethods.Ftp.UploadFile;
				request.Timeout = 60000;

				// This example assumes the FTP site uses anonymous logon.
				request.Credentials = new NetworkCredential(vUser, vPWD);

				// Copy the contents of the file to the requestrequest stream.
				request.ContentLength = vData.Length;
				Stream requestStream = request.GetRequestStream();

				requestStream.Write(vData, 0, vData.Length);
				requestStream.Close();

				FtpWebResponse response = (FtpWebResponse)request.GetResponse();

				Debug.Log(string.Format("Upload {0} complete status {1}", vPath, response.StatusDescription));

				response.Close();
			}
			catch (WebException e)
			{
				throw new System.Exception(e.Message);
			}
		}

		//處理上傳資料
		public static void HandleUpload (string vPath, bool vIsShow = true)
		{
			string zPath = Application.dataPath + "/" + vPath.Remove(0, 7);
			string zUser = Setting.User;
			string zPWD = Setting.PWD;
			string zIP = Setting.IP;
			byte[] zData;

			if (CheckOutputPath(zPath) == false)
			{
				if (vIsShow == true)
					EditorUtility.DisplayDialog("Upload", "Check your path!", "OK");
				return;
			}

			try
			{
				if (CheckIsFolder(zPath) == true)
				{
					string[] vFile = Directory.GetFiles(zPath, "*.*", SearchOption.AllDirectories);

					for (int i = 0; i < vFile.Length; i++)
					{
						zPath = vFile[i].Replace("\\", "/");

						if (Path.GetExtension(zPath) == ".meta")
							continue;

						if (Path.GetExtension(zPath) == ".crc")
							continue;

						EditorUtility.DisplayProgressBar(string.Format("Uploading {0}/{1}", i + 1, vFile.Length), GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/"), (float)(i + 1 / vFile.Length));

						zData = File.ReadAllBytes(zPath);
						zPath = GetLocation(zIP) + GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

						Upload(zUser, zPWD, zPath, zData);
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

					zData = File.ReadAllBytes(zPath);
					zPath = GetLocation(zIP) + GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

					Upload(zUser, zPWD, zPath, zData);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError(e.Message);
			}

			EditorUtility.ClearProgressBar();

			if (vIsShow == true)
				EditorUtility.DisplayDialog("Upload", "Upload all complete!", "OK");
		}

		//處理上傳資料
		public static void HandleUploadEx (eLanguage vLang, bool vIsShow = true)
		{
			string zUser = Setting.User;
			string zPWD = Setting.PWD;
			string zIP = Setting.IP;

			List<rResData> vList1 = GetUploadList(vLang);
			List<rResData> vList2 = GetDownloadList(vLang);

			for (int i = 0; i < vList1.Count; i++)
			{
				string zPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + vList1[i].Path + vList1[i].FileName;

				if (CheckNeedUpload(vList1[i], vList2) == false)
					continue;

				byte[] zData = File.ReadAllBytes(zPath);

				zPath = GetLocation(zIP) + GetDataPath(zPath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

				try
				{
					Upload(zUser, zPWD, zPath, zData);
				}
				catch (System.Exception e)
				{
					Debug.LogError("Upload " + zPath + " Err! " + e.Message);
					return;
				}
			}

			HandleUpload(string.Format("Assets/{0}/{1}", Setting.OutputAssetsFolder, GetLangPath(vLang, true) + "Versions/"), false);

			if (vIsShow == true)
				EditorUtility.DisplayDialog("Upload", "Upload all complete!", "OK");
		}
	}
}
#endif
