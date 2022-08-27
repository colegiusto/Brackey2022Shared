using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColiderGenerator : MonoBehaviour
{
    public EdgeCollider2D edgeCollider;
    public CaveGenerator caveGenerator;
    public GameObject sphere;
    public BoundsInt bounds;
    public float cellSize;


    public void init()
    {
        
        edgeCollider = GetComponent<EdgeCollider2D>();
        caveGenerator = GetComponent<CaveGenerator>();
        if (edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        }
        generateColiderPoints();

    }

    void generateColiderPoints()
    {
        bool[,] points = getSamplePoints();
        for (int i = 0; i < points.GetLength(0)-1; i++)
        {
            for (int j = 0; j < points.GetLength(1)-1; j++)
            {
                Vector2 pos = new Vector2(i * cellSize + bounds.min.x, j * cellSize + bounds.min.y);

                bool[] values = new bool[] { points[i, j], points[i + 1, j], points[i + 1, j + 1], points[i, j + 1] };
                Vector2[] positions = new Vector2[] { pos, pos + new Vector2(cellSize, 0), pos + new Vector2(cellSize, cellSize), pos + new Vector2(0, cellSize) };
                drawBox(values, positions);

                
                
            }
        }

    }




    void drawBox(bool[] corners, Vector2[] pos){
        int value = (corners[0] ? 1 : 0) | (corners[1] ? 1 << 1 : 0) | (corners[2] ? 1 << 2 : 0) | (corners[3] ? 1 << 3 : 0);
        if (value == 0 || value == 15)
        {
            return;
        }
        int numpoints = 0;
        Vector2[] points = new Vector2[2];
        for (int i = 0; i < 4; i++)
        {
            int firstIndex = i;
            int secondIndex = (i + 1) % 4;
            if(corners[firstIndex] != corners[secondIndex])
            {
                points[numpoints] = (pos[firstIndex] - pos[secondIndex]) / 2 + pos[secondIndex];
                points[numpoints] = binarySearch(pos[firstIndex], pos[secondIndex], points[numpoints], 0);
                
                numpoints++;
                if(numpoints == 2)
                {
                    break;
                }
            }
        }
        if(numpoints != 2)
        {
            return;
        }
        Debug.DrawLine(points[0], points[1], Color.green, 20f);
        



    }

    Vector2 binarySearch(Vector2 a, Vector2 b, Vector2 c, int numIter)
    {
        if(numIter == 0)
        {
            return c;
        }
        if(caveGenerator.getValue(a) > 0 == caveGenerator.getValue(c) > 0)
        {
            c = (c - b) / 2 + b;
            return binarySearch(a, b, c, numIter - 1);
        }
        else
        {
            c = (c - a) / 2 + a;
            return binarySearch(a, b, c, numIter - 1);
        }


    }


    bool[,] getSamplePoints()
    {
        bool[,] points = new bool[(int)(bounds.size.x / cellSize), (int)(bounds.size.y / cellSize)];

        for (int i = 0; i < points.GetLength(0); i++)
        {
            for (int j = 0; j < points.GetLength(1); j++)
            {
                Vector2 pos = new Vector2(j * cellSize + bounds.min.x, i * cellSize + bounds.min.y);
                //print(pos);
                points[j, i] = caveGenerator.getValue(pos) > 0f ? true : false;
                

            }
        }
        return points;


    }
        

    // Update is called once per frame
    void Update()
    {
        
    }
}
