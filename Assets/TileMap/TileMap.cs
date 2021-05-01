using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour {

    public int sizeX = 100;
    public int sizeZ = 100;
    public float tileSize = 1.0f;

    public Grid grid;

    float vertMaxHeight;
    float vertMinHeight;

    public Texture2D terrainTiles;
    public Texture2D empTex;
    public int tileResolution;

    public Texture2D tex; // grid texture
    public Color[] pix; //current grid tile colors
    public Color[] basePix; //base tile colors to revert instantly if need be

    public float power = 10.0f;
    public float scale = 1.0f;
    public int randomSeedS = 3;

    public bool isQuad = true;
    public bool perlinGen = true;

    Gradient potentialGrad;

	void Start () {
        randomSeedS = System.DateTime.Now.Minute;
        grid = FindObjectOfType<Grid>();
        power = grid.power;
        scale = grid.scale;
        sizeX = grid.gridSizeX;
        sizeZ = grid.gridSizeY;

        Camera.allCameras[0].transform.position = (transform.position - new Vector3(grid.nodeRadius, 0, grid.nodeRadius)) 
            + new Vector3(grid.gridSizeX / 2.0f, 0, grid.gridSizeY / 2.0f)
            + new Vector3(-20f, 50f, 0);
        Camera.allCameras[0].orthographicSize = grid.gridSizeX / 2 + 10;

        if (isQuad) {
            Build1QuadMesh();
            CreatePotentialGradient();
        }
	}

    void CreatePotentialGradient()
    {
        potentialGrad = new Gradient();
        GradientColorKey[] potentialGradColorKey = new GradientColorKey[8];
        GradientAlphaKey[] potentialGradAlphaKey = new GradientAlphaKey[2];

        potentialGradColorKey[0].color = Color.red;
        potentialGradColorKey[0].time = 0.0f;
        potentialGradColorKey[1].color = Color.yellow;
        potentialGradColorKey[1].time = 0.14286f;
        potentialGradColorKey[2].color = Color.green;
        potentialGradColorKey[2].time = 0.28572f;
        potentialGradColorKey[3].color = Color.green;
        potentialGradColorKey[3].time = 0.42858f;
        potentialGradColorKey[4].color = Color.cyan;
        potentialGradColorKey[4].time = 0.57144f;
        potentialGradColorKey[5].color = Color.blue;
        potentialGradColorKey[5].time = 0.7143f;
        potentialGradColorKey[6].color = Color.Lerp(Color.magenta, Color.gray, 0.75f);
        potentialGradColorKey[6].time = 0.85716f;
        potentialGradColorKey[7].color = Color.red;
        potentialGradColorKey[7].time = 1.0f;

        potentialGradAlphaKey[0].alpha = 0.1f;
        potentialGradAlphaKey[0].time = 0.0f;
        potentialGradAlphaKey[1].alpha = 0.1f;
        potentialGradAlphaKey[1].time = 1.0f;

        potentialGrad.SetKeys(potentialGradColorKey, potentialGradAlphaKey);
    }

    public float GetVertMinHeight() {
        return vertMinHeight;
    }

    Color[][] ChopUpTiles() {
        int numTilesPerRow = terrainTiles.width / tileResolution;
        int numRows = terrainTiles.height / tileResolution;

        Color[][] tiles = new Color[numTilesPerRow * numRows][];

        for (int y = 0; y < numRows; y++) {
            for (int x = 0; x < numTilesPerRow; x++) {
                tiles[y * numTilesPerRow + x] = terrainTiles.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
            }
        }

        return tiles;
    }

    public void BuildTexture() {
        sizeX = grid.gridSizeX;
        sizeZ = grid.gridSizeY;
        int texWidth = sizeX * tileResolution;
        int texHeight = sizeZ * tileResolution;
        Texture2D texture = new Texture2D(texWidth, texHeight);

        Color[][] tiles = ChopUpTiles();

        for (int y = 0; y < sizeZ; y++) {
            for (int x = 0; x < sizeX; x++) {
                Color[] p = tiles[Random.Range(0, 4)];
                texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, p);
            }
        }
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.sharedMaterials[0].mainTexture = texture;

        Debug.Log("Done Texture");
    }

    public void ColorTiles(int x1, int y1, int x2, int y2) {
        if (x1 < 0 || x1 > grid.gridSizeX || y1 < 0 || y1 > grid.gridSizeY)
            return;

        int firstTile = (x1 + y1 * grid.gridSizeY); //get first vert of tile
        int numTilesX = x2 - x1;
        int numTilesY = y2 - y1;

        for (int y = 0; y < numTilesY; y++) {
            for (int x = 0; x < numTilesX; x++) {
                if (grid[firstTile + x + y * grid.gridSizeY].isWalkable)
                    pix[firstTile + x + y * grid.gridSizeY] = new Color(0.0f, 1.0f, 0.0f, 1);
            }
        }
        tex.SetPixels(pix);
        tex.Apply();
    }

    public void Build1QuadMesh() {
        int quadSize = grid.gridSizeX * grid.gridSizeY;
        int numTris = 2;

        int vertSizeX = 2;
        int vertSizeZ = 2;
        int numVerts = vertSizeX * vertSizeZ;

        // Move transform to correct world position
        transform.position = grid.worldBottomLeft;

        // Generate the mesh data
        Vector3[] vertices = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uv = new Vector2[numVerts];

        int[] triangles = new int[numTris * 3];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(grid.gridSizeX, 0, 0);
        vertices[2] = new Vector3(0, 0, grid.gridSizeY);
        vertices[3] = new Vector3(grid.gridSizeX, 0, grid.gridSizeY);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;

        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        // Assign our mesh to our filter/renderer/collider
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    void Update() {

    }
}
