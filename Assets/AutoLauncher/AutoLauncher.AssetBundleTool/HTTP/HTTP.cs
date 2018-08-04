#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Net;
	using System.IO;
	using AutoLauncher.Enum;
	using Tools = AutoLauncher.Utility.Tools;

	public static class HTTP : object
	{
		private const string HEADER = "http://";

		//取得HTTP路徑
		public static string GetLocation(string ip, string hostName)
		{
			switch (Setting.BuildType)
			{
				case BuildTarget.StandaloneWindows:
					return HEADER + ip + "/" + hostName + "/Windows/";
				case BuildTarget.iOS:
					return HEADER + ip + "/" + hostName + "/Ios/";
				case BuildTarget.Android:
					return HEADER + ip + "/" + hostName + "/Android/";
			}
			return string.Empty;
		}

		//取得語系路徑
		private static string GetLangPath(eLanguage lang, bool isSlash = false)
		{
			return isSlash ? lang.ToString() + "/" : lang.ToString();
		}

		private static void Download(eLanguage lang, string path, string name)
		{
			using (WWW bundle = new WWW(path + name))
			{
				//檢查下載錯誤訊息
				if (bundle.error != null)
				{
					throw new System.Exception(bundle.error);
				}

				while (bundle.isDone == false)
				{
					if (bundle.error != null)
					{
						throw new System.Exception(bundle.error);
					}
				}

				//檢查是否下載完成
				if (bundle.isDone == true)
				{
					byte[] xor = Tools.XOR(bundle.bytes, Setting.EncryptionKeyValue);
					string str = GetLocation(Setting.HTTP, Setting.User);
					string zPath = Application.dataPath + "/" + Setting.DownloadAssetsFolder + "/" + path.Replace(str, "");
					Tools.Save(zPath, name, xor);
				}
				else
				{
					throw new System.Exception(bundle.error);
				}
			}
		}

		//處理上傳資料
		public static void HandleDownloadVersion(eLanguage lang, bool isShow = true)
		{
			string vPath = GetLocation(Setting.HTTP, Setting.User) + GetLangPath(lang, true) + "Versions/";
			string zPath = Application.dataPath + "/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(lang, true) + "Versions/";
			string xPath = Application.dataPath + "/AutoLauncher/log/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(lang, true) + "Versions/";

			//檢查目錄是否存在
			if (Directory.Exists(zPath) == true)
				Directory.Delete(zPath, true);

			//檢查目錄是否存在
			if (Directory.Exists(xPath) == true)
				Directory.Delete(xPath, true);

			try 
			{
				if (Setting.VersionItems == null || Setting.VersionItems.Count == 0)
				{
					Download(lang, vPath, "Main.res");
				}
				else
				{
					for (int i = 0; i < Setting.VersionItems.Count; i++)
					{
						Download(lang, vPath, Setting.VersionItems[i].ver);
					}
				}
			}
			catch (System.Exception e) 
			{
				Debug.LogWarning(e.Message + " " + "Version" + " Download Err!");
			}

			if (isShow == true)
				EditorUtility.DisplayDialog("Download", "Download all complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
