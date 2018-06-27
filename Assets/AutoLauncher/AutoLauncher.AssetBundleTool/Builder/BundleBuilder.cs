#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Object = UnityEngine.Object;

namespace AutoLauncher.AssetBundleTool
{
	public static class BundleBuilder : object
	{
		private const BuildAssetBundleOptions UncompressedOption = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle;
		private const BuildAssetBundleOptions CompressedOption   = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.DeterministicAssetBundle;

		//檢查包Bundle路徑
		private static bool CheckBundlePath (string vPath)
		{
			if (vPath == string.Empty || vPath == null)
				return false;

			if (vPath.Contains(Application.dataPath + "/" + Setting.InputAssetsFolder) == false)
				return false;

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

		//檢查是否為Atlas資源
		private static bool CheckIsAtlas (string vPath)
		{
			if (vPath.Contains("-Atlas") == true)
				return true;
			else
				return false;
		}

		//檢查是否為字型資源
		private static bool CheckIsWordPrefab (string vPath)
		{
			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				string str = Application.dataPath + "/" + Setting.InputAssetsFolder + "/" + GetLangPath(vPath) + "/" + Setting.DependenceWords[i].value;
				if (string.Compare(vPath, str) == 0)
					return true;
			}
			return false;
		}

		private static Object[] GetDependenceWords(string vPath)
		{
			List<Object> list = new List<Object>();
			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				Object vWord = AssetDatabase.LoadMainAssetAtPath(string.Format(Setting.DependenceWords[i].value, GetLangPath(vPath)));
				list.Add(vWord);
			}
			return list.ToArray();
		}

