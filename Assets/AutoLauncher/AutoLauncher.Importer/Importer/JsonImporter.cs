#if UNITY_EDITOR
namespace AutoLauncher.Importer
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditor.Experimental.AssetImporters;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using Object = UnityEngine.Object;
	using Tools = AutoLauncher.Utility.Tools;

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
			byte[] data = Tools.Load(ctx.assetPath);
			string json = System.Text.UTF8Encoding.UTF8.GetString(data);
			LitJson.JsonData jsonData = Tools.DeserializeObject(json);
			mJsonList = new List<StringStringDictionary>();
			for (int i = 0; i < jsonData.Count; i++)
			{
				LitJson.JsonData tmp = jsonData[i];
				StringStringDictionary dict = new StringStringDictionary();
				IEnumerator ite = jsonData[i].Keys.GetEnumerator();
				while (ite.MoveNext())
				{
					string key = ite.Current.ToString();
					string value = tmp[key].ToString();
					dict.Add(key, value);
				}
				mJsonList.Add(dict);
			}

			string fileName = Path.GetFileNameWithoutExtension(ctx.assetPath);
			Object asset = AssetDatabase.LoadAssetAtPath(ctx.assetPath, typeof(Object));
			ctx.AddObjectToAsset(fileName, asset);
			ctx.SetMainObject(asset);
		}
	}
}
#endif
