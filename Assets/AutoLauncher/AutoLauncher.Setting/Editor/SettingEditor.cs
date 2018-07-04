#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.IO;

namespace AutoLauncher
{
	[CustomEditor(typeof(SettingObject))]
	public class SettingEditor : UnityEditor.Editor
	{
		[MenuItem("AutoLauncher/Setting/Setting")]
		public static void SettingMenu ()
		{
			Selection.activeObject = Setting.Instance;
		}

		GUIContent userLabel = new GUIContent("User [?]:", "FTP User Account");
		GUIContent passwordLabel = new GUIContent("Password [?]:", "FTP User Password");
		GUIContent ipLabel = new GUIContent("IP [?]:", "FTP IP Address");
		GUIContent httpLabel = new GUIContent("HTTP [?]:", "HTTP IP Address");
		GUIContent verLabel = new GUIContent("Ver [?]:", "HTTP Data Version");
		GUIContent buildtypeLabel = new GUIContent("BuildType [?]:", "Build Bundle Type");
		GUIContent buildlanguageLabel = new GUIContent("BuildLanguage [?]:", "Build Language");
		GUIContent encryptionkeyLabel = new GUIContent("EncryptionKey [?]:", "Build Key");

		GUIContent inputassetsfolderLabel = new GUIContent("Input Assets Folder [?]:", "Input Assets Folder Path");
		GUIContent outputassetsfolderLabel = new GUIContent("Output Assets Folder [?]:", "Output Assets Folder Path");
		GUIContent downloadassetsfolderLabel = new GUIContent("Download Assets Folder [?]:", "Download Assets Folder Path");

		ReorderableList mStreamingList;
		ReorderableList mOutputList;
		ReorderableList mZipPathList;
		ReorderableList mZipTypeList;
		ReorderableList mDependenceWordList;
		ReorderableList mAutoActionList;

		//檢查是否是資料夾
		private bool CheckIsFolder (string vPath)
		{
			FileAttributes vAttr = File.GetAttributes(@vPath);
			if ((vAttr & FileAttributes.Directory) == FileAttributes.Directory)
				return true;
			else
				return false;
		}

		private static string GetReplaceLangPath (string vPath)
		{
			string[] strs = vPath.Split('/');
			if (strs.Length >= 3)
				strs[2] = "{0}";
			return string.Join("/", strs, 0, strs.Length);
		}

		private void OnEnable ()
		{
			mStreamingList = new ReorderableList(serializedObject, serializedObject.FindProperty("StreamingItems"), true, true, true, true);
			mStreamingList.drawHeaderCallback = DrawStreamAssetHeader;
			mStreamingList.drawElementCallback = DrawStreamAssetElement;

			mOutputList = new ReorderableList(serializedObject, serializedObject.FindProperty("OutputItems"), true, true, true, true);
			mOutputList.drawHeaderCallback = DrawOutputAssetHeader;
			mOutputList.drawElementCallback = DrawOutputAssetElement;

			mZipPathList = new ReorderableList(serializedObject, serializedObject.FindProperty("ZipPathItems"), true, true, true, true);
			mZipPathList.drawHeaderCallback = DrawZipPathHeader;
			mZipPathList.drawElementCallback = DrawZipPathElement;

			mZipTypeList = new ReorderableList(serializedObject, serializedObject.FindProperty("ZipTypeItems"), true, true, true, true);
			mZipTypeList.drawHeaderCallback = DrawZipTypeHeader;
			mZipTypeList.drawElementCallback = DrawZipTypeElement;

			mDependenceWordList = new ReorderableList(serializedObject, serializedObject.FindProperty("DependenceWords"), true, true, true, true);
			mDependenceWordList.drawHeaderCallback = DrawDependenceWordHeader;
			mDependenceWordList.drawElementCallback = DrawDependenceWordElement;

			mAutoActionList = new ReorderableList(serializedObject, serializedObject.FindProperty("AutoActionItems"), true, true, true, true);
			mAutoActionList.drawHeaderCallback = DrawAutoActionHeader;
			mAutoActionList.drawElementCallback = DrawAutoActionElement;
		}

		public override void OnInspectorGUI ()
		{
			SettingGUI();

			serializedObject.Update();
			mStreamingList.DoLayoutList();
			mOutputList.DoLayoutList();
			mZipPathList.DoLayoutList();
			mZipTypeList.DoLayoutList();
			mDependenceWordList.DoLayoutList();
			mAutoActionList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}

