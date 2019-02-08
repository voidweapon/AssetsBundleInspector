using System.Collections.Generic;
using UnityEngine;
namespace ABInspector
{
    /// <summary>
    /// view
    /// </summary>
    public class ViewNode
    {
        /// <summary>
        /// Gets or sets the position of the View Node.
        /// </summary>
        /// <value>The position.</value>
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Texture2D Icon { get; set; }

        private string m_name = string.Empty;
        public string Name { get { return m_name; } }

        private List<Link> m_inLinks = new List<Link>();
        public List<Link> InLinks { get { return m_inLinks; } }

        private List<Link> m_outLinks = new List<Link>();
        public List<Link> OutLinks { get { return m_outLinks; } }
    }

}
