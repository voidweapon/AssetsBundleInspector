using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace ABInspector
{
    public class ABInspectorDataManager : IDisposable
    {
        private readonly string dataFilePath = Path.GetFullPath(Application.dataPath + "./../AssetBundleInspectorData/");
        private ItemDataCollection m_dataCollection = null;
        private Dictionary<string, ABInspectorItemData> dataMap = null;
        public bool Ready { get {return m_ready; } }
       
        private bool inited = false;
        private bool m_ready = false;
        /// <summary>
        /// 加载资源文件
        /// </summary>
        public void Init()
        {
            if (!inited)
            {
                LoadDataFile();
                inited = true;
            }
        }

        private void LoadDataFile()
        {
            if(File.Exists(dataFilePath + "ABData.json") == false)
            {
                Directory.CreateDirectory(dataFilePath);
                m_dataCollection = new ItemDataCollection()
                {
                    items = new List<ABInspectorItemData>(),
                };
                dataMap = new Dictionary<string, ABInspectorItemData>();
                GenerateDependencyMap();
            }
            else
            {
                using (StreamReader reader = new StreamReader(dataFilePath + "ABData.json"))
                {
                    string data = reader.ReadToEnd();
                    m_dataCollection = JsonUtility.FromJson<ItemDataCollection>(data);
                    dataMap = new Dictionary<string, ABInspectorItemData>();
                    foreach (var item in m_dataCollection.items)
                    {
                        dataMap[item.GUID] = item;
                    }
                }
                //TODO:文件增删检测

                //校验文件
                VerifyAssetsMD5();
            }
            m_ready = true;
        }
        private void SaveDataFile()
        {
            using (StreamWriter writer = new StreamWriter(dataFilePath + "ABData.json"))
            {
                string json = JsonUtility.ToJson(m_dataCollection);
                writer.Write(json);
            }
        }

        /// <summary>
        /// 验证每个文件是否被更改过
        /// </summary>
        public void VerifyAssetsMD5() 
        {
            string path = string.Empty;
            foreach (var item in m_dataCollection.items)
            {
                //通过guid获取文件名，然后读取对应的.meta文件
                //并计算MD5和储存的MD5比较，如果不同则标记
                path = AssetDatabase.GUIDToAssetPath(item.GUID);
                if (path.Contains("SampleScene"))
                {
                    Debug.Log(1);
                }
                using (StreamReader reader = new StreamReader(path))
                {
                    string data = reader.ReadToEnd();
                    using (MD5 md5hash = MD5.Create())
                    {
                        item.isOld = !VerifyMd5Hash(md5hash, data, item.MetaMD5);
                    }
                }
                if (item.isOld)
                {
                    Debug.LogFormat("item[{0}] is Old", item.GUID);
                    GetNodeDependency(path, item.GUID);
                }
            }
        }

        public ABInspectorItemData GetItemDataByGUID(string guid)
        {
            ABInspectorItemData result = null;
            if(dataMap.TryGetValue(guid, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 生成资源依赖关系
        /// 记录正向直接引用和反向直接引用
        /// 然后保存到文件
        /// </summary>
        public void GenerateDependencyMap()
        {
            dataMap = new Dictionary<string, ABInspectorItemData>();

            var guids = AssetDatabase.FindAssets("", new string[] { "Assets" });
            string path = string.Empty;

            int index = 0;

            foreach (var nodeGUID in guids)
            {
                path = AssetDatabase.GUIDToAssetPath(nodeGUID);
                if (path.Contains(".") == false) continue;
                if (path.Contains(".manifest") == true) continue;
                if (path.Contains("/Editor/")) continue;
                EditorUtility.DisplayProgressBar("GenerateDependencyMap", path, (index++ * 1f) / (guids.Length * 1f));
                GetNodeDependency(path, nodeGUID);
            }

            SaveDataFile();
            EditorUtility.ClearProgressBar();
        }

        private void GetNodeDependency(string path,string nodeGUID)
        {
            ABInspectorItemData currentNode = null;
            ABInspectorItemData result = null;
            string dpdcGUID = string.Empty;
            string[] depsPathAry;
            //获取节点
            if (dataMap.TryGetValue(nodeGUID, out currentNode) == false)
            {
                currentNode = CreateDataNode(nodeGUID, path);
                m_dataCollection.items.Add(currentNode);
                dataMap[nodeGUID] = currentNode;
            }
            //获取直接依赖项
            depsPathAry = AssetDatabase.GetDependencies(path, false);
            foreach (var dpdcPath in depsPathAry)
            {
                dpdcGUID = AssetDatabase.AssetPathToGUID(dpdcPath);
                //去掉自身引用
                if (dpdcGUID == nodeGUID) continue;

                if (dataMap.TryGetValue(dpdcGUID, out result) == false)
                {
                    result = CreateDataNode(dpdcGUID, dpdcPath);
                    m_dataCollection.items.Add(result);
                    dataMap[dpdcGUID] = result;
                }

                //记录正向直接引用
                if (currentNode.Dependency.Contains(dpdcGUID) == false)
                {
                    currentNode.Dependency.Add(dpdcGUID);
                }

                //记录反向直接引用
                if (result.ReverseDependency.Contains(nodeGUID) == false)
                {
                    result.ReverseDependency.Add(nodeGUID);
                }
            }
        }

        private static ABInspectorItemData CreateDataNode(string guid, string path)
        {
            string metaMD5 = CaculateFileMD5(path);
            return new ABInspectorItemData()
            {
                GUID = guid,
                MetaMD5 = metaMD5,
                Dependency = new List<string>(),
                ReverseDependency = new List<string>(),
            };
        }

        #region MD5 Function
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

