#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;

namespace AutoLauncher.AssetBundleTool
{
	public static class Extension : object
	{
		//取得語系路徑
		private static string GetLangPath (eLanguage vLang, bool vIsSlash = false)
		{
			return vIsSlash ? vLang.ToString() + "/" : vLang.ToString();
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

		//複製檔案
		private static void CopyFile (string aFileName, string aSourcePath, string aTargetPath)
		{
			// Use Path class to manipulate file and directory paths.
			string sourceFile = System.IO.Path.Combine(aSourcePath, aFileName);
			string destFile = System.IO.Path.Combine(aTargetPath, aFileName);

			// To copy a folder's contents to a new location:
			// Create a new target folder, if necessary.
			if (!System.IO.Directory.Exists(aTargetPath))
			{
				System.IO.Directory.CreateDirectory(aTargetPath);
			}

			if (!System.IO.File.Exists(aSourcePath + aFileName))
			{
				return;
			}

			// To copy a file to another location and 
			// overwrite the destination file if it already exists.
			System.IO.File.Copy(sourceFile, destFile, true);
		}

		//移動檔案
		private static void MoveFile (string aFileName, string aSourcePath, string aTargetPath)
		{
			// Use Path class to manipulate file and directory paths.
			string sourceFile = System.IO.Path.Combine(aSourcePath, aFileName);
			string destFile = System.IO.Path.Combine(aTargetPath, aFileName);

			// To copy a folder's contents to a new location:
			// Create a new target folder, if necessary.
			if (!System.IO.Directory.Exists(aTargetPath))
			{
				System.IO.Directory.CreateDirectory(aTargetPath);
			}

			if (!System.IO.File.Exists(aSourcePath + aFileName))
			{
				return;
			}

			// To copy a file to another location and 
			// overwrite the destination file if it already exists.
			System.IO.File.Copy(sourceFile, destFile, true);

			System.IO.File.Delete(aSourcePath + aFileName);
		}

		//處理最小包
		public static void HandleOutputAssets (eLanguage vLang, bool vIsShow = true)
		{
			for (int i = 0; i < Setting.OutputCount; i++)
			{
				string[] vFile = Directory.GetFiles(Application.dataPath + "/" + Setting.InputAssetsFolder + "/" + GetLangPath(vLang), Setting.OutputItems[i].value, SearchOption.AllDirectories);

				for (int j = 0; j < vFile.Length; j++)
				{
					string vPath = vFile[j].Replace("\\", "/");
					string vName = Path.GetFileName(vPath);
					string vExtension = Path.GetExtension(vPath);

					if (vExtension == ".meta")
						continue;

					string vSourcePath = Application.dataPath + "/" + Setting.InputAssetsFolder + "/" + GetDataPath(Path.GetDirectoryName(vPath), Application.dataPath + "/" + Setting.InputAssetsFolder + "/");
					string vDestPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetDataPath(Path.GetDirectoryName(vPath), Application.dataPath + "/" + Setting.InputAssetsFolder + "/");

					if (vExtension != ".txt")
					{
						CopyFile(vName, vSourcePath, vDestPath);
					}
					else
					{
						//讀取資料
						byte[] vData = Tools.Load(GetDataPath(vSourcePath, vName), vName);

						//加密存檔
						Tools.Save(GetDataPath(vDestPath, vName), vName, vData, Setting.EncryptionKeyValue);
					}
				}
			}

			if (vIsShow == true)
				EditorUtility.DisplayDialog("Copy", "Copy complete!", "OK");

			AssetDatabase.Refresh();
		}

		//處理最小包
		public static void HandleStreamingAssets (eLanguage vLang, bool vIsShow = true)
		{
			for (int i = 0; i < Setting.StreamingCount; i++)
			{
				string[] vFile = Directory.GetFiles(Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetLangPath(vLang), Setting.StreamingItems[i].value, SearchOption.AllDirectories);

				for (int j = 0; j < vFile.Length; j++) {
					string vPath = vFile[j].Replace("\\", "/");
					string vName = Path.GetFileName(vPath);
					string vExtension = Path.GetExtension(vPath);

					if (vExtension == ".meta")
						continue;

					string vSourcePath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetDataPath(Path.GetDirectoryName(vPath), Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");
					string vDestPath = Application.streamingAssetsPath + "/" + GetDataPath(Path.GetDirectoryName(vPath), Application.dataPath + "/" + Setting.OutputAssetsFolder + "/");

					MoveFile(vName, vSourcePath, vDestPath);
				}
			}

			if (vIsShow == true)
				EditorUtility.DisplayDialog("Move", "Move complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
