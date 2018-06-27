#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace AutoLauncher
{
	public class SettingObject : ScriptableObject
	{
		public string User = "Server01";
		public string Password = "123456";
		public string IP = "127.0.0.1";
		public string HTTP = "127.0.0.1";
		public string Ver = "1.0";
		public string EncryptionKey = "0";

		public BuildTarget BuildType = BuildTarget.NoTarget;
		public eLanguage BuildLanguage = eLanguage.None;

		public string InputAssetsFolder = "InputAssets";
		public string OutputAssetsFolder = "OutputAssets";
		public string DownloadAssetsFolder = "DownloadAssets";

		public List<DragValue> StreamingItems = null;
		public List<DragValue> OutputItems = null;
		public List<DragValue> ZipPathItems = null;
		public List<DragValue> ZipTypeItems = null;
		public List<DragValue> DependenceWords = null;
		public List<AutoValue> AutoActionItems = null;
	}
}
#endif