#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AutoLauncher.Utility
{
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TKey> mKeys;
		[SerializeField]
		private List<TValue> mValues;

		public void OnBeforeSerialize()
		{
			mKeys = new List<TKey>(this.Count);
			mValues = new List<TValue>(this.Count);
			foreach (var kvp in this)
			{
				mKeys.Add(kvp.Key);
				mValues.Add(kvp.Value);
			}
		}

		public void OnAfterDeserialize()
		{
			this.Clear();
			int count = Mathf.Min(mKeys.Count, mValues.Count);
			for (int i = 0; i < count; ++i)
			{
				this.Add(mKeys[i], mValues[i]);
			}
		}
	}
}
#endif