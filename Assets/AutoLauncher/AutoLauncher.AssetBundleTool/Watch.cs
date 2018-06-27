#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

namespace AutoLauncher.AssetBundleTool
{
	public class Watch : EditorWindow
	{
		private struct WatchData
		{
			public string TypeName;
			public string FileName;
			public int Size;
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
		public static void Open ()
		{
			EditorWindow vScript = EditorWindow.GetWindow(typeof(Watch));
			vScript.autoRepaintOnSceneChange = true;
			vScript.Show();
			vScript.titleContent = new GUIContent("Bundle Watch");
		}

		void OnGUI ()
		{
			EditorGUILayout.LabelField("Asset bundle to watch", EditorStyles.boldLabel);

			if (Selection.activeObject == null)
			{
				EditorGUILayout.LabelField("Select null asset.");
				return;
			}

			if ((Selection.activeObject is DefaultAsset) == false)
			{
				EditorGUILayout.LabelField("Select not Assetbundle.");
				return;
			}

			string vPath = Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject));

			if (vPath.ToLower().Contains(".unity3d") == false)
			{
				EditorGUILayout.LabelField("Select not Assetbundle.");
				return;
			}

			if (mInfo == null)
				mInfo = new FileInfo(vPath);

			EditorGUILayout.LabelField("Asset: " + vPath);
			EditorGUILayout.LabelField("Size: " + mInfo.Length);
			EditorGUILayout.LabelField("CreationTime: " + mInfo.CreationTime);
			EditorGUILayout.LabelField("LastAccessTime: " + mInfo.LastAccessTime);
			EditorGUILayout.LabelField("LastWriteTime: " + mInfo.LastWriteTime);

			if (GUILayout.Button("Watch") == true)
				Watcher(vPath);

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

		void OnInspectorUpdate ()
		{
			Repaint();
		}

		void OnSelectionChange ()
		{
			if (mDataList != null)
				mDataList.Clear();

			if (mTypeStatusMap != null)
				mTypeStatusMap.Clear();

			mInfo = null;
		}

		private void Watcher (string vPath)
		{
			if (string.IsNullOrEmpty(vPath) == true)
				return;

			if (mDataList == null)
				mDataList = new List<WatchData>();

			if (mTypeStatusMap == null)
				mTypeStatusMap = new Dictionary<string, bool>();

			mDataList.Clear();
			mTypeStatusMap.Clear();
			Load(vPath);
		}

		//載入資源
		private void Load (string vPath)
		{
			//檢查檔案是否存在
			if (File.Exists(vPath) == false)
			{
				Debug.LogWarning(vPath + " is not exists");
				return;
			}

			Uri vUri = new Uri(vPath);

			vPath = vUri.AbsoluteUri;

			WWW vBundle = new WWW(vPath);

			if (vBundle.assetBundle.isStreamedSceneAssetBundle == true)
			{
				string[] vNames = vBundle.assetBundle.GetAllScenePaths();

				for (int i = 0; i < vNames.Length; i++)
				{
					WatchData vData = new WatchData();
					vData.TypeName = "Scene";
					vData.FileName = vNames[i];
					mDataList.Add(vData);
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
				UnityEngine.Object[] vAry = vBundle.assetBundle.LoadAllAssets();

				if (vAry == null)
				{

				}
				else
				{
					for (int i = 0; i < vAry.Length; i++)
					{
						WatchData vData = new WatchData();
						vData.TypeName = vAry[i].GetType().FullName;
						vData.FileName = vAry[i].name;
						mDataList.Add(vData);
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

			vBundle.assetBundle.Unload(true);
			vBundle.Dispose();
			vBundle = null;
		}
	}
}
#endif