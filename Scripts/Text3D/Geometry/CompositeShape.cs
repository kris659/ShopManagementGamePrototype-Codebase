using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/*
 * Processes array of shapes into a single mesh
 * Automatically determines which shapes are solid, and which are holes
 * Ignores invalid shapes (contain self-intersections, too few points, overlapping holes)
 */

namespace Sebastian.Geometry
{
    public partial class CompositeShape
    {
        public Vector3[] vertices;
        public Vector3[] normals;
        public int[] triangles;

        Shape[] shapes;
        float height = 0;

        public CompositeShape(IEnumerable<Shape> shapes)
        {
            this.shapes = shapes.ToArray();
        }

        public Mesh GetMesh()
        {
            Process();
            ProcessTo3D();
            Mesh mesh = new Mesh();


            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;

            return mesh;
        }

        public void Process()
        {
            // Generate array of valid shape data
            CompositeShapeData[] eligibleShapes = shapes.Select(x => new CompositeShapeData(x.points.ToArray())).Where(x => x.IsValidShape).ToArray();

            // Set parents for all shapes. A parent is a shape which completely contains another shape.
            for (int i = 0; i < eligibleShapes.Length; i++)
            {
                for (int j = 0; j < eligibleShapes.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (eligibleShapes[i].IsParentOf(eligibleShapes[j]))
                    {
                        eligibleShapes[j].parents.Add(eligibleShapes[i]);
                    }
                }
            }

            // Holes are shapes with an odd number of parents.
            CompositeShapeData[] holeShapes = eligibleShapes.Where(x => x.parents.Count % 2 != 0).ToArray();
            foreach (CompositeShapeData holeShape in holeShapes)
            {
                // The most immediate parent (i.e the smallest parent shape) will be the one that has the highest number of parents of its own. 
                CompositeShapeData immediateParent = holeShape.parents.OrderByDescending(x => x.parents.Count).First();
                immediateParent.holes.Add(holeShape);
            }

            // Solid shapes have an even number of parents
            CompositeShapeData[] solidShapes = eligibleShapes.Where(x => x.parents.Count % 2 == 0).ToArray();
            foreach (CompositeShapeData solidShape in solidShapes)
            {
                solidShape.ValidateHoles();

            }
            // Create polygons from the solid shapes and their associated hole shapes
            Polygon[] polygons = solidShapes.Select(x => new Polygon(x.polygon.points, x.holes.Select(h => h.polygon.points).ToArray())).ToArray();
  
            // Flatten the points arrays from all polygons into a single array, and convert the vector2s to vector3s.
            vertices = polygons.SelectMany(x => x.points.Select(v2 => new Vector3(v2.x, height, v2.y))).ToArray();

            // Triangulate each polygon and flatten the triangle arrays into a single array.
            List<int> allTriangles = new List<int>();
            int startVertexIndex = 0;
            for (int i = 0; i < polygons.Length; i++)
            {
                Triangulator triangulator = new Triangulator(polygons[i]);
                int[] polygonTriangles = triangulator.Triangulate();

                for (int j = 0; j < polygonTriangles.Length; j++)
                {
                    allTriangles.Add(polygonTriangles[j] + startVertexIndex);
                }
                startVertexIndex += polygons[i].numPoints;
            }

            triangles = allTriangles.ToArray();
        }

        public void ProcessTo3D()
        {
            int startingVerticesCount = vertices.Length;
            int startingTrianglesCount = triangles.Length;

            Vector3[] newVertices = new Vector3[vertices.Length * 2];
            Vector3[] newNormals = new Vector3[vertices.Length * 2];
            for (int i = 0; i < vertices.Length; i++) {
                newVertices[i] = vertices[i];
                newVertices[i + vertices.Length] = vertices[i] - new Vector3(0, 1, 0);

                newNormals[i] = Vector3.up;
                newNormals[i + vertices.Length] = Vector3.down;
            }
            
            int[] newTriangles = new int[triangles.Length * 2];
            int trianglesCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++) {
                newTriangles[i] = triangles[i];
                newTriangles[i + triangles.Length] = triangles[i] + vertices.Length;
            }
            for (int i = triangles.Length; i < newTriangles.Length; i += 3) {
                int temp = newTriangles[i];
                newTriangles[i] = newTriangles[i + 2];
                newTriangles[i + 2] = temp;
            }

