using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ABInspector
{
    [System.Serializable]
    public class ABInspectorItemData
    {
        public string GUID;
        public string MetaMD5;

        public List<string> Dependency;
        public List<string> ReverseDependency; 
        [System.NonSerialized]
        public bool isOld = false;
    }

    [System.Serializable]
    public class ItemDataCollection
    {
        public List<ABInspectorItemData> items;
    }
}

