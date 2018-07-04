#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

namespace AutoLauncher.AssetBundleTool
{
	public static class ZipBuilder : object
	{
		//檢查包Zip路徑
		private static bool CheckZipPath (string vPath)
		{
			if (vPath == string.Empty || vPath == null)
				return false;

			if (vPath.Contains(Application.dataPath + "/" + Setting.InputAssetsFolder) == false)
				return false;

			string vLangPath = GetLangPath(vPath);
			string vZipPath = GetDataPath(vPath, Application.dataPath + "/" + Setting.InputAssetsFolder + "/");

			for (int i = 0; i < Setting.ZipPathCount; i++)
			{
				if (vZipPath == string.Format(Setting.ZipPathItems[i].value, vLangPath))
					return true;
			}

			return false;
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
			string vJson = Tools.SerializeObject(new rCRC(vFileName + ".zip", code));
			Tools.Save(vPath, "." + vFileName + ".crc", System.Text.UTF8Encoding.UTF8.GetBytes(vJson));
			File.SetAttributes(vPath + "." + vFileName + ".crc", FileAttributes.Hidden);
		}

		private static uint CreateZipFile (string vPath, string vFileName, string vPWD, List<string> vContainer)
		{
			Crc32 crc = new Crc32();
			using (ZipOutputStream vZipStream = new ZipOutputStream(File.Create(vPath + vFileName + ".zip")))
			{
				vZipStream.Password = vPWD;
				for (int i = 0; i < vContainer.Count; i++)
				{
					FileInfo vInfo = new FileInfo(vContainer[i]);
					ZipEntry vZipEntry = new ZipEntry(Path.GetFileName(vContainer[i]));
					vZipEntry.DateTime = vInfo.CreationTime;
					vZipStream.PutNextEntry(vZipEntry);

					using (FileStream vFileStream = File.OpenRead(vContainer[i]))
					{
						byte[] vBuffer = new byte[vFileStream.Length];
						crc.Update(vBuffer);
						StreamUtils.Copy(vFileStream, vZipStream, vBuffer);
						vFileStream.Close();
					}
				}
			}
			vContainer.Clear();
			return (uint)crc.Value;
		}

		//建立Zip
		private static void BuildZip (List<string> vFiles, string vPath, string vPWD = "")
		{
			string vZipPath = Application.dataPath + "/" + Setting.OutputAssetsFolder + "/" + GetDataPath(vPath, Application.dataPath + "/" + Setting.InputAssetsFolder + "/");
			string vZipName = Path.GetFileNameWithoutExtension(vPath) + "{0}";

			if (!Directory.Exists(vZipPath))
				Directory.CreateDirectory(vZipPath);
			
			List<string> vContainer = new List<string>();
			for (int i = 0; i < vFiles.Count; i++)
			{
				vContainer.Add(vFiles[i]);

				if (vContainer.Count != 10)
					continue;

				int vCount = Mathf.CeilToInt(i / 10f);
				string vFileName = string.Format(vZipName, vCount.ToString("D2"));

				uint crc = CreateZipFile(vZipPath, vFileName, vPWD, vContainer);
				CreateCrcFile(vZipPath, vFileName, crc);
			}

			if (vContainer.Count != 0)
			{
				int vCount = Mathf.CeilToInt(vFiles.Count / 10f);
				string vFileName = string.Format(vZipName, vCount.ToString("D2"));

				uint crc = CreateZipFile(vZipPath, vFileName, vPWD, vContainer);
				CreateCrcFile(vZipPath, vFileName, crc);
			}
		}

		//處理Zip
		public static void HandleZip (string vDirectPath, BuildTarget vTarget, bool vIsShow = true)
		{
			string[] vPaths = vDirectPath.Split('/');
			string vPath = Application.dataPath + "/" + string.Join("/", vPaths, 1, vPaths.Length - 1);
			string vTmp = string.Empty;

			//檢查路徑
			if (CheckZipPath(vPath) == false)
			{
				if (vIsShow == true)
					EditorUtility.DisplayDialog("HandleZip", "Build error and check your path!", "OK");
				return;
			}

			//檢查該路徑是否為資料夾
			if (CheckIsFolder(vPath) == true)
			{
				List<string> vFiles = new List<string>();
				for (int i = 0; i < Setting.ZipTypeCount; i++)
				{
					string[] vFileAry = Directory.GetFiles(vPath, Setting.ZipTypeItems[i].value, SearchOption.AllDirectories);
					for (int j = 0; j < vFileAry.Length; j++)
					{
						vTmp = vFileAry[j];
						vTmp = vTmp.Replace("\\", "/");
						vFiles.Add(vTmp);
					}
				}
				BuildZip(vFiles, vPath);
			}

			if (vIsShow == true)
				EditorUtility.DisplayDialog("HandleZip", "Build complete!", "OK");

			AssetDatabase.Refresh();
		}
	}
}
#endif
