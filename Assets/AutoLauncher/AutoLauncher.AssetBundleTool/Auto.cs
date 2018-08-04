#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEditor;
	using AutoLauncher.Enum;

	public static class Auto : object
	{
		//取得語系路徑
		private static string GetLangPath(eLanguage lang, bool isSlash = false)
		{
			return isSlash ? lang.ToString() + "/" : lang.ToString();
		}

		//處理AutoRun
		public static void HandleAutoRun(eLanguage lang, bool isBundle, bool isUpload, bool isCompare)
		{
			for (int i = 0; i < Setting.AutoActionItems.Count; i++)
			{
				switch (Setting.AutoActionItems[i].acion)
				{
					case eBuildAcion.BundleUncompress:
						if (!isBundle)
							continue;
						BundleBuilder.HandleBundle(string.Format(Setting.AutoActionItems[i].value, GetLangPath(lang)), Setting.BuildType, false, false);
						break;
					case eBuildAcion.BundleCompress:
						if (!isBundle)
							continue;
						BundleBuilder.HandleBundle(string.Format(Setting.AutoActionItems[i].value, GetLangPath(lang)), Setting.BuildType, true, false);
						break;
					case eBuildAcion.Zip:
						ZipBuilder.HandleZip(string.Format(Setting.AutoActionItems[i].value, GetLangPath(lang)), Setting.BuildType, false);
						break;
					case eBuildAcion.Move2Output:
						Extension.HandleOutputAssets(lang, false);
						break;
					case eBuildAcion.Move2Streaming:
						Extension.HandleStreamingAssets(lang, false);
						break;
					case eBuildAcion.BuildVersion:
						VersionBuilder.HandleVersion(lang.ToString(), false);
						break;
					case eBuildAcion.DownloadVersion:
						HTTP.HandleDownloadVersion(lang, false);
						break;
					case eBuildAcion.Upload:
						if (!isUpload)
							continue;
						if (!isCompare)
							FTP.HandleUpload(string.Format("Assets/{0}/{1}", Setting.OutputAssetsFolder, GetLangPath(lang)), false);
						else
							FTP.HandleUploadEx(lang, false);
						break;
				}
			}
			EditorUtility.DisplayDialog("AutoRun", "Auto Run Complete!", "OK");
		}
	}
}
#endif
