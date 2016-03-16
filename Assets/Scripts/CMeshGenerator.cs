using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CMeshGenerator : MonoBehaviour {

    public CSquareGrid m_SquareGrid;
    public MeshFilter m_Walls;
    public MeshFilter m_Cave;

    public bool m_bIS2D;

    List<Vector3> m_VecVertices;
    List<int> m_lstOfTriangles;

    Dictionary<int, List<CTriangle>> m_DicTriangleDictionary = new Dictionary<int, List<CTriangle>>();
    List<List<int>> m_lstOfOutlines = new List<List<int>>();
    HashSet<int> m_lstOfCheckedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] a_map, float a_squareSize)
    {

        m_DicTriangleDictionary.Clear();
        m_lstOfOutlines.Clear();
        m_lstOfCheckedVertices.Clear();

        m_SquareGrid = new CSquareGrid(a_map, a_squareSize);

        m_VecVertices = new List<Vector3>();
        m_lstOfTriangles = new List<int>();

        for (int x = 0; x < m_SquareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < m_SquareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(m_SquareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        m_Cave.mesh = mesh;

        mesh.vertices = m_VecVertices.ToArray();
        mesh.triangles = m_lstOfTriangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[m_VecVertices.Count];
        for (int i = 0; i < m_VecVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-a_map.GetLength(0) / 2 * a_squareSize, a_map.GetLength(0) / 2 * a_squareSize, m_VecVertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-a_map.GetLength(0) / 2 * a_squareSize, a_map.GetLength(0) / 2 * a_squareSize, m_VecVertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;


        if (m_bIS2D)
        {
            Generate2DColliders();
        }
        else
        {
            CreateWallMesh();
        }
    }

    void CreateWallMesh()
    {

        MeshCollider currentCollider = GetComponent<MeshCollider>();
        DestroyImmediate(currentCollider);

        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in m_lstOfOutlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(m_VecVertices[outline[i]]); // left
                wallVertices.Add(m_VecVertices[outline[i + 1]]); // right
                wallVertices.Add(m_VecVertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(m_VecVertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        m_Walls.mesh = wallMesh;

        MeshCollider wallCollider = gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }

    void Generate2DColliders()
    {

        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach (List<int> outline in m_lstOfOutlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i++)
            {
                edgePoints[i] = new Vector2(m_VecVertices[outline[i]].x, m_VecVertices[outline[i]].z);
            }
            edgeCollider.points = edgePoints;
        }

    }

    void TriangulateSquare(CSquare a_Square)
    {
        switch (a_Square.m_iConfiguration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(a_Square.m_CentreLeft, a_Square.m_CentreBottom, a_Square.m_BottomLeft);
                break;
            case 2:
                MeshFromPoints(a_Square.m_BottomRight, a_Square.m_CentreBottom, a_Square.m_CentreRight);
                break;
            case 4:
                MeshFromPoints(a_Square.m_TopRight, a_Square.m_CentreRight, a_Square.m_CentreTop);
                break;
            case 8:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_CentreTop, a_Square.m_CentreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(a_Square.m_CentreRight, a_Square.m_BottomRight, a_Square.m_BottomLeft, a_Square.m_CentreLeft);
                break;
            case 6:
                MeshFromPoints(a_Square.m_CentreTop, a_Square.m_TopRight, a_Square.m_BottomRight, a_Square.m_CentreBottom);
                break;
            case 9:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_CentreTop, a_Square.m_CentreBottom, a_Square.m_BottomLeft);
                break;
            case 12:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_TopRight, a_Square.m_CentreRight, a_Square.m_CentreLeft);
                break;
            case 5:
                MeshFromPoints(a_Square.m_CentreTop, a_Square.m_TopRight, a_Square.m_CentreRight, a_Square.m_CentreBottom, a_Square.m_BottomLeft, a_Square.m_CentreLeft);
                break;
            case 10:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_CentreTop, a_Square.m_CentreRight, a_Square.m_BottomRight, a_Square.m_CentreBottom, a_Square.m_CentreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(a_Square.m_CentreTop, a_Square.m_TopRight, a_Square.m_BottomRight, a_Square.m_BottomLeft, a_Square.m_CentreLeft);
                break;
            case 11:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_CentreTop, a_Square.m_CentreRight, a_Square.m_BottomRight, a_Square.m_BottomLeft);
                break;
            case 13:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_TopRight, a_Square.m_CentreRight, a_Square.m_CentreBottom, a_Square.m_BottomLeft);
                break;
            case 14:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_TopRight, a_Square.m_BottomRight, a_Square.m_CentreBottom, a_Square.m_CentreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(a_Square.m_TopLeft, a_Square.m_TopRight, a_Square.m_BottomRight, a_Square.m_BottomLeft);
                m_lstOfCheckedVertices.Add(a_Square.m_TopLeft.vertexIndex);
                m_lstOfCheckedVertices.Add(a_Square.m_TopRight.vertexIndex);
                m_lstOfCheckedVertices.Add(a_Square.m_BottomRight.vertexIndex);
                m_lstOfCheckedVertices.Add(a_Square.m_BottomLeft.vertexIndex);
                break;
        }

    }

    void MeshFromPoints(params CNode[] a_points)
    {
        AssignVertices(a_points);

        if (a_points.Length >= 3)
            CreateTriangle(a_points[0], a_points[1], a_points[2]);
        if (a_points.Length >= 4)
            CreateTriangle(a_points[0], a_points[2], a_points[3]);
        if (a_points.Length >= 5)
            CreateTriangle(a_points[0], a_points[3], a_points[4]);
        if (a_points.Length >= 6)
            CreateTriangle(a_points[0], a_points[4], a_points[5]);

    }

    void AssignVertices(CNode[] a_points)
    {
        for (int i = 0; i < a_points.Length; i++)
        {
            if (a_points[i].vertexIndex == -1)
            {
                a_points[i].vertexIndex = m_VecVertices.Count;
                m_VecVertices.Add(a_points[i].position);
            }
        }
    }

    void CreateTriangle(CNode a_a, CNode a_b, CNode a_c)
    {
        m_lstOfTriangles.Add(a_a.vertexIndex);
        m_lstOfTriangles.Add(a_b.vertexIndex);
        m_lstOfTriangles.Add(a_c.vertexIndex);

        CTriangle triangle = new CTriangle(a_a.vertexIndex, a_b.vertexIndex, a_c.vertexIndex);
        AddTriangleToDictionary(triangle.m_iVertexIndexA, triangle);
        AddTriangleToDictionary(triangle.m_iVertexIndexB, triangle);
        AddTriangleToDictionary(triangle.m_iVertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int a_vertexIndexKey, CTriangle a_triangle)
    {
        if (m_DicTriangleDictionary.ContainsKey(a_vertexIndexKey))
        {
            m_DicTriangleDictionary[a_vertexIndexKey].Add(a_triangle);
        }
        else
        {
            List<CTriangle> triangleList = new List<CTriangle>();
            triangleList.Add(a_triangle);
            m_DicTriangleDictionary.Add(a_vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < m_VecVertices.Count; vertexIndex++)
        {
            if (!m_lstOfCheckedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    m_lstOfCheckedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    m_lstOfOutlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, m_lstOfOutlines.Count - 1);
                    m_lstOfOutlines[m_lstOfOutlines.Count - 1].Add(vertexIndex);
                }
            }
        }

        SimplifyMeshOutlines();
    }

    void SimplifyMeshOutlines()
    {
        for (int outlineIndex = 0; outlineIndex < m_lstOfOutlines.Count; outlineIndex++)
        {
            List<int> simplifiedOutline = new List<int>();
            Vector3 dirOld = Vector3.zero;
            for (int i = 0; i < m_lstOfOutlines[outlineIndex].Count; i++)
            {
                Vector3 p1 = m_VecVertices[m_lstOfOutlines[outlineIndex][i]];
                Vector3 p2 = m_VecVertices[m_lstOfOutlines[outlineIndex][(i + 1) % m_lstOfOutlines[outlineIndex].Count]];
                Vector3 dir = p1 - p2;
                if (dir != dirOld)
                {
                    dirOld = dir;
                    simplifiedOutline.Add(m_lstOfOutlines[outlineIndex][i]);
                }
            }
            m_lstOfOutlines[outlineIndex] = simplifiedOutline;
        }
    }

    void FollowOutline(int a_vertexIndex, int a_outlineIndex)
    {
        m_lstOfOutlines[a_outlineIndex].Add(a_vertexIndex);
        m_lstOfCheckedVertices.Add(a_vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(a_vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, a_outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int a_vertexIndex)
    {
        List<CTriangle> trianglesContainingVertex = m_DicTriangleDictionary[a_vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            CTriangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != a_vertexIndex && !m_lstOfCheckedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(a_vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int a_vertexA, int a_vertexB)
    {
        List<CTriangle> trianglesContainingVertexA = m_DicTriangleDictionary[a_vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(a_vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

   

   
}