		private static Object[] GetPrefabObjects (string vPath)
		{
			string[] vObjPaths = Directory.GetFiles(vPath.Replace(Path.GetFileName(vPath), ""), "*.prefab", SearchOption.AllDirectories);
			List<Object> vAssets = new List<Object>();
			foreach (string vObjPath in vObjPaths)
			{
				if (vObjPath.Contains(".svn") == true) 
					continue;
				
				try
				{
					Object vTmp = AssetDatabase.LoadMainAssetAtPath(string.Format("{0}/{1}", "Assets", vObjPath.Replace(Application.dataPath + "/", "")));

					if (vTmp == null) 
						continue;

					vAssets.Add(vTmp);
				}
				catch (Exception e)
				{
					Debug.LogError(string.Format("GetPrefabObjects Error -> {0} {1}", vObjPath, e.Message));
				}
			}
			return vAssets.ToArray();
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

		//取得語系路徑
		public static string GetLangPath (string vPath)
		{
			string[] values = Enum.GetNames(typeof(eLanguage));
			foreach (string value in values)
			{
				if (vPath.Contains(value + "/") == true)
					return value;
			}
			return string.Empty;
		}

		private static void CreateCrcFile (string vPath, string vFileName, uint code)
		{
			string vJson = Tools.SerializeObject(new rCRC(vFileName + ".unity3d", code));
			Tools.Save(vPath, "." + vFileName + ".crc", System.Text.UTF8Encoding.UTF8.GetBytes(vJson));
			File.SetAttributes(vPath + "." + vFileName + ".crc", FileAttributes.Hidden);
		}

		//建立Bundle
		private static void BuildBundle (string vPath, BuildTarget vTarget, bool vIsCompress)
		{
			string vBundlePath = "Assets/" + Setting.OutputAssetsFolder + "/" + GetDataPath(vPath, Application.dataPath + "/" + Setting.InputAssetsFolder + "/");
			string vBundleName = Path.GetFileNameWithoutExtension(vPath);
			string vLangPath = GetLangPath(vPath);

			if (CheckIsWordPrefab(vPath) == true)
				return;

			Object[] vWords = GetDependenceWords(vPath);
			Object[] vAssets = GetPrefabObjects(vPath);

			//檢查AssetBundle目錄是否存在
			if (!Directory.Exists(vBundlePath))
				Directory.CreateDirectory(vBundlePath);

			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				string[] paths = Setting.DependenceWords[i].value.Split('/');
				string path = "Assets/" + Setting.OutputAssetsFolder + "/" + string.Join("/", paths, 2, paths.Length - 4);
				//檢查AssetBundle目錄是否存在
				if (!Directory.Exists(string.Format(path, vLangPath)))
					Directory.CreateDirectory(string.Format(path, vLangPath));
			}

			Debug.Log("Build bundle: " + vBundlePath + vBundleName);

			BuildAssetBundleOptions vOptions = vIsCompress ? CompressedOption : UncompressedOption;
			BuildPipeline.PushAssetDependencies();
			List<uint> vWordCrcs = new List<uint>();
			for (int i = 0; i < vWords.Length; i++)
			{
				uint crc = 0;
				string[] paths = Setting.DependenceWords[i].value.Split('/');
				string path = "Assets/" + Setting.OutputAssetsFolder + "/" + string.Join("/", paths, 2, paths.Length - 4);
				string name = paths[paths.Length - 2] + ".unity3d";
				BuildPipeline.BuildAssetBundle(vWords[i], null, string.Format(path, vLangPath) + "/" + name, out crc, vOptions, vTarget);
				vWordCrcs.Add(crc);
			}
			if (vBundleName.Contains("Panel_") == true)
			{
				uint vAtlasCode = 0;
				uint vPrefabCode = 0;
				BuildPipeline.PushAssetDependencies();
				if (vAssets.Length == 2)
				{
					BuildPipeline.BuildAssetBundle(vAssets[0], null, vBundlePath + vBundleName + "-Atlas.unity3d", out vAtlasCode, vOptions, vTarget);
					BuildPipeline.PushAssetDependencies();
					BuildPipeline.BuildAssetBundle(vAssets[1], null, vBundlePath + vBundleName + ".unity3d", out vPrefabCode, vOptions, vTarget);
					BuildPipeline.PopAssetDependencies();
				}
				else
				{
					BuildPipeline.BuildAssetBundle(vAssets[0], null, vBundlePath + vBundleName + ".unity3d", out vPrefabCode, vOptions, vTarget);
				}
				BuildPipeline.PopAssetDependencies();

				if (vAssets.Length == 2)
				{
					CreateCrcFile(vBundlePath, vBundleName + "-Atlas", vAtlasCode);
					CreateCrcFile(vBundlePath, vBundleName, vPrefabCode);
				}
				else
				{
					CreateCrcFile(vBundlePath, vBundleName, vPrefabCode);
				}
			}
			else
			{
				uint vPrefabCode = 0;
				BuildPipeline.PushAssetDependencies();
				BuildPipeline.BuildAssetBundle(vAssets[0], vAssets, vBundlePath + vBundleName + ".unity3d", out vPrefabCode, vOptions, vTarget);
				BuildPipeline.PopAssetDependencies();
				CreateCrcFile(vBundlePath, vBundleName, vPrefabCode);
			}
			BuildPipeline.PopAssetDependencies();

			for (int i = 0; i < Setting.DependenceWordCount; i++)
			{
				string[] paths = Setting.DependenceWords[i].value.Split('/');
				string path = "Assets/" + Setting.OutputAssetsFolder + "/" + string.Join("/", paths, 2, paths.Length - 4) + "/";
				string name = paths[paths.Length - 2];
				CreateCrcFile(string.Format(path, vLangPath), name, vWordCrcs[i]);
			}

			Debug.Log("Build bundle: " + vBundlePath + vBundleName + " is ok!");
		}

		//處理Bundle
		public static void HandleBundle (string vDirectPath, BuildTarget vTarget, bool vIsCompress, bool vIsShow = true)
		{
			string[] vPaths = vDirectPath.Split('/');
			string vPath = Application.dataPath + "/" + string.Join("/", vPaths, 1, vPaths.Length - 1);
			string vTmp = string.Empty;

			//檢查路徑
			if (CheckBundlePath(vPath) == false)
			{
				if (vIsShow == true)
					EditorUtility.DisplayDialog("HandleBundle", "Build error and check your path!", "OK");
				return;
			}

			//檢查該路徑是否為資料夾
			if (CheckIsFolder(vPath) == true)
			{
				string[] vFileAry = Directory.GetFiles(vPath, "*.prefab", SearchOption.AllDirectories);
				for (int i = 0; i < vFileAry.Length; i++)
				{
					vTmp = vFileAry[i];
					vTmp = vTmp.Replace("\\", "/");

					if (CheckIsAtlas(vTmp) == true)
						continue;

					BuildBundle(vTmp, vTarget, vIsCompress);
				}
			}
			else
			{
				vTmp = vPath;
				vTmp = vTmp.Replace("\\", "/");

				BuildBundle(vTmp, vTarget, vIsCompress);
			}

			if (vIsShow == true)
				EditorUtility.DisplayDialog("HandleBundle", "Build complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
