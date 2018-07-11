#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;
using Tools = AutoLauncher.Utility.Tools;

namespace AutoLauncher
{
	[System.Serializable]
	public class StringStringDictionary : Utility.SerializableDictionary<string, string> {}

	[ScriptedImporter(1, new string[] {"res","dat"})]
	public class JsonImporter : ScriptedImporter
	{
		[SerializeField] private List<StringStringDictionary> mJsonList = null;
		public StringStringDictionary this[int index]
		{
			get
			{
				if (mJsonList == null)
					return null;

				if (index < 0 || index >= mJsonList.Count)
					return null;

				return mJsonList[index];
			}
			private set
			{
				mJsonList[index] = value;
			}
		}
		public int Count
		{
			get
			{
				return (mJsonList == null) ? 0 : mJsonList.Count;
			}
		}

		public override void OnImportAsset(AssetImportContext ctx)
		{
			byte[] vData = Tools.Load(ctx.assetPath);
			string vJson = System.Text.UTF8Encoding.UTF8.GetString(vData);
			LitJson.JsonData vJsonData = Tools.DeserializeObject(vJson);
			mJsonList = new List<StringStringDictionary>();
			for (int i = 0; i < vJsonData.Count; i++)
			{
				LitJson.JsonData tmp = vJsonData[i];
				StringStringDictionary dict = new StringStringDictionary();
				IEnumerator ite = vJsonData[i].Keys.GetEnumerator();
				while (ite.MoveNext())
				{
					string key = ite.Current.ToString();
					string value = tmp[key].ToString();
					dict.Add(key, value);
				}
				mJsonList.Add(dict);
			}

			string vFileName = Path.GetFileNameWithoutExtension(ctx.assetPath);
			Object asset = AssetDatabase.LoadAssetAtPath(ctx.assetPath, typeof(Object));
			ctx.AddObjectToAsset(vFileName, asset);
			ctx.SetMainObject(asset);
		}
	}
}
#endif
