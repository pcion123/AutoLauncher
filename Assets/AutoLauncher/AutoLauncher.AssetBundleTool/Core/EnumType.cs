#if UNITY_EDITOR
namespace AutoLauncher.Enum
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