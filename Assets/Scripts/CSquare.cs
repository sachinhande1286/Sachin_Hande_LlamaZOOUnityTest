public class CSquare
{

    public CControlCNode m_TopLeft;
    public CControlCNode m_TopRight;
    public CControlCNode m_BottomRight;
    public CControlCNode m_BottomLeft;

    public CNode m_CentreTop;
    public CNode m_CentreRight;
    public CNode m_CentreBottom; 
    public CNode m_CentreLeft;

    public int m_iConfiguration;

    public CSquare(CControlCNode mTopLeft, CControlCNode mTopRight, CControlCNode mBottomRight, CControlCNode mBottomLeft)
    {
        m_TopLeft = mTopLeft;
        m_TopRight = mTopRight;
        m_BottomRight = mBottomRight;
        m_BottomLeft = mBottomLeft;

        m_CentreTop = m_TopLeft.right;
        m_CentreRight = m_BottomRight.above;
        m_CentreBottom = m_BottomLeft.right;
        m_CentreLeft = m_BottomLeft.above;

        if (m_TopLeft.active)
            m_iConfiguration += 8;
        if (m_TopRight.active)
            m_iConfiguration += 4;
        if (m_BottomRight.active)
            m_iConfiguration += 2;
        if (m_BottomLeft.active)
            m_iConfiguration += 1;
    }

}