		private void SettingGUI ()
		{
			//User
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(userLabel);
			Setting.User = EditorGUILayout.TextField(Setting.User);
			EditorGUILayout.EndHorizontal();

			//Password
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(passwordLabel);
			Setting.PWD = EditorGUILayout.TextField(Setting.PWD);
			EditorGUILayout.EndHorizontal();

			//IP
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(ipLabel);
			Setting.IP = EditorGUILayout.TextField(Setting.IP);
			EditorGUILayout.EndHorizontal();

			//HTTP IP
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(httpLabel);
			Setting.HTTP = EditorGUILayout.TextField(Setting.HTTP);
			EditorGUILayout.EndHorizontal();

			//HTTP Ver
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(verLabel);
			Setting.Ver = EditorGUILayout.TextField(Setting.Ver);
			EditorGUILayout.EndHorizontal();

			//EncryptionKey
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(encryptionkeyLabel);
			Setting.EncryptionKeyStr = EditorGUILayout.TextField(Setting.EncryptionKeyStr);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			//BuildType BuildTarget
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(buildtypeLabel);
			Setting.BuildType = (BuildTarget)EditorGUILayout.EnumPopup(Setting.BuildType);
			EditorGUILayout.EndHorizontal();

			//BuildLanguage
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(buildlanguageLabel);
			Setting.BuildLanguage = (eLanguage)EditorGUILayout.EnumPopup(Setting.BuildLanguage);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			//InputAssetsFolder Path
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(inputassetsfolderLabel);
			Setting.InputAssetsFolder = EditorGUILayout.TextField(Setting.InputAssetsFolder);
			EditorGUILayout.EndHorizontal();

			//OutputAssetsFolder Path
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(outputassetsfolderLabel);
			Setting.OutputAssetsFolder = EditorGUILayout.TextField(Setting.OutputAssetsFolder);
			EditorGUILayout.EndHorizontal();

			//DownloadAssetsFolder Path
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(downloadassetsfolderLabel);
			Setting.DownloadAssetsFolder = EditorGUILayout.TextField(Setting.DownloadAssetsFolder);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
		}

		private void DrawStreamAssetHeader (Rect rect)
		{
			GUI.Label(rect, "StreamingAssets");
		}

		private void DrawOutputAssetHeader (Rect rect)
		{
			GUI.Label(rect, "OutputAssets");
		}

		private void DrawZipPathHeader (Rect rect)
		{
			GUI.Label(rect, "ZipPaths");
		}

		private void DrawZipTypeHeader (Rect rect)
		{
			GUI.Label(rect, "ZipTypes");
		}

		private void DrawDependenceWordHeader (Rect rect)
		{
			GUI.Label(rect, "DependenceWords");
		}

		private void DrawAutoActionHeader (Rect rect)
		{
			GUI.Label(rect, "AutoActions");
		}

