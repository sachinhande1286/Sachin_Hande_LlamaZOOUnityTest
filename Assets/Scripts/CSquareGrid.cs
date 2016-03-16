using UnityEngine;

public class CSquareGrid
{
    public CSquare[,] squares;

    public CSquareGrid(int[,] map, float squareSize)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX * squareSize;
        float mapHeight = nodeCountY * squareSize;

        CControlCNode[,] cControlCNodes = new CControlCNode[nodeCountX, nodeCountY];

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                cControlCNodes[x, y] = new CControlCNode(pos, map[x, y] == 1, squareSize);
            }
        }

        squares = new CSquare[nodeCountX - 1, nodeCountY - 1];
        for (int x = 0; x < nodeCountX - 1; x++)
        {
            for (int y = 0; y < nodeCountY - 1; y++)
            {
                squares[x, y] = new CSquare(cControlCNodes[x, y + 1], cControlCNodes[x + 1, y + 1], cControlCNodes[x + 1, y], cControlCNodes[x, y]);
            }
        }

    }
}
