using UnityEngine;

public class CTriangle 
{

	public int m_iVertexIndexA;
    public int m_iVertexIndexB;
    public int m_iVertexIndexC;
    int[] m_ArrVertices;

    public CTriangle(int a_a, int a_b, int a_c)
    {
        m_iVertexIndexA = a_a;
        m_iVertexIndexB = a_b;
        m_iVertexIndexC = a_c;

        m_ArrVertices = new int[3];
        m_ArrVertices[0] = a_a;
        m_ArrVertices[1] = a_b;
        m_ArrVertices[2] = a_c;
    }

    public int this[int i]
    {
        get
        {
            return m_ArrVertices[i];
        }
    }


    public bool Contains(int a_vertexIndex)
    {
        return a_vertexIndex == m_iVertexIndexA || a_vertexIndex == m_iVertexIndexB || a_vertexIndex == m_iVertexIndexC;
    }
}
