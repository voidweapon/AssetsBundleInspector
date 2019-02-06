using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ABInspector
{
    [System.Serializable]
    public class ABINspectorItemData
    {
        public string GUID;
        public string MetaMD5;

        [System.NonSerialized]
        public bool isOld = false;
    }

    [System.Serializable]
    public class ItemDataCollection
    {
        public List<ABINspectorItemData> items;
    }
}

