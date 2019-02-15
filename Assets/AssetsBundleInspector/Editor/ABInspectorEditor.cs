using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ABInspector
{
    public class ABInspectorEditor
    {
        public bool Ready { get { return dataManager.Ready; } }

        private ViewNode selectNode = null;
        private List<ViewNode> nodes;
        private List<Link> links;
        private Queue<ABInspectorItemData> handleQueque = null;
        private GUIStyle NodeStyle = null;
        private GUIStyle NodeHoverStyle = null;
        private ABInspectorDataManager dataManager = null;
        private Vector2 NodeOffest = new Vector2(80F, 10F);
        private Vector2 NodeSize = new Vector2(150F, 80F);
        private Vector2 NodeIconSize = new Vector2(50F, 50F);
        //测试数据
        private ABInspectorItemData selectItem = null;
        private List<ABInspectorItemData> m_testData = null; 

        public ABInspectorEditor()
        {
            nodes = new List<ViewNode>();
            links = new List<Link>();
            dataManager = new ABInspectorDataManager();
            handleQueque = new Queue<ABInspectorItemData>();
        }

        public void Init()
        {
            dataManager.Init();
        }
        public void OnDestory()
        {
            dataManager.Dispose();
        }
        
        public void SelectNode(string GUID)
        {
            ABInspectorItemData node = dataManager.GetItemDataByGUID(GUID);
            if(node != null)
            {
                SelectNode(dataManager.GetItemDataByGUID(GUID));
            }
        }
        public void SelectNode(ABInspectorItemData selectData)
        {
            if (selectData == null) return;

            if(nodes != null)
            {
                nodes.Clear();
            }
            if(links != null)
            {
                links.Clear();
            }
            //向下处理选中的节点
            HandleChildNode(selectData);
            //处理父节点
            HandleParentNode(selectData);


            float minX = float.MaxValue;
            float minY = float.MaxValue;
            //float maxX = float.MinValue;
            //float maxY = float.MinValue;
            foreach (var item in nodes)
            {
                if (item.Rect.xMin < minX) minX = item.Rect.xMin;
                if (item.Rect.yMin < minY) minY = item.Rect.yMin;

                //if (item.Rect.xMax > maxX) maxX = item.Rect.xMax;
                //if (item.Rect.yMax < maxY) maxY = item.Rect.yMax;
            }

            Vector2 offset = Vector2.zero;
            offset.x = minX < 0 ? Mathf.Abs(minX) : 0;
            offset.y = minY < 0 ? Mathf.Abs(minY) : 0;

            foreach (var item in nodes)
            {
                item.Rect = new Rect(item.Rect.position + offset, item.Rect.size);
            }
        }

        private void HandleChildNode(ABInspectorItemData node) {
            if (node.Dependency != null)
            {
                //记录当前深度
                int depth = 0;
                //记录当前的广度
                int breadth = 0;
                //记录下一层的节点总数
                int childCount = 0;
                Queue<ViewNode> parentQueue = new Queue<ViewNode>();
                handleQueque.Enqueue(node);
                ABInspectorItemData current = null;
                ViewNode viewNode = null;
                childCount = handleQueque.Count;
                while (handleQueque.Count != 0)
                {
                    current = handleQueque.Dequeue();
                    //用depth，breadth和当前层节点数量绘制当前节点
                    viewNode = AddViewNode(current, depth, breadth, childCount);
                    if(parentQueue.Count > 0)
                    {
                        var link = new Link(parentQueue.Dequeue(), viewNode);
                        links.Add(link);
                    }
                    breadth++;

                    foreach (var guid in current.Dependency)
                    {
                        handleQueque.Enqueue(GetItemByGUID(guid));
                        parentQueue.Enqueue(viewNode);
                    }

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

        private void HandleParentNode(ABInspectorItemData node)
        {
            if (node.ReverseDependency != null)
            {
                //记录当前深度
                int depth = -1;
                //记录当前的广度
                int breadth = 0;

                Queue<ViewNode> parentQueue = new Queue<ViewNode>();

                //记录下一层的节点总数
                int childCount = node.ReverseDependency.Count;
                ABInspectorItemData current = null;
                ViewNode viewNode = null;

                foreach (var guid in node.ReverseDependency)
                {
                    handleQueque.Enqueue(GetItemByGUID(guid));
                    parentQueue.Enqueue(nodes[0]);
                }

                while (handleQueque.Count != 0)
                {
                    current = handleQueque.Dequeue();
                    viewNode = AddViewNode(current, depth, breadth, childCount);
                    if (parentQueue.Count > 0)
                    {
                        var link = new Link(viewNode, parentQueue.Dequeue());
                        links.Add(link);
                    }
                    breadth++;
                    foreach (var guid in current.ReverseDependency)
                    {
                        handleQueque.Enqueue(GetItemByGUID(guid));
                        parentQueue.Enqueue(viewNode);
                    }

                    if(breadth == childCount)
                    {
                        //一层绘制完毕,此时队列里都是下一层节点
                        depth--;
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
        public ABInspectorItemData GetItemByGUID(string guid)
        {
            //return m_testData.Find(x => x.GUID == guid);
            return dataManager.GetItemDataByGUID(guid);
        }

        ViewNode GetViewNodeByGUID(string guid)
        {
            return nodes.Find(x => x.GUID == guid);
        }

        ViewNode AddViewNode(ABInspectorItemData data, int depth, int breadth, int totalBreadth)
        {
            string path = AssetDatabase.GUIDToAssetPath(data.GUID);
            string name = Path.GetFileNameWithoutExtension(path);
            Debug.LogFormat("depth:{0}, breadth:{1} node:{2}", depth, breadth, data.GUID);

            ViewNode viewNode = new ViewNode(name, data.GUID);
            viewNode.Icon = AssetDatabase.GetCachedIcon(path) as Texture2D;

            int halfBreadth = totalBreadth / 2;

            Vector2 position;
            if (totalBreadth % 2 == 0)
            {
                int index = (breadth - halfBreadth);
                index = index >= 0 ? index + 1 : index;
                position = new Vector2(depth * (NodeSize.x + NodeOffest.x), index * (NodeSize.y + NodeOffest.y));
            }
            else
            {
                position = new Vector2(depth * (NodeSize.x + NodeOffest.x), (breadth - halfBreadth) * (NodeSize.y + NodeOffest.y));
            }

            viewNode.Rect = new Rect(position, NodeSize);

            nodes.Add(viewNode);
            return viewNode;
        }

        public void DrawNode()
        {
            if(NodeStyle == null)
            {
                NodeStyle = new GUIStyle(GUI.skin.GetStyle("flow node 0"))
                {
                    alignment = TextAnchor.UpperCenter,
                    margin = new RectOffset(0, 0, 0, 0)
                }; 
            }
            if(NodeHoverStyle == null)
            {
                NodeHoverStyle = new GUIStyle(GUI.skin.GetStyle("flow node 0 on"))
                {
                    alignment = TextAnchor.UpperCenter,
                    margin = new RectOffset(0, 0, 0, 0)
                };
            }

            //高亮根节点
            int index = 0;
            foreach (var item in nodes)
            {
                GUI.Window(index, item.Rect, NodeWindowFun, item.Name, index == 0 ? NodeHoverStyle : NodeStyle);
                index++;
            }
        }

        public void DrawLink()
        {
            Color color = Color.red;
            foreach (var link in links)
            {
                Vector3 startPos = new Vector3(link.StartNode.Rect.x +link.StartNode.Rect.width, link.StartNode.Rect.y + link.StartNode.Rect.height / 2f, 0);
                Vector3 endPos = new Vector3(link.EndNode.Rect.x, link.EndNode.Rect.y + link.EndNode.Rect.height / 2f, 0);
                Vector3 startTan = startPos + Vector3.right * 50;
                Vector3 endTan = endPos + Vector3.left * 50;
                var distance = Vector3.Distance(startPos, endPos);

                if (distance < 100)
                {
                    startTan = startPos + Vector3.right * (distance * 0.5f);
                    endTan = endPos + Vector3.left * (distance * 0.5f);
                }

                Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 5);
            }
        }

        private void NodeWindowFun(int windowID)
        {
            var node = nodes[windowID];
            if(node.Icon != null)
            {
                GUI.DrawTexture(new Rect((node.Rect.width - NodeIconSize.x)/2, node.Rect.height - NodeIconSize.y, NodeIconSize.x, NodeIconSize.y), node.Icon);
            }
            
        }
    }
}



