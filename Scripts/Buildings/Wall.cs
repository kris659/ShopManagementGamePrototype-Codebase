using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wall: Building
{
    [SerializeField] private Material wallMaterial;

    Vector3 startPosition;
    Vector3 endPosition;
    //inter bool isReady = false;
    private bool isBuilt = false;
    //public override bool isReadyToBuild => false;
    Mesh wallMesh;
    GameObject wallGO;

    float baseLenght = 0.625f;
    float baseWidth = 0.2f;
    float wallHeight = 3;


    private void Awake()
    {
        isReady = false;        
    }

    private void Update()
    {
        if(isBuilt && Input.GetKeyDown(KeyCode.H)) {
            MakeHole(new Vector3(0, 1.5f, 0), new Vector2(0.25f, 1));
        }
        if (isBuilt || isReady)
            return;
        if (Input.GetMouseButtonDown(0)) {
            PlaceFirstPart();
        }
    }
    public override void StartBuilding()
    {
        base.StartBuilding();

        //wallMesh = buildingModel.GetComponentInChildren<MeshFilter>().mesh;
        //Debug.Log(string.Join(',', wallMesh.vertices));
        //Debug.Log(string.Join(',', wallMesh.uv));

        Destroy(buildingModel);
        Destroy(buildingPreview);
        Destroy(buildingPreviewCollision);

        wallGO = CreateWall();
        wallGO.transform.SetParent(transform, false);
        wallGO.GetComponent<Renderer>().material = wallMaterial;

        //buildingPreview = wallGO;
        //buildingPreviewCollision = wallGO;
        buildingModel = wallGO;

        UpdateMesh();
    } 

    private void PlaceFirstPart()
    {
        isReady = true;
        startPosition = transform.position;
        endPosition = transform.position;
    }

    public override bool CanBuildHere(Vector3 position, Quaternion rotation)
    {
        return base.CanBuildHere(position, rotation);
    }

    public override void UpdatePreview(Vector3 position, Quaternion rotation)
    {       
        if (isReady) {
            transform.position = (startPosition + position) / 2;
            endPosition = position;
            UpdateMesh();
        }
        else {
            transform.position = position;
            transform.rotation = rotation;
        }

        if (CanBuildHere(position, rotation)) {
            
        }
        else {

        }       
        
    }

    public override void Build()
    {
        if(wallMesh == null) {
            Destroy(buildingModel);
            Destroy(buildingPreview);
            Destroy(buildingPreviewCollision);

            wallGO = CreateWall();
            wallGO.transform.SetParent(transform, false);
            buildingModel = wallGO;
            startPosition = transform.position;
            UpdateMesh();
        }
        base.Build();
        isBuilt = true;
    }

    private void MakeHole(Vector3 center, Vector2 size)
    {
        Vector3[] vertices = wallMesh.vertices;
        int[] triangles = wallMesh.triangles;
        List<Vector3> newVertices = new();
        List<int> newTriangles = new();

        Debug.Log(string.Join(',', wallMesh.vertices));
        //Debug.Log(string.Join(',', wallMesh.uv));

        for (int i = 0; i < vertices.Length; i += 4) {
            if (vertices[i + 0].y == vertices[i + 2].y)
                continue;
            //float minX = Mathf.Min(vertices[0].x, vertices[1].x);
            //float maxX = Mathf.Max(vertices[0].x, vertices[1].x);
            if (center.x > vertices[i + 0].x && center.x < vertices[i + 3].x) {
                
                newVertices.Add(vertices[i + 0]);
                newVertices.Add(vertices[i + 1]);
                newVertices.Add(vertices[i + 1] - Vector3.right * (center.x - size.x / 2));
                newVertices.Add(vertices[i + 0] - Vector3.right * (center.x - size.x / 2));

                int len = newVertices.Count - 4;

                newTriangles.AddRange(new int[] { len, len + 1, len + 2 });
                newTriangles.AddRange(new int[] { len , len + 2, len + 3 });

                newVertices.Add(vertices[i + 3] + Vector3.right * (center.x - size.x / 2));
                newVertices.Add(vertices[i + 2] + Vector3.right * (center.x - size.x / 2));
                newVertices.Add(vertices[i + 2]);
                newVertices.Add(vertices[i + 3]);


                len = newVertices.Count - 4;
                newTriangles.AddRange(new int[] { len, len + 1, len + 2 });
                newTriangles.AddRange(new int[] { len, len + 2, len + 3 });

                vertices[i + 0] = Vector3.zero;
                vertices[i + 1] = Vector3.zero;
                vertices[i + 2] = Vector3.zero;
                vertices[i + 3] = Vector3.zero;
            }
        }
        List<Vector3> finalVertices = new();
        List<int> finalTriangles = new();
        int skipCount = 0;
        int triangleIndex = 0;
        for(int i = 0; i < vertices.Length; i += 4) {
            if (vertices[i] == Vector3.zero) {
                skipCount += 4;
                triangleIndex += 6;
                Debug.Log(skipCount);
                continue;
            }
            Debug.Log(i + " " + triangleIndex);
            finalVertices.Add(vertices[i]);
            finalVertices.Add(vertices[i + 1]);
            finalVertices.Add(vertices[i + 2]);
            finalVertices.Add(vertices[i + 3]);

            finalTriangles.Add(triangles[triangleIndex++] - skipCount);
            finalTriangles.Add(triangles[triangleIndex++] - skipCount);
            finalTriangles.Add(triangles[triangleIndex++] - skipCount);

            finalTriangles.Add(triangles[triangleIndex++] - skipCount);
            finalTriangles.Add(triangles[triangleIndex++] - skipCount);
            finalTriangles.Add(triangles[triangleIndex++] - skipCount);
        }
        Debug.Log(finalVertices.Count);
        Debug.Log(finalTriangles.Count);
        Debug.Log(string.Join(',', wallMesh.vertices));
        Debug.Log(string.Join(',', wallMesh.triangles));
        Debug.Log(string.Join(',', finalVertices));
        Debug.Log(string.Join(',', finalTriangles));
        int trianglesOffset = finalVertices.Count;
        finalVertices.AddRange(newVertices);
        finalTriangles.AddRange(newTriangles.Select(x => x + trianglesOffset));
        wallMesh.triangles = new int[0];
        wallMesh.vertices = finalVertices.ToArray();
        wallMesh.triangles = finalTriangles.ToArray();
        wallMesh.RecalculateBounds();
    }

    private void UpdateMesh()
    {
        float xCoord = (baseLenght + Mathf.Abs(startPosition.x - endPosition.x)) / 2;
        float zCoord = baseWidth / 2;

        Vector3[] vertices = wallMesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            if (vertices[i].x < 0) {
                vertices[i].x = -xCoord;
            }
            else {
                vertices[i].x = xCoord;
            }            
        }
        wallMesh.vertices = vertices;
        wallMesh.RecalculateBounds();
    }

    private GameObject CreateWall()
    {
        GameObject wallGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallMesh = wallGO.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = wallMesh.vertices;
        int[] triangles = wallMesh.triangles;
        List<Vector3> newVertices = new();
        List<int> newTriangles = new();

        Vector3 xCoord = new Vector3(baseLenght / 2, 0, 0);
        Vector3 zCoord = new Vector3(0, 0, baseWidth / 2);
        Vector3 height = new Vector3(0, wallHeight, 0);

        newVertices.Add(-xCoord - zCoord);
        newVertices.Add(-xCoord - zCoord + height);
        newVertices.Add(xCoord - zCoord + height);
        newVertices.Add(xCoord - zCoord);

        newVertices.Add(-xCoord + zCoord);
        newVertices.Add(-xCoord + zCoord + height);
        newVertices.Add(-xCoord - zCoord + height);
        newVertices.Add(-xCoord - zCoord);

        newVertices.Add(xCoord + zCoord);
        newVertices.Add(xCoord + zCoord + height);
        newVertices.Add(-xCoord + zCoord + height);
        newVertices.Add(-xCoord + zCoord);

        newVertices.Add(xCoord - zCoord);
        newVertices.Add(xCoord - zCoord + height);
        newVertices.Add(xCoord + zCoord + height);
        newVertices.Add(xCoord + zCoord);

        for (int i = 0; i < newVertices.Count; i += 4) {
            newTriangles.AddRange(new int[] { i, i + 1, i + 2 });
            newTriangles.AddRange(new int[] { i, i + 2, i + 3 });
        }
        Debug.Log(string.Join(',', newTriangles));
        wallMesh.triangles = new int[0];
        wallMesh.vertices = newVertices.ToArray();
        wallMesh.triangles = newTriangles.ToArray();
        wallMesh.RecalculateBounds();
        wallMesh.RecalculateNormals();
        return wallGO;
    }
}
