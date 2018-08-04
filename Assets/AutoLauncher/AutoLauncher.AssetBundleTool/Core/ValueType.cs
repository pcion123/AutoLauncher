#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	using UnityEngine;
	using System;
	using AutoLauncher.Enum;
	using Object = UnityEngine.Object;

	[Serializable]
	public class DragValue
	{
		public Object obj = null;
		public string value = string.Empty;
	}

	[Serializable]
	public class VersionValue
	{
		public Object obj = null;
		public string value = string.Empty;
		public string ver = string.Empty;
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