#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;

	public static class Menu : object
	{
		[MenuItem("Assets/AutoLauncher/Bundle(Uncompress)")]
		public static void BuildBundleUncompress()
		{
			if (!(Selection.activeObject is DefaultAsset))
			{
				EditorUtility.DisplayDialog("HandleBundle", "Build error and check your path!", "OK");
				return;
			}

			BundleBuilder.HandleBundle(AssetDatabase.GetAssetPath(Selection.activeObject), Setting.BuildType, false);
		}

		[MenuItem("Assets/AutoLauncher/Bundle(Compress)")]
		public static void BuildBundleCompress()
		{
			if (!(Selection.activeObject is DefaultAsset))
			{
				EditorUtility.DisplayDialog("HandleBundle", "Build error and check your path!", "OK");
				return;
			}

			BundleBuilder.HandleBundle(AssetDatabase.GetAssetPath(Selection.activeObject), Setting.BuildType, true);
		}

		[MenuItem("Assets/AutoLauncher/Scene")]
		public static void BuildScene()
		{
			if (!(Selection.activeObject is DefaultAsset))
			{
				EditorUtility.DisplayDialog("HandleScene", "Build error and check your path!", "OK");
				return;
			}

			//HandleScene(AssetDatabase.GetAssetPath(Selection.activeObject), BuildTarget.Android);
		}

		[MenuItem("Assets/AutoLauncher/Zip")]
		public static void BuildZip()
		{
			if (!(Selection.activeObject is DefaultAsset))
			{
				EditorUtility.DisplayDialog("HandleZip", "Build error and check your path!", "OK");
				return;
			}

			ZipBuilder.HandleZip(AssetDatabase.GetAssetPath(Selection.activeObject), Setting.BuildType);
		}

		[MenuItem("AutoLauncher/Version/Build")]
		public static void BuildVersion()
		{
			VersionBuilder.HandleVersion(Setting.BuildLanguage.ToString());
		}

		[MenuItem("AutoLauncher/Version/Download")]
		public static void DownloadVersion()
		{
			HTTP.HandleDownloadVersion(Setting.BuildLanguage);
		}

		[MenuItem("Assets/AutoLauncher/Upload")]
		public static void Upload()
		{
			FTP.HandleUpload(AssetDatabase.GetAssetPath(Selection.activeObject));
		}

		[MenuItem("AutoLauncher/Copy/Copy2OutputAssets")]
		public static void Copy2OutputAssets()
		{
			Extension.HandleOutputAssets(Setting.BuildLanguage);
		}

		[MenuItem("AutoLauncher/Copy/Copy2StreamingAssets")]
		public static void Copy2StreamingAssets()
		{
			Extension.HandleStreamingAssets(Setting.BuildLanguage);
		}

		[MenuItem("AutoLauncher/Auto Run/Force")]
		public static void AutorunForce()
		{
			Auto.HandleAutoRun(Setting.BuildLanguage, true, true, false);
		}

		[MenuItem("AutoLauncher/Auto Run/Compare")]
		public static void AutorunCompare()
		{
			Auto.HandleAutoRun(Setting.BuildLanguage, true, true, true);
		}

		[MenuItem("AutoLauncher/Auto Run/Only Build")]
		public static void AutorunOnlyBuild()
		{
			Auto.HandleAutoRun(Setting.BuildLanguage, true, false, false);
		}

		[MenuItem("AutoLauncher/Auto Run/Only Upload")]
		public static void AutorunOnlyUpload()
		{
			Auto.HandleAutoRun(Setting.BuildLanguage, false, true, true);
		}
	}
}
#endif
