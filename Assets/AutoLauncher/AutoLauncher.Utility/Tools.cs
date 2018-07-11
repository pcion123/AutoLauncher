#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.IO;
//using Tools = AutoLauncher.Utility.Tools;

namespace AutoLauncher.Utility
{
	public static class Tools : object
	{
		//儲存檔案
		public static void Save (string vPath, byte[] vData, int vXor = 0)
		{
			string zPath = Path.GetDirectoryName(vPath);

			//檢查目錄是否存在
			if (Directory.Exists(zPath) == false) Directory.CreateDirectory(zPath);

			//檢查舊檔案
			if (File.Exists(vPath) == true) File.Delete(vPath);

			//檢查是否需要編碼
			if (vXor != 0)
				vData = XOR(vData, vXor);

			//建立檔案
			using (FileStream vFileStream = new FileStream(vPath, FileMode.Create, FileAccess.Write))
			{
				try
				{
					//調整起始寫入位置
					vFileStream.Seek(0, SeekOrigin.Begin);

					//寫入檔案
					vFileStream.Write(vData, 0, vData.Length);
				}
				finally
				{
					vFileStream.Close();
				}
			}
		}

		//儲存檔案
		public static void Save (string vPath, string vFileName, byte[] vData, int vXor = 0)
		{
			//檢查目錄是否存在
			if (Directory.Exists(vPath) == false) Directory.CreateDirectory(vPath);

			//檢查舊檔案
			if (File.Exists(vPath + vFileName) == true) File.Delete(vPath + vFileName);

			//檢查是否需要編碼
			if (vXor != 0)
				vData = XOR(vData, vXor);

			//建立檔案
			using (FileStream vFileStream = new FileStream(vPath + vFileName, FileMode.Create, FileAccess.Write))
			{
				try
				{
					//調整起始寫入位置
					vFileStream.Seek(0, SeekOrigin.Begin);

					//寫入檔案
					vFileStream.Write(vData, 0, vData.Length);
				}
				finally
				{
					vFileStream.Close();
				}
			}
		}

		//讀入檔案
		public static byte[] Load (string vPath, int vXor = 0, bool vIsDel = false)
		{
			string zPath = Path.GetDirectoryName(vPath);

			//檢查目錄是否存在
			if (Directory.Exists(zPath) == false) return null;

			//檢查舊檔案
			if (File.Exists(vPath) == false) return null;

			byte[] vData = null;

			//建立檔案
			using (FileStream vFileStream = new FileStream(vPath, FileMode.Open, FileAccess.Read))
			{
				try
				{
					//調整起始寫入位置
					vFileStream.Seek(0, SeekOrigin.Begin);

					//建立資料空間
					vData = new byte[vFileStream.Length];

					//取得資料長度
					int vLength = (int)vFileStream.Length;

					//讀出資料
					vFileStream.Read(vData, 0, vLength);

					//檢查是否需要解碼
					if (vXor != 0)
						vData = XOR(vData, vXor);
				}
				finally
				{
					vFileStream.Close();
				}
			}

			if (vIsDel == true)
				File.Delete(vPath);

			return vData;
		}

		//讀入檔案
		public static byte[] Load (string vPath, string vFileName, int vXor = 0, bool vIsDel = false)
		{
			//檢查目錄是否存在
			if (Directory.Exists(vPath) == false) return null;

			//檢查舊檔案
			if (File.Exists(vPath + vFileName) == false) return null;

			byte[] vData = null;

			//建立檔案
			using (FileStream vFileStream = new FileStream(vPath + vFileName, FileMode.Open, FileAccess.Read))
			{
				try
				{
					//調整起始寫入位置
					vFileStream.Seek(0, SeekOrigin.Begin);

					//建立資料空間
					vData = new byte[vFileStream.Length];

					//取得資料長度
					int vLength = (int)vFileStream.Length;

					//讀出資料
					vFileStream.Read(vData, 0, vLength);

					//檢查是否需要解碼
					if (vXor != 0)
						vData = XOR(vData, vXor);
				}
				finally
				{
					vFileStream.Close();
				}
			}

			if (vIsDel == true)
				File.Delete(vPath + vFileName);

			return vData;
		}

		//XOR
		public static byte[] XOR (byte[] vData, int vKey)
		{
			if (vData == null) return null;

			if (vKey == 0) return vData;

			byte[] vKeyAry = System.BitConverter.GetBytes(vKey);

			for (int i = 0; i < vData.Length; i++)
			{
				for (int j = 0; j < vKeyAry.Length; j++)
				{
					vData[i] = System.Convert.ToByte(vData[i] ^ vKeyAry[j]);
				}
			}

			return vData;
		}

		//Json反序列化
		public static LitJson.JsonData DeserializeObject (string vJson)
		{
			return LitJson.JsonMapper.ToObject(vJson);
		}

		//Json反序列化
		public static T DeserializeObject<T> (string vJson)
		{
			return LitJson.JsonMapper.ToObject<T>(vJson);
		}

		//Json序列化
		public static string SerializeObject (object obj)
		{
			return LitJson.JsonMapper.ToJson(obj);
		}
	}
}
#endif
