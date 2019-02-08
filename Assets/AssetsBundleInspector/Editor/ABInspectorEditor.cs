using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ABInspector
{
    public class ABInspectorEditor
    {
        private ViewNode selectNode = null;
        private List<ViewNode> nodes;
        private List<Link> links;
        private Queue<ABInspectorItemData> handleQueque = null;
        public ABInspectorEditor()
        {
            nodes = new List<ViewNode>();
            //root
            selectNode = new ViewNode();

            //parent1-1 parent1-2

            //child 1-1 1-2

            //child 2-1


            int a = 1;
            Debug.Log(a);
        }
        public void OnGUI()
        {
            DrawNode();
            DrawLink();
        }
        public void SelectNode(ABInspectorItemData selectData)
        {
            if(nodes != null)
            {
                nodes.Clear();
            }
            if(links != null)
            {
                links.Clear();
            }
            //处理选中的节点

            //处理子节点

            //处理父节点

        }

        private void HandleChildNode(ABInspectorItemData node) {
            if(node.Dependency != null && node.Dependency.Count > 0)
            {
                //记录当前深度
                int depth = 0;
                //记录当前的广度
                int breadth = 0;
                //记录下一层的节点总数
                int childCount = 0;
                handleQueque.Enqueue(node);
                ABInspectorItemData current = null;
                while (handleQueque.Count != 0)
                {
                    current = handleQueque.Dequeue();
                    foreach (var guid in current.Dependency)
                    {
                        handleQueque.Enqueue(GetItemByGUID(guid));
                    }
                    //用depth，breadth和当前层节点数量绘制当前节点
                    AddViewNode(current, depth, breadth, childCount);
                    breadth ++;
                    if (breadth == childCount)
                    {
                        //一层绘制完毕,此时队列里都是下一层节点
                        depth ++;
                        breadth = 0;
                        childCount = handleQueque.Count;
                    }

                }
            }
        }

        /// <summary>
        /// 从Manager里通过GUID获取Item
        /// </summary>
        /// <returns>The item by GUID.</returns>
        /// <param name="guid">GUID.</param>
        ABInspectorItemData GetItemByGUID(string guid)
        {
            return null;
        }

        void AddViewNode(ABInspectorItemData data, int depth, int breadth, int totalBreadth)
        {

        }

        private void DrawNode()
        {

        }

        private void DrawLink()
        {

        }
    }
}



