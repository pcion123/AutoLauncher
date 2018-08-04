#if UNITY_EDITOR
namespace AutoLauncher.AssetBundleTool
{
	//CRC結構
	public struct rCRC
	{
		public string FileName;
		public uint CRC;

		public rCRC(string fileName, uint crc)
		{
			FileName = fileName;
			CRC = crc;
		}
	}

	//更新紀錄結構
	public struct rMD5Info
	{
		public string FlieName; //檔名
		public string MD5Code;  //MD5編碼
	}

	//資源檔結構
	[System.Serializable]
	public struct rRes
	{
		public string Version;  //版本號
		public string FileName; //檔名
		public string Path;     //路徑
		public long FileSize;   //檔案大小
		public string MD5Code;  //MD5編碼
	}
}
#endif
