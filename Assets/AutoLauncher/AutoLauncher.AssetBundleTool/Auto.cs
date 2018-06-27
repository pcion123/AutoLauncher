#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;

namespace AutoLauncher.AssetBundleTool
{
	public static class Auto : object
	{
		//取得語系路徑
		private static string GetLangPath (eLanguage vLang, bool vIsSlash = false)
		{
			return vIsSlash ? vLang.ToString() + "/" : vLang.ToString();
		}

		//處理AutoRun
		public static void HandleAutoRun (eLanguage vLang, bool vIsBundle, bool vIsUpload, bool vIsCompare)
		{
			for (int i = 0; i < Setting.AutoActionItems.Count; i++)
			{
				switch (Setting.AutoActionItems[i].acion)
				{
					case eBuildAcion.BundleUncompress:
						if (vIsBundle == false)
							continue;
						BundleBuilder.HandleBundle(string.Format(Setting.AutoActionItems[i].value, GetLangPath(vLang)), Setting.BuildType, false, false);
						break;
					case eBuildAcion.BundleCompress:
						if (vIsBundle == false)
							continue;
						BundleBuilder.HandleBundle(string.Format(Setting.AutoActionItems[i].value, GetLangPath(vLang)), Setting.BuildType, true, false);
						break;
					case eBuildAcion.Zip:
						ZipBuilder.HandleZip(string.Format(Setting.AutoActionItems[i].value, GetLangPath(vLang)), Setting.BuildType, false);
						break;
					case eBuildAcion.Move2Output:
						Extension.HandleOutputAssets(vLang, false);
						break;
					case eBuildAcion.Move2Streaming:
						Extension.HandleStreamingAssets(vLang, false);
						break;
					case eBuildAcion.BuildVersion:
						VersionBuilder.HandleVersion(vLang.ToString(), false);
						break;
					case eBuildAcion.DownloadVersion:
						HTTP.HandleDownloadVersion(vLang, false);
						break;
					case eBuildAcion.Upload:
						if (vIsUpload == false)
							continue;
						if (vIsCompare == false)
							FTP.HandleUpload(string.Format("Assets/{0}/{1}", Setting.OutputAssetsFolder, GetLangPath(vLang)), false);
						else
							FTP.HandleUploadEx(vLang, false);
						break;
				}
			}
			EditorUtility.DisplayDialog("AutoRun", "Auto Run Complete!", "OK");
		}
	}
}
#endif
