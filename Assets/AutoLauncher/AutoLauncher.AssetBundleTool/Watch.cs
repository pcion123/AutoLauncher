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

	public class Watch : EditorWindow
	{
		private struct WatchData
		{
			public string TypeName;
			public string FileName;
		}

		private class WatchDataComparer : IComparer<WatchData>
		{
			public int Compare(WatchData stat1, WatchData stat2)
			{
				return stat1.TypeName.CompareTo(stat2.TypeName);
			}
		}

		private List<WatchData> mDataList = null;
		private Dictionary<string, bool> mTypeStatusMap = null;
		private FileInfo mInfo = null;
		private Vector2 mScrollPos = Vector2.zero;

		[MenuItem("AutoLauncher/Window/Bundle Watch", false, 999)]
		public static void Open()
		{
			EditorWindow script = EditorWindow.GetWindow(typeof(Watch));
			script.autoRepaintOnSceneChange = true;
			script.Show();
			script.titleContent = new GUIContent("Bundle Watch");
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Asset bundle to watch", EditorStyles.boldLabel);

			if (Selection.activeObject == null)
			{
				EditorGUILayout.LabelField("Select null asset.");
				return;
			}

			if (!(Selection.activeObject is DefaultAsset))
			{
				EditorGUILayout.LabelField("Select not Assetbundle.");
				return;
			}

			string path = Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject));

			if (!path.ToLower().Contains(".unity3d"))
			{
				EditorGUILayout.LabelField("Select not Assetbundle.");
				return;
			}

			if (mInfo == null)
				mInfo = new FileInfo(path);

			EditorGUILayout.LabelField("Asset: " + path);
			EditorGUILayout.LabelField("Size: " + mInfo.Length);
			EditorGUILayout.LabelField("CreationTime: " + mInfo.CreationTime);
			EditorGUILayout.LabelField("LastAccessTime: " + mInfo.LastAccessTime);
			EditorGUILayout.LabelField("LastWriteTime: " + mInfo.LastWriteTime);

			if (GUILayout.Button("Watch"))
				Watcher(path);

			if ((mDataList == null) || (mDataList.Count == 0))
				return;

			mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));
			string curType = "";
			foreach (WatchData data in mDataList)
			{
				if (curType != data.TypeName)
				{
					EditorGUILayout.BeginHorizontal();
					mTypeStatusMap[data.TypeName] = EditorGUILayout.Foldout(mTypeStatusMap[data.TypeName], data.TypeName);
					EditorGUILayout.EndHorizontal();
					curType = data.TypeName;
				}

				if (mTypeStatusMap[data.TypeName])
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("    " + data.FileName);
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndScrollView();
		}

		void OnInspectorUpdate()
		{
			Repaint();
		}

		void OnSelectionChange()
		{
			if (mDataList != null)
				mDataList.Clear();

			if (mTypeStatusMap != null)
				mTypeStatusMap.Clear();

			mInfo = null;
		}

		private void Watcher(string path)
		{
			if (string.IsNullOrEmpty(path))
				return;

			if (mDataList == null)
				mDataList = new List<WatchData>();

			if (mTypeStatusMap == null)
				mTypeStatusMap = new Dictionary<string, bool>();

			mDataList.Clear();
			mTypeStatusMap.Clear();
			Load(path);
		}

		//載入資源
		private void Load(string path)
		{
			//檢查檔案是否存在
			if (!File.Exists(path))
			{
				Debug.LogWarning(path + " is not exists");
				return;
			}

			Uri uri = new Uri(path);
			path = uri.AbsoluteUri;
			WWW bundle = new WWW(path);
			if (bundle.assetBundle.isStreamedSceneAssetBundle == true)
			{
				string[] names = bundle.assetBundle.GetAllScenePaths();
				for (int i = 0; i < names.Length; i++)
				{
					WatchData data = new WatchData();
					data.TypeName = "Scene";
					data.FileName = names[i];
					mDataList.Add(data);
				}

				foreach (WatchData data in mDataList)
				{
					if (mTypeStatusMap.ContainsKey(data.TypeName))
					{
					}
					else
					{
						mTypeStatusMap.Add(data.TypeName, false);
					}
				}

				mDataList.Sort(new WatchDataComparer());
			}
			else
			{
				Object[] array = bundle.assetBundle.LoadAllAssets();
				if (array == null)
				{

				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						WatchData data = new WatchData();
						data.TypeName = array[i].GetType().FullName;
						data.FileName = array[i].name;
						mDataList.Add(data);
					}

					foreach (WatchData data in mDataList)
					{
						if (mTypeStatusMap.ContainsKey(data.TypeName))
						{
						}
						else
						{
							mTypeStatusMap.Add(data.TypeName, false);
						}
					}

					mDataList.Sort(new WatchDataComparer());
				}
			}
			bundle.assetBundle.Unload(true);
			bundle.Dispose();
			bundle = null;
		}
	}
}
#endif