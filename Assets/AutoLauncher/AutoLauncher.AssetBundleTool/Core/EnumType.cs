#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;

namespace AutoLauncher.AssetBundleTool
{
	public enum eLanguage
	{
		None,
		EN,
		TW,
		CN,
		JP,
		KR
	}

	public enum eBuildAcion
	{
		None,
		BundleUncompress,
		BundleCompress,
		Zip,
		Move2Output,
		Move2Streaming,
		BuildVersion,
		DownloadVersion,
		Upload
	}
}
#endif