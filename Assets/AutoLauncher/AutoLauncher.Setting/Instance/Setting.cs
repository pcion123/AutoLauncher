#if UNITY_EDITOR
namespace AutoLauncher
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using AutoLauncher.AssetBundleTool;
	using AutoLauncher.Enum;
	using Tools = AutoLauncher.Utility.Tools;

	[InitializeOnLoad]
	public static class Setting : object
	{
		//資訊結構
		private struct rSave
		{
			public string Name;
			public string IP;
			public string Version;

			public rSave(string name, string ip, string version)
			{
				Name = name;
				IP = ip;
				Version = version;
			}
		}

		private static SettingObject mInstance;

		public static SettingObject Instance
		{
			get
			{
				if (mInstance == null)
				{
					mInstance = AssetDatabase.LoadAssetAtPath("Assets/AutoLauncher/AutoLauncher.Setting/Setting.asset", typeof(SettingObject)) as SettingObject;

					if (mInstance == null)
					{
						// If not found, autocreate the asset object.
						mInstance = ScriptableObject.CreateInstance<SettingObject>();

						string properPath = Path.Combine(Application.dataPath, "AutoLauncher/AutoLauncher.Setting");

						if (!Directory.Exists(properPath))
							AssetDatabase.CreateFolder("Assets", "AutoLauncher/AutoLauncher.Setting");

						string fullPath = Path.Combine(Path.Combine("Assets", "AutoLauncher/AutoLauncher.Setting"), "Setting.asset");
						AssetDatabase.CreateAsset(mInstance, fullPath);
					}
				}

				return mInstance;
			}
		}

		public static string User
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.User;
			}

			set
			{
				if (mInstance == null)
					return;

				if (mInstance.User != value)
				{
					mInstance.User = value;
					SaveInfo(new rSave(mInstance.User, mInstance.HTTP, mInstance.Ver));
					EditorUtility.SetDirty(mInstance);
				}
			}
		}

		public static string PWD
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.Password;
			}

			set
			{
				if (mInstance == null)
					return;

				if (mInstance.Password != value)
				{
					mInstance.Password = value;
				}
			}
		}

		public static string IP
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.IP;
			}

			set
			{
				if (mInstance == null)
					return;

				if (mInstance.IP != value)
				{
					mInstance.IP = value;
				}
			}
		}

		public static string HTTP
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.HTTP;
			}

			set
			{
				if (mInstance == null)
					return;

				if (mInstance.HTTP != value)
				{
					mInstance.HTTP = value;
					SaveInfo(new rSave(mInstance.User, mInstance.HTTP, mInstance.Ver));
					EditorUtility.SetDirty(mInstance);
				}
			}
		}

		public static string Ver
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.Ver;
			}
			set
			{
				if (mInstance == null)
					return;

				if (mInstance.Ver != value)
				{
					mInstance.Ver = value;
					SaveInfo(new rSave(mInstance.User, mInstance.HTTP, mInstance.Ver));
					EditorUtility.SetDirty(mInstance);
				}
			}
		}

		public static int EncryptionKeyValue
		{
			get
			{
				if (mInstance == null)
				{
					return 0;
				}
				else
				{
					int tmp = 0;
					int.TryParse(mInstance.EncryptionKey, out tmp);
					return tmp;
				}
			}
			set
			{
				if (mInstance == null)
					return;

				mInstance.EncryptionKey = value.ToString();
			}
		}

		public static string EncryptionKeyStr
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.EncryptionKey;
			}
			set
			{
				if (mInstance == null)
					return;

				mInstance.EncryptionKey = value;
			}
		}

		public static BuildTarget BuildType
		{
			get
			{
				if (mInstance == null)
					return BuildTarget.NoTarget;
				else
					return mInstance.BuildType;
			}
			set
			{
				if (mInstance == null)
					return;

				mInstance.BuildType = value;
			}
		}

		public static eLanguage BuildLanguage
		{
			get
			{
				if (mInstance == null)
					return eLanguage.None;
				else
					return mInstance.BuildLanguage;
			}
			set
			{
				if (mInstance == null)
					return;

				mInstance.BuildLanguage = value;
			}
		}

		public static string InputAssetsFolder
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.InputAssetsFolder;
			}
			set
			{
				if (mInstance == null)
					return;

				if (mInstance.InputAssetsFolder != value)
				{
					mInstance.InputAssetsFolder = value;
				}
			}
		}

		public static string OutputAssetsFolder
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.OutputAssetsFolder;
			}
			set
			{
				if (mInstance == null)
					return;

				if (mInstance.OutputAssetsFolder != value)
				{
					mInstance.OutputAssetsFolder = value;
				}
			}
		}

		public static string DownloadAssetsFolder
		{
			get
			{
				if (mInstance == null)
					return string.Empty;
				else
					return mInstance.DownloadAssetsFolder;
			}
			set
			{
				if (mInstance == null)
					return;

				if (mInstance.DownloadAssetsFolder != value)
				{
					mInstance.DownloadAssetsFolder = value;
				}
			}
		}

		public static int StreamingCount
		{
			get
			{
				if (mInstance == null || mInstance.StreamingItems == null)
					return 0;
				else
					return mInstance.StreamingItems.Count;
			}
		}

		public static int OutputCount
		{
			get
			{
				if (mInstance == null || mInstance.OutputItems == null)
					return 0;
				else
					return mInstance.OutputItems.Count;
			}
		}

		public static int ZipPathCount
		{
			get
			{
				if (mInstance == null || mInstance.ZipPathItems == null)
					return 0;
				else
					return mInstance.ZipPathItems.Count;
			}
		}

		public static int ZipTypeCount
		{
			get
			{
				if (mInstance == null || mInstance.ZipTypeItems == null)
					return 0;
				else
					return mInstance.ZipTypeItems.Count;
			}
		}

		public static int DependenceWordCount
		{
			get
			{
				if (mInstance == null || mInstance.DependenceWords == null)
					return 0;
				else
					return mInstance.DependenceWords.Count;
			}
		}

		public static int AutoActionCount
		{
			get
			{
				if (mInstance == null || mInstance.AutoActionItems == null)
					return 0;
				else
					return mInstance.AutoActionItems.Count;
			}
		}

		public static List<DragValue> StreamingItems
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.StreamingItems;
			}
		}

		public static List<DragValue> OutputItems
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.OutputItems;
			}
		}

		public static List<DragValue> ZipPathItems
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.ZipPathItems;
			}
		}

		public static List<DragValue> ZipTypeItems
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.ZipTypeItems;
			}
		}

		public static List<DragValue> DependenceWords
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.DependenceWords;
			}
		}

		public static List<VersionValue> VersionItems
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.VersionItems;
			}
		}

		public static List<AutoValue> AutoActionItems
		{
			get
			{
				if (mInstance == null)
					return null;
				else
					return mInstance.AutoActionItems;
			}
		}

		static Setting()
		{
			if (mInstance == null)
			{
				mInstance = AssetDatabase.LoadAssetAtPath("Assets/AutoLauncher/AutoLauncher.Setting/Setting.asset", typeof(SettingObject)) as SettingObject;

				if (mInstance == null)
				{
					// If not found, autocreate the asset object.
					mInstance = ScriptableObject.CreateInstance<SettingObject>();

					string properPath = Path.Combine(Application.dataPath, "AutoLauncher/AutoLauncher.Setting");

					if (!Directory.Exists(properPath))
						AssetDatabase.CreateFolder("Assets", "AutoLauncher/AutoLauncher.Setting");

					string fullPath = Path.Combine(Path.Combine("Assets", "AutoLauncher/AutoLauncher.Setting"), "Setting.asset");
					AssetDatabase.CreateAsset(mInstance, fullPath);
				}
			}
		}

		private static void SaveInfo(rSave save)
		{
			string path = Application.streamingAssetsPath + "/";
			string name = "HTTP.dat";
			string json = Tools.SerializeObject(save);
			Tools.Save(path, name, System.Text.UTF8Encoding.UTF8.GetBytes(json));

			AssetDatabase.Refresh();
		}
	}
}
#endif