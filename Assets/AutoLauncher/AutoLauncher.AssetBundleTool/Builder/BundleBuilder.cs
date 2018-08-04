#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using System.IO;
	using Object = UnityEngine.Object;
	using Tools = AutoLauncher.Utility.Tools;

	public static class BundleBuilder : object
	{
		private const BuildAssetBundleOptions UncompressedOption = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle;
		private const BuildAssetBundleOptions CompressedOption   = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;

		//檢查包Bundle路徑
		private static bool CheckBundlePath(string path)
		{
			if (path == string.Empty || path == null)
				return false;

			if (path.Contains(Application.dataPath + "/" + Setting.InputAssetsFolder) == false)
				return false;

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

		//檢查是否為Atlas資源
		private static bool CheckIsAtlas(string path)
		{
			return path.Contains("-Atlas");
		}

		//檢查是否為字型資源
		private static bool CheckIsWordPrefab(string path)
		{
			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				string str = Application.dataPath + "/" + Setting.InputAssetsFolder + "/" + GetLangPath(path) + "/" + Setting.DependenceWords[i].value;
				if (string.Compare(path, str) == 0)
					return true;
			}
			return false;
		}

		private static Object[] GetDependenceWords(string path)
		{
			List<Object> list = new List<Object>();
			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				Object word = AssetDatabase.LoadMainAssetAtPath(string.Format(Setting.DependenceWords[i].value, GetLangPath(path)));
				list.Add(word);
			}
			return list.ToArray();
		}

		private static Object[] GetPrefabObjects(string path)
		{
			string[] objPaths = Directory.GetFiles(path.Replace(Path.GetFileName(path), ""), "*.prefab", SearchOption.AllDirectories);
			List<Object> assets = new List<Object>();
			foreach (string objPath in objPaths)
			{
				if (objPath.Contains(".svn") == true) 
					continue;
				
				try
				{
					Object vTmp = AssetDatabase.LoadMainAssetAtPath(string.Format("{0}/{1}", "Assets", objPath.Replace(Application.dataPath + "/", "")));

					if (vTmp == null) 
						continue;

					assets.Add(vTmp);
				}
				catch (Exception e)
				{
					Debug.LogError(string.Format("GetPrefabObjects Error -> {0} {1}", objPath, e.Message));
				}
			}
			return assets.ToArray();
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

		//取得語系路徑
		public static string GetLangPath(string path)
		{
			string[] values = System.Enum.GetNames(typeof(eLanguage));
			foreach (string value in values)
			{
				if (path.Contains(value + "/") == true)
					return value;
			}
			return string.Empty;
		}

		private static void CreateCrcFile(string path, string fileName, uint code)
		{
			string json = Tools.SerializeObject(new rCRC(fileName + ".unity3d", code));
			Tools.Save(path, "." + fileName + ".crc", System.Text.UTF8Encoding.UTF8.GetBytes(json));
			File.SetAttributes(path + "." + fileName + ".crc", FileAttributes.Hidden);
		}

		//建立Bundle
		private static void BuildBundle(string dataPath, BuildTarget target, bool isCompress)
		{
			string bundlePath = "Assets/" + Setting.OutputAssetsFolder + "/" + GetDataPath(dataPath, Application.dataPath + "/" + Setting.InputAssetsFolder + "/");
			string bundleName = Path.GetFileNameWithoutExtension(dataPath);
			string langPath = GetLangPath(dataPath);

			if (CheckIsWordPrefab(dataPath) == true)
				return;

			Object[] words = GetDependenceWords(dataPath);
			Object[] assets = GetPrefabObjects(dataPath);

			//檢查AssetBundle目錄是否存在
			if (!Directory.Exists(bundlePath))
				Directory.CreateDirectory(bundlePath);

			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				string[] paths = Setting.DependenceWords[i].value.Split('/');
				string path = "Assets/" + Setting.OutputAssetsFolder + "/" + string.Join("/", paths, 2, paths.Length - 4);
				//檢查AssetBundle目錄是否存在
				if (!Directory.Exists(string.Format(path, langPath)))
					Directory.CreateDirectory(string.Format(path, langPath));
			}

			Debug.Log("Build bundle: " + bundlePath + bundleName);

			BuildAssetBundleOptions options = isCompress ? CompressedOption : UncompressedOption;
			BuildPipeline.PushAssetDependencies();
			List<uint> wordCrcs = new List<uint>();
			for (int i = 0; i < words.Length; i++)
			{
				uint crc = 0;
				string[] paths = Setting.DependenceWords[i].value.Split('/');
				string path = "Assets/" + Setting.OutputAssetsFolder + "/" + string.Join("/", paths, 2, paths.Length - 4);
				string name = paths[paths.Length - 2] + ".unity3d";
				BuildPipeline.BuildAssetBundle(words[i], null, string.Format(path, langPath) + "/" + name, out crc, options, target);
				wordCrcs.Add(crc);
			}
			if (bundleName.Contains("Panel_") == true)
			{
				uint atlasCode = 0;
				uint prefabCode = 0;
				BuildPipeline.PushAssetDependencies();
				if (assets.Length == 2)
				{
					BuildPipeline.BuildAssetBundle(assets[0], null, bundlePath + bundleName + "-Atlas.unity3d", out atlasCode, options, target);
					BuildPipeline.PushAssetDependencies();
					BuildPipeline.BuildAssetBundle(assets[1], null, bundlePath + bundleName + ".unity3d", out prefabCode, options, target);
					BuildPipeline.PopAssetDependencies();
				}
				else
				{
					BuildPipeline.BuildAssetBundle(assets[0], null, bundlePath + bundleName + ".unity3d", out prefabCode, options, target);
				}
				BuildPipeline.PopAssetDependencies();

				if (assets.Length == 2)
				{
					CreateCrcFile(bundlePath, bundleName + "-Atlas", atlasCode);
					CreateCrcFile(bundlePath, bundleName, prefabCode);
				}
				else
				{
					CreateCrcFile(bundlePath, bundleName, prefabCode);
				}
			}
			else
			{
				uint prefabCode = 0;
				BuildPipeline.PushAssetDependencies();
				BuildPipeline.BuildAssetBundle(assets[0], assets, bundlePath + bundleName + ".unity3d", out prefabCode, options, target);
				BuildPipeline.PopAssetDependencies();
				CreateCrcFile(bundlePath, bundleName, prefabCode);
			}
			BuildPipeline.PopAssetDependencies();

			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				string[] paths = Setting.DependenceWords[i].value.Split('/');
				string path = "Assets/" + Setting.OutputAssetsFolder + "/" + string.Join("/", paths, 2, paths.Length - 4) + "/";
				string name = paths[paths.Length - 2];
				CreateCrcFile(string.Format(path, langPath), name, wordCrcs[i]);
			}

			Debug.Log("Build bundle: " + bundlePath + bundleName + " is ok!");
		}

		//處理Bundle
		public static void HandleBundle(string directPath, BuildTarget target, bool isCompress, bool isShow = true)
		{
			string[] paths = directPath.Split('/');
			string path = Application.dataPath + "/" + string.Join("/", paths, 1, paths.Length - 1);
			string tmp = string.Empty;

			//檢查路徑
			if (CheckBundlePath(path) == false)
			{
				if (isShow == true)
					EditorUtility.DisplayDialog("HandleBundle", "Build error and check your path!", "OK");
				return;
			}

			//檢查該路徑是否為資料夾
			if (CheckIsFolder(path) == true)
			{
				string[] fileArray = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
				for (int i = 0; i < fileArray.Length; i++)
				{
					tmp = fileArray[i];
					tmp = tmp.Replace("\\", "/");

					if (CheckIsAtlas(tmp) == true)
						continue;

					BuildBundle(tmp, target, isCompress);
				}
			}
			else
			{
				tmp = path;
				tmp = tmp.Replace("\\", "/");

				BuildBundle(tmp, target, isCompress);
			}

			if (isShow == true)
				EditorUtility.DisplayDialog("HandleBundle", "Build complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
