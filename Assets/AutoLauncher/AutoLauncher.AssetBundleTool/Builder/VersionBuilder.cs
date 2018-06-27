#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace AutoLauncher.AssetBundleTool
{
	public static class VersionBuilder : object
	{
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

		//處理ResData
		public static void HandleVersion (string vLang, bool vIsShow = true) 
		{
			string vPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + vLang + "/";
			string vName = "Version.res";

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

			List<rResData> vContainer = new List<rResData>();

			//檢查目錄是否存在
			if (Directory.Exists(vPath) == true)
			{
				//取得目錄底下的Data檔案
				string[] vFiles = Directory.GetFiles(vPath, "*.*", SearchOption.AllDirectories);

				for (int i = 0; i < vFiles.Length; i++)
				{
					FileInfo vInfo = new FileInfo(vFiles[i]);

					if (vInfo.Extension == ".res")
						continue;

					if (vInfo.Extension == ".meta")
						continue;

					if (vInfo.Extension == ".crc")
						continue;
					
					string vFilePath = Path.GetDirectoryName(vFiles[i]).Replace("\\", "/");
					rResData vData = new rResData();
					vData.Version = Setting.Ver;
					vData.FileName = vInfo.Name;
					vData.Path = GetDataPath(vFilePath, Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");
					vData.FileSize = vInfo.Length;
					vData.MD5Code = GetMd5File(vFilePath + "/", vInfo.Name);
					vContainer.Add(vData);
				}
			}

			//整合資料
			rResData[] vResData = vContainer.ToArray();
			//轉成Json
			string vJson = Tools.SerializeObject(vResData);
			//寫入檔案
			Tools.Save(vPath + "/" + "Versions" + "/", vName, System.Text.UTF8Encoding.UTF8.GetBytes(vJson));

//			string vStr = string.Empty;
//			for (int i = 0; i < vResData.Length; i++)
//			{
//				vStr = vStr + string.Format("Ver={0} FileName \"{1}\" Size={2} MD5 [{3}]\n", vResData[i].Version, vResData[i].FileName, vResData[i].FileSize, vResData[i].MD5Code);
//			}
//
//			string xPath = Application.dataPath + "/" + "Log" + "/" + Setting.OutputAssetsFolder + "/" + vLang + "/" + "Versions" + "/";
//
//			//寫入檔案
//			Tools.Save(xPath, vName.Replace(".res", ".txt"), System.Text.UTF8Encoding.UTF8.GetBytes(vStr));

			if (vIsShow == true)
				EditorUtility.DisplayDialog("VersionData", "Build " + vName + " complete!", "OK");

			Debug.Log(vName + " build over!");

			AssetDatabase.Refresh();
		}
	}
}
#endif
