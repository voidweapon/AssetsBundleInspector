using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace ABInspector
{
    public class ABInspectorDataManager : IDisposable
    {
        private readonly string dataFilePath = Path.GetFullPath(Application.dataPath + "../AssetBundleInspectorData/");
        private ItemDataCollection m_dataCollection = null;
        private Dictionary<string, ABInspectorItemData> dataMap = null;
        private bool inited = false;
        /// <summary>
        /// 加载资源文件
        /// </summary>
        public void Init()
        {
            if (!inited)
            {
                using (StreamReader reader = new StreamReader(dataFilePath + "ABData.txt"))
                {
                    string data = reader.ReadToEnd();
                    m_dataCollection = JsonUtility.FromJson<ItemDataCollection>(data);
                    dataMap = new Dictionary<string, ABInspectorItemData>();
                    foreach (var item in m_dataCollection.items)
                    {
                        dataMap[item.GUID] = item;
                    }
                }
                inited = true;
            }
        }

        /// <summary>
        /// 验证每个文件是否被更改过
        /// </summary>
        public void VerifyAssetsMD5() 
        {
            foreach (var item in m_dataCollection.items)
            {
                //通过guid获取文件名，然后读取对应的.meta文件
                //并计算MD5和储存的MD5比较，如果不同则标记
            }
        }


        /// <summary>
        /// 计算文件的MD5值，用于比较文件是否被更改
        /// </summary>
        /// <returns>The file MD.</returns>
        /// <param name="filePath">File path.</param>
        private static string CaculateFileMD5(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string data = reader.ReadToEnd();
                using(MD5 md5hash = MD5.Create())
                {
                    return GetMd5Hash(md5hash, data);
                }
            }
        }
        #region MD5 Function
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    m_dataCollection.items.Clear();
                    m_dataCollection = null;
                    dataMap.Clear();
                    dataMap = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ABInspectorDataManager() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}

