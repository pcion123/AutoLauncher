#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Tools = AutoLauncher.Utility.Tools;

namespace AutoLauncher.AssetBundleTool
{
	public static class VersionBuilder : object
	{
		private static bool CheckFileExtension (FileInfo vInfo)
		{
			if (vInfo.Extension == ".res")
				return false;

			if (vInfo.Extension == ".meta")
				return false;

			if (vInfo.Extension == ".crc")
				return false;

			return true;
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

		//取得檔案的MD5編碼
		private static string GetMd5File (string vPath, string vName)
		{
			FileInfo vInfo = new FileInfo(vPath + vName);
			if ((vInfo.Extension == ".unity3d") || (vInfo.Extension == ".zip"))
				return GetMd5Unity(vPath, "." + vName);

			byte[] vData = Tools.Load(vPath, vName);

			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

			vData = md5.ComputeHash(vData);

			StringBuilder sb = new StringBuilder(32);

			for (int i = 0; i < vData.Length; i++)
				sb.Append(vData[i].ToString("x").PadLeft(2, '0'));

			return sb.ToString();
		}

		//取得Bundle的MD5編號
		private static string GetMd5Unity (string vPath, string vName)
		{
			FileInfo vInfo = new FileInfo(vPath + vName);

			byte[] vData = Tools.Load(vPath, vName.Replace(vInfo.Extension, ".crc"));

			if (vData == null)
				return string.Empty;

			//取出Json字串
			string vJson = System.Text.UTF8Encoding.UTF8.GetString(vData);

			//Json反序列化
			rCRC vCRC = Tools.DeserializeObject<rCRC>(vJson);

			return vCRC.CRC.ToString();
		}

		private static void BuildVersion (string vLang, string vPath, string vVersionName)
		{
			//檢查目錄是否存在
			if (Directory.Exists(vPath) == true)
			{
				List<rRes> vContainer = new List<rRes>();
				//取得目錄底下的Data檔案
				string[] vFiles = Directory.GetFiles(vPath, "*.*", SearchOption.AllDirectories);

				for (int i = 0; i < vFiles.Length; i++)
				{
					FileInfo vInfo = new FileInfo(vFiles[i]);
					if (!CheckFileExtension(vInfo))
						continue;

					string vFilePath = Path.GetDirectoryName(vFiles[i]).Replace("\\", "/");
					rRes vData = new rRes();
					vData.Version = Setting.Ver;
					vData.FileName = vInfo.Name;
					vData.Path = GetDataPath(vFilePath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");
					vData.FileSize = vInfo.Length;
					vData.MD5Code = GetMd5File(vFilePath + "/", vInfo.Name);
					vContainer.Add(vData);
				}
				//整合資料
				rRes[] vResData = vContainer.ToArray();
				//轉成Json
				string vJson = Tools.SerializeObject(vResData);
				//寫入檔案
				Tools.Save(Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + vLang + "/" + "Versions" + "/", vVersionName, System.Text.UTF8Encoding.UTF8.GetBytes(vJson));
			}
		}

		//處理ResData
		public static void HandleVersion (string vLang, bool vIsShow = true) 
		{
			if (Setting.VersionItems == null || Setting.VersionItems.Count == 0)
			{
				string vPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + vLang + "/";
				string vName = "Main.res";

				if (vPath == "")
				{
					if (vIsShow == true)
						EditorUtility.DisplayDialog("VersionData", "Build VersionData Path Err!", "OK");
					return;
				}

				if (vName == "")
				{
					if (vIsShow == true)
						EditorUtility.DisplayDialog("VersionData", "Build VersionData Name Err!", "OK");
					return;
				}

				BuildVersion(vLang, vPath, vName);

				if (vIsShow == true)
					EditorUtility.DisplayDialog("VersionData", "Build " + vName + " complete!", "OK");
				
				Debug.Log(string.Format("{0} build over", vName));
			}
			else
			{
				for (int i = 0; i < Setting.VersionItems.Count; i++)
				{
					string[] vPaths = Setting.VersionItems[i].value.Split('/');
					string vPath = Application.dataPath + "/" + string.Format(string.Join("/", vPaths, 1, vPaths.Length - 1), vLang);
					string vName = Setting.VersionItems[i].ver;

					if (vPath == "")
					{
						if (vIsShow == true)
							EditorUtility.DisplayDialog("VersionData", "Build VersionData Path Err!", "OK");
						return;
					}

					if (vName == "")
					{
						if (vIsShow == true)
							EditorUtility.DisplayDialog("VersionData", "Build VersionData Name Err!", "OK");
						return;
					}

					BuildVersion(vLang, vPath, vName);
					Debug.Log(string.Format("{0} build over", vName));
				}

				if (vIsShow == true)
					EditorUtility.DisplayDialog("VersionData", "Build all complete!", "OK");
			}
			AssetDatabase.Refresh();
		}
	}
}
#endif
