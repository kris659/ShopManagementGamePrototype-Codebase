using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FontData;

public class FontMeshGenerator
{
    enum NodeType
    {
        Vertex,
        Edge,
        Trapezoid
    }
    class Node
    {
        public NodeType nodeType;
        public int index;
        public Node left, right, parent;
    }

    class TrapezoidData
    {
        public int highVertex, lowVertex;
        public TrapezoidData trapezoidAbove, trapezoidAbove_2;
        public TrapezoidData trapezoidBelow, trapezoidBelow_2;
        public TrapezoidData trapezoidExtra;
        public int trapezoidExtra_side;
        // left right edge
        // sink?
        public bool isInside;
    }


    public static Mesh GenerateMesh(GlyphData glyphData)
    {
        // To chyba jest du¿o trudniejsze 

        //List<Vector2Int> endgesList = GetContourEdges(glyphData);
        //for (int i = 0; i < endgesList.Count; i++) {
        //    Vector3 startPosition = new Vector3(glyphData.Points[endgesList[i].x].X, glyphData.Points[endgesList[i].x].Y);
        //    Vector3 endPosition = new Vector3(glyphData.Points[endgesList[i].y].X, glyphData.Points[endgesList[i].y].Y);
        //    Debug.DrawLine(startPosition / 100, endPosition / 100, Color.red, 5f);
        //}
        //endgesList.Shuffle();
        //Debug.Log(string.Join(", ", endgesList));

        //List<TrapezoidData> trapezoidsList = new List<TrapezoidData>();
        
        //int higherVertex = glyphData.Points[endgesList[0].x].Y > glyphData.Points[endgesList[0].y].Y ? endgesList[0].x : endgesList[0].y;
        //int lowerVertex = glyphData.Points[endgesList[0].x].Y > glyphData.Points[endgesList[0].y].Y ? endgesList[0].y : endgesList[0].x;

        //Node root = new Node();
        //root.nodeType = NodeType.Vertex;
        //root.index = higherVertex;

        //TrapezoidData trapezoidUp = new TrapezoidData();
        //TrapezoidData trapezoidDown = new TrapezoidData();

        //trapezoidUp.lowVertex = higherVertex;
        //trapezoidDown.highVertex = higherVertex;

        //for (int i = 1; i < endgesList.Count; i++) {
            
        //}




        return null;
    }

    private static List<Vector2Int> GetContourEdges(GlyphData glyphData)
    {
        List<Vector2Int> endgesList = new List<Vector2Int>();

        for (int contourIndex = 0; contourIndex < glyphData.ContourEndIndices.Length; contourIndex++) {
            int pointIndex = 0;
            if (contourIndex > 0)
                pointIndex = glyphData.ContourEndIndices[contourIndex - 1] + 1;
            for (int i = pointIndex + 1; i <= glyphData.ContourEndIndices[contourIndex]; i++) {
                endgesList.Add(new Vector2Int(i - 1, i));
            }
            endgesList.Add(new Vector2Int(glyphData.ContourEndIndices[contourIndex], pointIndex));
        }
        return endgesList;
    }
}

