#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AutoLauncher
{
	[CustomEditor(typeof(JsonImporter))]
	public class JsonImporterEditor : ScriptedImporterEditor
	{
		private JsonImporter mImporter = null;
		private List<bool> mOpenList = null;

		public override void OnInspectorGUI()
		{
			if (mImporter == null)
				mImporter = serializedObject.targetObject as JsonImporter;

			if (mOpenList == null)
				mOpenList = new List<bool>();

			if (mOpenList.Count != mImporter.Count)
			{
				mOpenList.Clear();
				for (int i = 0; i < mImporter.Count; i++)
					mOpenList.Add(false);
			}

			for (int i = 0; i < mImporter.Count; i++)
			{
				mOpenList[i] = EditorGUILayout.Foldout(mOpenList[i], string.Format("index={0}", i.ToString("D3")));
				if (mOpenList[i])
				{
					Dictionary<string, string> dict = mImporter[i];
					IEnumerator ite = dict.Keys.GetEnumerator();
					while (ite.MoveNext())
					{
						string key = ite.Current.ToString();
						string value;
						dict.TryGetValue(key, out value);
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(key, GUILayout.Width(120));
						EditorGUILayout.TextField(value, GUILayout.Width(EditorGUIUtility.currentViewWidth - 120));
						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					
				}
			}
		}
	}
}
#endif