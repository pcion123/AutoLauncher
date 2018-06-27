#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;

namespace AutoLauncher
{
	[Serializable]
	public class DragValue
	{
		public Object obj = null;
		public string value = string.Empty;
	}

	[Serializable]
	public class AutoValue
	{
		public eBuildAcion acion = eBuildAcion.None;
		public Object obj = null;
		public string value = string.Empty;
	}
}
#endif