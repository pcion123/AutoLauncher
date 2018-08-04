#if UNITY_EDITOR
namespace AutoLauncher.Utility
{
	using System.Collections;
	using System.IO;

	public static class Tools : object
	{
		//儲存檔案
		public static void Save(string path, byte[] data, int xor = 0)
		{
			string zPath = Path.GetDirectoryName(path);

			//檢查目錄是否存在
			if (!Directory.Exists(zPath))
				Directory.CreateDirectory(zPath);

			//檢查舊檔案
			if (File.Exists(path))
				File.Delete(path);

			//檢查是否需要編碼
			if (xor != 0)
				data = XOR(data, xor);

			//建立檔案
			using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				try
				{
					//調整起始寫入位置
					fileStream.Seek(0, SeekOrigin.Begin);

					//寫入檔案
					fileStream.Write(data, 0, data.Length);
				}
				finally
				{
					fileStream.Close();
				}
			}
		}

		//儲存檔案
		public static void Save(string path, string fileName, byte[] data, int xor = 0)
		{
			//檢查目錄是否存在
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			//檢查舊檔案
			if (File.Exists(path + fileName))
				File.Delete(path + fileName);

			//檢查是否需要編碼
			if (xor != 0)
				data = XOR(data, xor);

			//建立檔案
			using (FileStream fileStream = new FileStream(path + fileName, FileMode.Create, FileAccess.Write))
			{
				try
				{
					//調整起始寫入位置
					fileStream.Seek(0, SeekOrigin.Begin);

					//寫入檔案
					fileStream.Write(data, 0, data.Length);
				}
				finally
				{
					fileStream.Close();
				}
			}
		}

		//讀入檔案
		public static byte[] Load(string path, int xor = 0, bool isDel = false)
		{
			string zPath = Path.GetDirectoryName(path);

			//檢查目錄是否存在
			if (!Directory.Exists(zPath))
				return null;

			//檢查舊檔案
			if (!File.Exists(path))
				return null;

			byte[] data = null;

			//建立檔案
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				try
				{
					//調整起始寫入位置
					fileStream.Seek(0, SeekOrigin.Begin);

					//建立資料空間
					data = new byte[fileStream.Length];

					//取得資料長度
					int vLength = (int)fileStream.Length;

					//讀出資料
					fileStream.Read(data, 0, vLength);

					//檢查是否需要解碼
					if (xor != 0)
						data = XOR(data, xor);
				}
				finally
				{
					fileStream.Close();
				}
			}

			if (isDel)
				File.Delete(path);

			return data;
		}

		//讀入檔案
		public static byte[] Load(string path, string fileName, int xor = 0, bool isDel = false)
		{
			//檢查目錄是否存在
			if (!Directory.Exists(path))
				return null;

			//檢查舊檔案
			if (!File.Exists(path + fileName))
				return null;

			byte[] data = null;

			//建立檔案
			using (FileStream fileStream = new FileStream(path + fileName, FileMode.Open, FileAccess.Read))
			{
				try
				{
					//調整起始寫入位置
					fileStream.Seek(0, SeekOrigin.Begin);

					//建立資料空間
					data = new byte[fileStream.Length];

					//取得資料長度
					int vLength = (int)fileStream.Length;

					//讀出資料
					fileStream.Read(data, 0, vLength);

					//檢查是否需要解碼
					if (xor != 0)
						data = XOR(data, xor);
				}
				finally
				{
					fileStream.Close();
				}
			}

			if (isDel)
				File.Delete(path + fileName);

			return data;
		}

		//XOR
		public static byte[] XOR(byte[] data, int key)
		{
			if (data == null)
				return null;

			if (key == 0)
				return data;

			byte[] keyArray = System.BitConverter.GetBytes(key);
			for (int i = 0; i < data.Length; i++)
			{
				for (int j = 0; j < keyArray.Length; j++)
				{
					data[i] = System.Convert.ToByte(data[i] ^ keyArray[j]);
				}
			}
			return data;
		}

		//Json反序列化
		public static LitJson.JsonData DeserializeObject(string json)
		{
			return LitJson.JsonMapper.ToObject(json);
		}

		//Json反序列化
		public static T DeserializeObject<T>(string json)
		{
			return LitJson.JsonMapper.ToObject<T>(json);
		}

		//Json序列化
		public static string SerializeObject(object obj)
		{
			return LitJson.JsonMapper.ToJson(obj);
		}
	}
}
#endif
