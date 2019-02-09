using System.Collections.Generic;
using UnityEngine;
namespace ABInspector
{
    /// <summary>
    /// view
    /// </summary>
    public class ViewNode
    {
        public Rect Rect { get; set; }
        public Texture2D Icon { get; set; }

        private string m_name = string.Empty;
        public string Name { get { return m_name; } }

        private List<Link> m_inLinks = new List<Link>();
        public List<Link> InLinks { get { return m_inLinks; } }

        private List<Link> m_outLinks = new List<Link>();
        public List<Link> OutLinks { get { return m_outLinks; } }

        private string m_guid;
        public string GUID { get { return m_guid; } }

        public ViewNode(string name, string guid)
        {
            m_name = name;
            m_guid = guid;
        }
    }

}
