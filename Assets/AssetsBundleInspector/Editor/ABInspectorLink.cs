namespace ABInspector
{
    public class Link
    {
        private ViewNode m_startNode;
        public ViewNode StartNode { get { return m_startNode; } }

        private ViewNode m_endNode;
        public ViewNode EndNode { get { return m_endNode; } }

        public Link(ViewNode start, ViewNode end)
        {
            m_startNode = start;
            m_endNode = end;
        }
    }

}
