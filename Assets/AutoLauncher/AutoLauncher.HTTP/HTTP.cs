#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Net;
using System.IO;

namespace AutoLauncher
{
	public static class HTTP : object
	{
		private const string HEADER = "http://";

		//取得HTTP路徑
		public static string GetLocation (string vIP, string vName)
		{
			switch (Setting.BuildType)
			{
				case BuildTarget.StandaloneWindows:
					return HEADER + vIP + "/" + vName + "/Windows/";
				case BuildTarget.iOS:
					return HEADER + vIP + "/" + vName + "/Ios/";
				case BuildTarget.Android:
					return HEADER + vIP + "/" + vName + "/Android/";
			}
			return string.Empty;
		}

		//取得語系路徑
		private static string GetLangPath (eLanguage vLang, bool vIsSlash = false)
		{
			return vIsSlash ? vLang.ToString() + "/" : vLang.ToString();
		}

		private static void Download (eLanguage vLang, string vPath, string vName)
		{
			using (WWW vBundle = new WWW(vPath + vName))
			{
				//檢查下載錯誤訊息
				if (vBundle.error != null)
				{
					throw new System.Exception(vBundle.error);
				}

				while (vBundle.isDone == false)
				{
					if (vBundle.error != null)
					{
						throw new System.Exception(vBundle.error);
					}
				}

				//檢查是否下載完成
				if (vBundle.isDone == true)
				{
					byte[] vXor = Tools.XOR(vBundle.bytes, Setting.EncryptionKeyValue);

					string vStr = GetLocation(Setting.HTTP, Setting.User);
					string zPath = Application.dataPath + "/" + Setting.DownloadAssetsFolder + "/" + vPath.Replace(vStr, "");

					Tools.Save(zPath, vName, vXor);

					string vJson = System.Text.UTF8Encoding.UTF8.GetString(vXor);
					rResData[] vResData = Tools.DeserializeObject<rResData[]>(vJson);

					string xStr = string.Empty;

					for (int i = 0; i < vResData.Length; i++)
					{
						xStr = xStr + string.Format("Ver={0} FileName \"{1}\" Size={2} MD5 [{3}]\n", vResData[i].Version, vResData[i].FileName, vResData[i].FileSize, vResData[i].MD5Code);
					}

					string xPath = Application.dataPath + "/AutoLauncher/log/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(vLang, true) + "Versions/";

					//寫入檔案
					Tools.Save(xPath, vName.Replace(".res", ".txt"), System.Text.UTF8Encoding.UTF8.GetBytes(xStr));
				}
				else
				{
					throw new System.Exception(vBundle.error);
				}
			}
		}

		//處理上傳資料
		public static void HandleDownloadVersion (eLanguage vLang, bool vIsShow = true)
		{
			string vPath = GetLocation(Setting.HTTP, Setting.User) + GetLangPath(vLang, true) + "Versions/";
			string zPath = Application.dataPath + "/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(vLang, true) + "Versions/";
			string xPath = Application.dataPath + "/AutoLauncher/log/" + Setting.DownloadAssetsFolder + "/" + GetLangPath(vLang, true) + "Versions/";

			//檢查目錄是否存在
			if (Directory.Exists(zPath) == true)
				Directory.Delete(zPath, true);

			//檢查目錄是否存在
			if (Directory.Exists(xPath) == true)
				Directory.Delete(xPath, true);

			try 
			{
				Download(vLang, vPath, "Version.res");
			}
			catch (System.Exception e) 
			{
				Debug.LogWarning(e.Message + " " + "Version.res" + " Download Err!");
			}

			if (vIsShow == true)
				EditorUtility.DisplayDialog("Download", "Download all complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
