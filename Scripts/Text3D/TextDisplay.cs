using Sebastian.Geometry;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Rendering;
using UnityEngine;
using static FontData;

public class TextDisplay : MonoBehaviour
{
    private FontData fontData;
    [SerializeField] private GameObject letterPrefab;
    private float distanceMult = 0.01f;
    private float textSize = 0.04f;
    private GameObject parentGO;
    private BoxCollider boxCollider;
    public Vector3 TextColliderSize => boxCollider.size;
    public Vector3 TextColliderCenter => boxCollider.center;

    private void Awake()
    {
        boxCollider = transform.GetComponent<BoxCollider>();
    }

    public void GenerateText(string text)
    {
        //fontData = FontParser.Parse(Path.Combine(Application.streamingAssetsPath, "JetBrainsMono-Bold.ttf")); 
        //fontData = FontParser.Parse(Path.Combine(Application.streamingAssetsPath, "Josefin_Sans", "static", "JosefinSans-Regular.ttf"));
        //fontData = FontParser.Parse(Path.Combine(Application.streamingAssetsPath, "Roboto", "Roboto-Regular.ttf"));

        if (fontData == null)
            fontData = FontParser.Parse(Path.Combine(Application.streamingAssetsPath, "Roboto", "Roboto-Regular.ttf"));

        Vector3 currentPosition = Vector3.zero;
        if(parentGO != null)
            Destroy(parentGO);
        parentGO = new GameObject("TextModel");
        parentGO.transform.SetParent(transform);
        parentGO.transform.localPosition = Vector3.zero;
        parentGO.transform.localEulerAngles = new Vector3(-90, 180, 0);
        parentGO.transform.localScale = Vector3.one * textSize;
        for (int i = 0; i < text.Length; i++) {
            GameObject letterGO = Instantiate(letterPrefab, parentGO.transform);
            MeshFilter meshFilter = letterGO.GetComponent<MeshFilter>();
            letterGO.name = text[i] + " " + Time.time;
            fontData.TryGetGlyph(text[i], out GlyphData glyphData);
            List<Shape> shapes = GetShapes(glyphData);
            CompositeShape compositeShape = new CompositeShape(shapes);
            if (text[i] != ' ') {
                Mesh mesh = compositeShape.GetMesh();
                meshFilter.mesh = mesh;
            }
            letterGO.transform.localPosition = currentPosition;
            letterGO.transform.localRotation = Quaternion.identity;
            currentPosition.x += glyphData.AdvanceWidth * distanceMult;
        }
        parentGO.transform.localPosition = currentPosition * textSize / 2 + new Vector3(0, -0.25f, 0.02f);
        if(boxCollider == null)
            boxCollider = transform.GetComponent<BoxCollider>();
        boxCollider.size = currentPosition * textSize + new Vector3(0, 0.7f, 0.05f);
        boxCollider.center = new Vector3(0, 0, 0.04f);
        return;
    }

    private static List<Shape> GetShapes(GlyphData glyphData)
    {
        List<Shape> shapes = new List<Shape>();

        for (int contourIndex = 0; contourIndex < glyphData.ContourEndIndices.Length; contourIndex++) {
            List<Vector3> points = new List<Vector3>();
            int pointIndex = 0;
            if (contourIndex > 0)
                pointIndex = glyphData.ContourEndIndices[contourIndex - 1] + 1;
            for (int i = pointIndex; i <= glyphData.ContourEndIndices[contourIndex]; i++) {
                points.Add(new Vector3(glyphData.Points[i].X, 0, glyphData.Points[i].Y) / 100);
            }
            //points.Add(new Vector3(glyphData.Points[pointIndex].X, glyphData.Points[pointIndex].Y, glyphData.Points[pointIndex].Y));
            Shape shape = new Shape();
            shape.points = points;
            shapes.Add(shape);
        }
        return shapes;
    }
}
