using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.IO;
using System;

namespace ABInspector
{
    /// <summary>
    /// 树形视图
    /// </summary>
    public class ABInspectorOrganizationTreeView : TreeView
    {
        static OrganizationTreeViewItem root;
        public Func<string, ABInspectorItemData> GetABInspectorItemDataByGUID;
        public List<Rect> ColumnRects = new List<Rect>();
        public ABInspectorOrganizationTreeView(TreeViewState state, MultiColumnHeaderState headerState)
            : base(state, new MultiColumnHeader(headerState))
        {
            root = new OrganizationTreeViewItem(-1, -1, "");
            root.children = new List<TreeViewItem>();
            showBorder = true;
        }

        #region TreeView Implementation
        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();
            ColumnRects.Clear();
            for (int i = 0; i < multiColumnHeader.state.columns.Length; i++)
            {
                ColumnRects.Add(multiColumnHeader.GetColumnRect(i));
            }
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            Color old = GUI.color;
            base.RowGUI(args);
            GUI.color = old;

            var item = (args.item as OrganizationTreeViewItem);
            Rect TypeRect = new Rect(ColumnRects[1].x, args.rowRect.y, ColumnRects[1].width, ColumnRects[1].height);
            GUI.Label(TypeRect, item.Type);

            Rect CountRect = new Rect(ColumnRects[2].x, args.rowRect.y, ColumnRects[2].width, ColumnRects[2].height);
            GUI.Label(CountRect, item.Count.ToString());
        }
        protected override TreeViewItem BuildRoot()
        {
            return root;
        } 
        #endregion

        #region MultiColumnHeaderState
        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
            };

            retVal[0].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 140;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Type", "Asset Type");
            retVal[1].minWidth = 35;
            retVal[1].width = 100;
            retVal[1].maxWidth = 200;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = true;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Count", "Reverse dependency count.");
            retVal[2].minWidth = 30;
            retVal[2].width = 75;
            retVal[2].maxWidth = 100;
            retVal[2].headerTextAlignment = TextAlignment.Right;
            retVal[2].canSort = true;
            retVal[2].autoResize = true;

            return retVal;
        } 
        #endregion

        public void ShowSelectNodeOrganization(string guid)
        {
            ShowSelectNodeOrganization(GetABInspectorItemDataByGUID(guid));
        }

        public void ShowSelectNodeOrganization(ABInspectorItemData node)
        {
            if(node != null)
            {
                root.children.Clear();
                CreatTreeView(root, node, 0);
            }
        }

        private void CreatTreeView(OrganizationTreeViewItem parent, ABInspectorItemData node, int depth)
        {
            string path = AssetDatabase.GUIDToAssetPath(node.GUID);
            string name = Path.GetFileName(path);

            OrganizationTreeViewItem nodeItem = new OrganizationTreeViewItem(node.GUID.GetHashCode(), depth, name);
            nodeItem.children = new List<TreeViewItem>();
            //只显示类型名，忽略其他信息
            string TypeName = AssetDatabase.GetMainAssetTypeAtPath(path).ToString();
            nodeItem.Type = TypeName.Substring(TypeName.LastIndexOf(".") + 1);

            nodeItem.Count = node.Dependency.Count;
            nodeItem.icon = AssetDatabase.GetCachedIcon(path) as Texture2D;

            parent.AddChild(nodeItem);
            foreach (var dpcGUID in node.Dependency)
            {
                ABInspectorItemData dpcNode = GetABInspectorItemDataByGUID(dpcGUID);
                CreatTreeView(nodeItem, dpcNode, depth + 1);
            }
        }
    } 

    /// <summary>
    /// 树形视图项目
    /// </summary>
    public class OrganizationTreeViewItem : TreeViewItem
    {
        public string Type { get; set; }
        public int Count { get; set; }
        public OrganizationTreeViewItem(int id, int depth, string displayName)
            : base(id, depth, displayName)
        {

        }
    }
}
