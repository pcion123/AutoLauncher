#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System;
	using System.IO;
	using AutoLauncher.Enum;
	using Tools = AutoLauncher.Utility.Tools;

	public static class Extension : object
	{
		//取得語系路徑
		private static string GetLangPath(eLanguage lang, bool isSlash = false)
		{
			return isSlash ? lang.ToString() + "/" : lang.ToString();
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

		//複製檔案
		private static void CopyFile(string fileName, string sourcePath, string targetPath)
		{
			// Use Path class to manipulate file and directory paths.
			string sourceFile = Path.Combine(sourcePath, fileName);
			string destFile = Path.Combine(targetPath, fileName);

			// To copy a folder's contents to a new location:
			// Create a new target folder, if necessary.
			if (!Directory.Exists(targetPath))
				Directory.CreateDirectory(targetPath);

			if (!File.Exists(sourcePath + fileName))
				return;

			// To copy a file to another location and 
			// overwrite the destination file if it already exists.
			File.Copy(sourceFile, destFile, true);
		}

		//移動檔案
		private static void MoveFile(string fileName, string sourcePath, string targetPath)
		{
			// Use Path class to manipulate file and directory paths.
			string sourceFile = Path.Combine(sourcePath, fileName);
			string destFile = Path.Combine(targetPath, fileName);

			// To copy a folder's contents to a new location:
			// Create a new target folder, if necessary.
			if (!Directory.Exists(targetPath))
				Directory.CreateDirectory(targetPath);

			if (!File.Exists(sourcePath + fileName))
				return;

			// To copy a file to another location and 
			// overwrite the destination file if it already exists.
			File.Copy(sourceFile, destFile, true);

			File.Delete(sourcePath + fileName);
		}

		//處理最小包
		public static void HandleOutputAssets(eLanguage lang, bool isShow = true)
		{
			for (int i = 0; i < Setting.OutputCount; i++)
			{
				string[] files = Directory.GetFiles(Application.dataPath + "/" + Setting.InputAssetsFolder + "/" + GetLangPath(lang), Setting.OutputItems[i].value, SearchOption.AllDirectories);
				for (int j = 0; j < files.Length; j++)
				{
					string path = files[j].Replace("\\", "/");
					string name = Path.GetFileName(path);
					string extension = Path.GetExtension(path);

					if (extension == ".meta")
						continue;

					string sourcePath = Application.dataPath + "/" + Setting.InputAssetsFolder + "/" + GetDataPath(Path.GetDirectoryName(path), Application.dataPath + "/" + Setting.InputAssetsFolder + "/");
					string destPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetDataPath(Path.GetDirectoryName(path), Application.dataPath + "/" + Setting.InputAssetsFolder + "/");

					if (extension != ".txt")
					{
						CopyFile(name, sourcePath, destPath);
					}
					else
					{
						//讀取資料
						byte[] data = Tools.Load(GetDataPath(sourcePath, name), name);

						//加密存檔
						Tools.Save(GetDataPath(destPath, name), name, data, Setting.EncryptionKeyValue);
					}
				}
			}

			if (isShow)
				EditorUtility.DisplayDialog("Copy", "Copy complete!", "OK");

			AssetDatabase.Refresh();
		}

		//處理最小包
		public static void HandleStreamingAssets(eLanguage lang, bool isShow = true)
		{
			for (int i = 0; i < Setting.StreamingCount; i++)
			{
				string[] files = Directory.GetFiles(Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetLangPath(lang), Setting.StreamingItems[i].value, SearchOption.AllDirectories);
				for (int j = 0; j < files.Length; j++) {
					string path = files[j].Replace("\\", "/");
					string name = Path.GetFileName(path);
					string extension = Path.GetExtension(path);

					if (extension == ".meta")
						continue;

					string sourcePath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetDataPath(Path.GetDirectoryName(path), Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");
					string destPath = Application.streamingAssetsPath + "/" + GetDataPath(Path.GetDirectoryName(path), Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

					MoveFile(name, sourcePath, destPath);
				}
			}

			if (isShow)
				EditorUtility.DisplayDialog("Move", "Move complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