		private void DrawStreamAssetElement (Rect rect, int index, bool selected, bool focused)
		{
			SerializedProperty itemData = mStreamingList.serializedProperty.GetArrayElementAtIndex(index);
			Object obj = itemData.FindPropertyRelative("obj").objectReferenceValue;
			string value = itemData.FindPropertyRelative("value").stringValue;
			if (string.IsNullOrEmpty(value) && obj == null)
			{
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("obj"), GUIContent.none);
			}
			else
			{
				if (string.IsNullOrEmpty(value) && obj != null)
				{
					string[] paths = AssetDatabase.GetAssetPath(obj).Split('/');
					itemData.FindPropertyRelative("value").stringValue = paths[paths.Length - 1];
					itemData.FindPropertyRelative("obj").objectReferenceValue = null;
				}
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("value"), GUIContent.none);
			}
		}

		private void DrawOutputAssetElement (Rect rect, int index, bool selected, bool focused)
		{
			SerializedProperty itemData = mOutputList.serializedProperty.GetArrayElementAtIndex(index);
			Object obj = itemData.FindPropertyRelative("obj").objectReferenceValue;
			string value = itemData.FindPropertyRelative("value").stringValue;
			if (string.IsNullOrEmpty(value) && obj == null)
			{
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("obj"), GUIContent.none);
			}
			else
			{
				if (string.IsNullOrEmpty(value) && obj != null)
				{
					string[] paths = AssetDatabase.GetAssetPath(obj).Split('/');
					itemData.FindPropertyRelative("value").stringValue = paths[paths.Length - 1];
					itemData.FindPropertyRelative("obj").objectReferenceValue = null;
				}
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("value"), GUIContent.none);
			}
		}

		private void DrawZipPathElement (Rect rect, int index, bool selected, bool focused)
		{
			SerializedProperty itemData = mZipPathList.serializedProperty.GetArrayElementAtIndex(index);
			Object obj = itemData.FindPropertyRelative("obj").objectReferenceValue;
			string value = itemData.FindPropertyRelative("value").stringValue;
			if (string.IsNullOrEmpty(value) && obj == null)
			{
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("obj"), GUIContent.none);
			}
			else
			{
				if (string.IsNullOrEmpty(value) && obj != null)
				{
					itemData.FindPropertyRelative("value").stringValue = GetReplaceLangPath(AssetDatabase.GetAssetPath(obj).Replace("Assets/" + Setting.InputAssetsFolder + "/", "")) + "/";
					itemData.FindPropertyRelative("obj").objectReferenceValue = null;
				}
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("value"), GUIContent.none);
			}
		}

		private void DrawZipTypeElement (Rect rect, int index, bool selected, bool focused)
		{
			SerializedProperty itemData = mZipTypeList.serializedProperty.GetArrayElementAtIndex(index);
			Object obj = itemData.FindPropertyRelative("obj").objectReferenceValue;
			string value = itemData.FindPropertyRelative("value").stringValue;
			if (string.IsNullOrEmpty(value) && obj == null)
			{
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("obj"), GUIContent.none);
			}
			else
			{
				if (string.IsNullOrEmpty(value) && obj != null)
				{
					string[] paths = AssetDatabase.GetAssetPath(obj).Split('/');
					string[] extensions = paths[paths.Length - 1].Split('.');
					itemData.FindPropertyRelative("value").stringValue = "*." + extensions[extensions.Length - 1];
					itemData.FindPropertyRelative("obj").objectReferenceValue = null;
				}
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("value"), GUIContent.none);
			}
		}

		private void DrawDependenceWordElement (Rect rect, int index, bool selected, bool focused)
		{
			SerializedProperty itemData = mDependenceWordList.serializedProperty.GetArrayElementAtIndex(index);
			Object obj = itemData.FindPropertyRelative("obj").objectReferenceValue;
			string value = itemData.FindPropertyRelative("value").stringValue;
			if (string.IsNullOrEmpty(value) && obj == null)
			{
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("obj"), GUIContent.none);
			}
			else
			{
				if (string.IsNullOrEmpty(value) && obj != null)
				{
					if (!CheckIsFolder(AssetDatabase.GetAssetPath(obj)))
					{
						string[] paths = AssetDatabase.GetAssetPath(obj).Split('/');
						string path = string.Join("/", paths, 3, paths.Length - 3);
						itemData.FindPropertyRelative("value").stringValue = GetReplaceLangPath(AssetDatabase.GetAssetPath(obj));;
						itemData.FindPropertyRelative("obj").objectReferenceValue = null;
					}
					else
					{
						itemData.FindPropertyRelative("obj").objectReferenceValue = null;
					}
				}
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(rect, itemData.FindPropertyRelative("value"), GUIContent.none);
			}
		}

		private void DrawAutoActionElement (Rect rect, int index, bool selected, bool focused)
		{
			SerializedProperty itemData = mAutoActionList.serializedProperty.GetArrayElementAtIndex(index);
			Object obj = itemData.FindPropertyRelative("obj").objectReferenceValue;
			string value = itemData.FindPropertyRelative("value").stringValue;
			if (string.IsNullOrEmpty(value) && obj == null)
			{
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("acion"), GUIContent.none);
				if (itemData.FindPropertyRelative("acion").enumValueIndex == 1 || itemData.FindPropertyRelative("acion").enumValueIndex == 2 || itemData.FindPropertyRelative("acion").enumValueIndex == 3)
				{
					EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("obj"), GUIContent.none);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(value) && obj != null)
				{
					itemData.FindPropertyRelative("value").stringValue = GetReplaceLangPath(AssetDatabase.GetAssetPath(obj));
					itemData.FindPropertyRelative("obj").objectReferenceValue = null;
				}
				rect.y += 2;
				rect.height = EditorGUIUtility.singleLineHeight;
				EditorGUI.PropertyField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("acion"), GUIContent.none);
				if (itemData.FindPropertyRelative("acion").enumValueIndex == 1 || itemData.FindPropertyRelative("acion").enumValueIndex == 2 || itemData.FindPropertyRelative("acion").enumValueIndex == 3)
				{
					EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), itemData.FindPropertyRelative("value"), GUIContent.none);
				}
			}
		}
	}
}
#endif