            List<Vector3> sideVertices = new List<Vector3>();
            List<Vector3> sideNormals = new List<Vector3>();
            List<int> sideTriangles = new List<int>();
            int index = 0;
            int count = 0;
            for (int i = 0; i < shapes.Length; i++) {
                count += shapes[i].points.Count;

                sideVertices.Add(newVertices[index]);
                sideVertices.Add(newVertices[count - 1]);
                sideVertices.Add(newVertices[index + startingVerticesCount]);
                sideVertices.Add(newVertices[count - 1 + startingVerticesCount]);

                Vector3 normal = -GetNormal(newVertices[index], newVertices[count - 1], newVertices[index + startingVerticesCount]);
                sideNormals.Add(normal);
                sideNormals.Add(normal);
                sideNormals.Add(normal);
                sideNormals.Add(normal);

                sideTriangles.Add(sideVertices.Count - 2);
                sideTriangles.Add(sideVertices.Count - 3);
                sideTriangles.Add(sideVertices.Count - 4);

                sideTriangles.Add(sideVertices.Count - 3);
                sideTriangles.Add(sideVertices.Count - 2);
                sideTriangles.Add(sideVertices.Count - 1);

                while (++index < count) {
                    sideVertices.Add(newVertices[index]);
                    sideVertices.Add(newVertices[index - 1]);
                    sideVertices.Add(newVertices[index + startingVerticesCount]);
                    sideVertices.Add(newVertices[index - 1 + startingVerticesCount]);

                    normal = -GetNormal(newVertices[index], newVertices[index - 1], newVertices[index + startingVerticesCount]);
                    sideNormals.Add(normal);
                    sideNormals.Add(normal);
                    sideNormals.Add(normal);
                    sideNormals.Add(normal);

                    sideTriangles.Add(sideVertices.Count - 2);
                    sideTriangles.Add(sideVertices.Count - 3);
                    sideTriangles.Add(sideVertices.Count - 4);

                    sideTriangles.Add(sideVertices.Count - 3);
                    sideTriangles.Add(sideVertices.Count - 2);
                    sideTriangles.Add(sideVertices.Count - 1);
                }
            }

            vertices = new Vector3[newVertices.Length + sideVertices.Count];
            for (int i = 0; i < newVertices.Length; i++) {
                vertices[i] = newVertices[i];
            }
            for (int i = 0; i < sideVertices.Count; i++) {
                vertices[i + newVertices.Length] = sideVertices[i];
            }

            normals = new Vector3[newNormals.Length + sideNormals.Count];
            for (int i = 0; i < newNormals.Length; i++) {
                normals[i] = newNormals[i];
            }
            for (int i = 0; i < sideNormals.Count; i++) {
                normals[i + newNormals.Length] = sideNormals[i];
            }

            triangles = new int[newTriangles.Length + sideTriangles.Count];
            for(int i = 0; i < newTriangles.Length; i++) {
                triangles[i] = newTriangles[i];
            }
            for (int i = 0; i < sideTriangles.Count; i++) {
                triangles[i + newTriangles.Length] = sideTriangles[i] + startingVerticesCount * 2;
            }


            //Debug.Log("Vertices: " + String.Join(", ", vertices));
            //Debug.Log("Triangles: " + String.Join(", ", triangles));
        }

        Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 o)
        {
            // Find vectors corresponding to two of the sides of the triangle.
            Vector3 side1 = a - o;
            Vector3 side2 = b - o;

            // Cross the vectors to get a perpendicular vector, then normalize it. This is the Result vector in the drawing above.
            return Vector3.Cross(side1, side2).normalized;
        }
    }
}