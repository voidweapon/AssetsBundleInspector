using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class NestedTest : MonoBehaviour
{
    public GameObject A;
    public GameObject B;
    public GameObject C;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 100, 50), "A"))
        {
            ShowDependency(A);
        }
        if (GUI.Button(new Rect(0, 70, 100, 50), "B"))
        {
            ShowDependency(B);
        }
        if (GUI.Button(new Rect(0, 140, 100, 50), "C"))
        {
            ShowDependency(C);
        }
    }

    private void ShowDependency(GameObject obj)
    {
        string guid;
        long localId;
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj.GetInstanceID(), out guid, out localId);
        var dpcies = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(guid), false);
        foreach (var item in dpcies)
        {

            Debug.LogFormat("[{0}]: {1}",AssetDatabase.AssetPathToGUID(item), item);
        }
    }
}
