using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public bool displayGridGizmos;
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public Vector3 worldBottomLeft;
    public float nodeRadius;

    Node[] grid;
    Vector3[,] vecLines;

    public float[,] densityGrid;

    public float power = 10.0f;
    public float scale = 0.1f;
    public int randomSeedS = 3;
    public Vector2 v2SampleStart;

    public bool randHeightsPerlin = true;

    float maxHeight;
    float minHeight;

    float nodeDiameter;
    public int gridSizeX;
    public int gridSizeY;

    public float maxVecLen = Mathf.NegativeInfinity;

    // Use this for initialization
    void Awake () {
        nodeDiameter = nodeRadius * 2;
        // Calculate grid dimensions
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        randomSeedS = (int)System.DateTime.Now.Minute;

        CreateGrid();
    }

    public int MaxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }

    public float MaxHeight {
        get {
            return maxHeight;
        }
    }
	
    void CreateGrid() {
        grid = new Node[gridSizeX * gridSizeY];

        // Get bottom left corner of grid: (0,0,0)   -   (1,0,0)     * midpointOfGrid.x    - (0,0,1)         * midpointOfGrid.y; 
        worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        worldBottomLeft = Vector3.zero;
        // Setup grids
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {

                // Set bottom left position of each Node on the grid
                Vector3 worldPoint = worldBottomLeft + 
                    Vector3.right * (x * nodeDiameter + nodeRadius) +
                    Vector3.forward * (y * nodeDiameter + nodeRadius);

                // Check walkability: if a sphere located on node hits collider of obstacle, then not walkable
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask)); //PHYSICS!!!!!!!!!! HORY CLAP
                float height = 0;
                if (x == 0 || x == gridSizeX - 1 || y == 0 || y == gridSizeY - 1) {
                    height = 0;
                    grid[x + gridSizeX * y] = new Node(false, worldPoint, x, y, height);
                }
                else {
                    height = 0;
                    grid[x + gridSizeX * y] = new Node(walkable, worldPoint, x, y, height);
                }
                if (!walkable)
                    grid[x + gridSizeX * y].unitCost = Mathf.Infinity;
            }
        }
    }


    public Vector2 getAveragedNodeGradients(int x, int y) {
        Vector2 returnVector;
        returnVector.x = ((this[x + 1, y].height - this[x, y].height) + (this[x, y].height - this[x - 1, y].height)) / 2.0f;
        returnVector.y = ((this[x, y + 1].height - this[x, y].height) + (this[x, y].height - this[x, y - 1].height)) / 2.0f;
        return returnVector;
    }
    public void getAveragedNodeGradients(int x, int y, out Vector2 returnVector) {
        returnVector.x = ((this[x + 1, y].height - this[x, y].height) + (this[x, y].height - this[x - 1, y].height)) / 2.0f;
        returnVector.y = ((this[x, y + 1].height - this[x, y].height) + (this[x, y].height - this[x, y - 1].height)) / 2.0f;
    }
    public Vector2 getAveragedNodeGradients(Node node) {
        return getAveragedNodeGradients(node.gridX, node.gridY);
    }
    public void getAveragedNodeGradients(Node node, out Vector2 returnVector) {
        getAveragedNodeGradients(node.gridX, node.gridY, out returnVector);
    }

    public float getDirectedNodeGradient(Node from, Node to) {
        return getDirectedNodeGradient(from.gridX, from.gridY, to.gridX, to.gridY);
    }
    public float getDirectedNodeGradient(int xFrom, int yFrom, int xTo, int yTo) {
        if (!(xFrom >= 0 && xFrom < gridSizeX) || !(yFrom >= 0 && yFrom < gridSizeY))
            return 0;
        if (!(xTo >= 0 && xTo < gridSizeX) || !(yTo >= 0 && yTo < gridSizeY))
            return 0;
        return this[xFrom, yFrom].height - this[xTo, yTo].height;
    }


    public float getDirectedCost(Node from, Node to, UnitGroup group) {
        return getDirectedCost(from.gridX, from.gridY, to.gridX, to.gridY, group);
    }
    public float getDirectedCost(Node from, int direction, UnitGroup group) {
        return getDirectedCost(from.gridX, from.gridY, direction, group);
    }
    public float getDirectedCost(int xFrom, int yFrom, int xTo, int yTo, UnitGroup group) {
        int dirX = xTo - xFrom;
        int dirY = yTo - yFrom;
        int direction = 0;
        if      (dirX == 1  && dirY ==  0) direction = CardinalDirections.LEFT;
        else if (dirX == 0  && dirY ==  1) direction = CardinalDirections.UP;
        else if (dirX == -1 && dirY ==  0) direction = CardinalDirections.RIGHT;
        else if (dirX == 0  && dirY == -1) direction = CardinalDirections.DOWN;
        return getDirectedCost(xFrom, yFrom, xTo, yTo, direction, group);
    }
    public float getDirectedCost(int xFrom, int yFrom, int direction, UnitGroup group) {
        int dirX = 0;
        int dirY = 0;
        if (direction == CardinalDirections.LEFT) {
            dirX = 1; dirY = 0; }
        else if (direction == CardinalDirections.UP) {
            dirX = 0; dirY = 1; }
        else if (direction == CardinalDirections.RIGHT) {
            dirX = -1; dirY = 0; }
        else if (direction == CardinalDirections.DOWN) {
            dirX = 0; dirY = -1; }
        return getDirectedCost(xFrom, yFrom, xFrom + dirX, yFrom + dirY, direction, group);
    }
    public float getDirectedCost(int xFrom, int yFrom, int xTo, int yTo, int direction, UnitGroup group) {
        return (group.speedField[xFrom, yFrom][direction] /*speed in direction*/ + 1 /*time*/ + this[xTo, yTo].discomfort /*discomfort*/) 
            / group.speedField[xFrom, yFrom][direction] /*speed in direction*/;
    }


    public float getDirectedSpeed(Node from, Node to) {
        return getDirectedSpeed(from.gridX, from.gridY, to.gridX, to.gridY);
    }
    public float getDirectedSpeed(Node from, int direction) {
        return getDirectedSpeed(from.gridX, from.gridY, direction);
    }
    public float getDirectedSpeed(int xFrom, int yFrom, int xTo, int yTo) {
        int dirX = xTo - xFrom;
        int dirY = yTo - yFrom;
        int direction = 0;
        if (dirX == 1 && dirY == 0)
            direction = CardinalDirections.LEFT;
        else if (dirX == 0 && dirY == 1)
            direction = CardinalDirections.UP;
        else if (dirX == -1 && dirY == 0)
            direction = CardinalDirections.RIGHT;
        else if (dirX == 0 && dirY == -1)
            direction = CardinalDirections.DOWN;
        return getDirectedSpeed(xFrom, yFrom, xTo, yTo, direction);
    }
    public float getDirectedSpeed(int xFrom, int yFrom, int direction) {
        int dirX = 0;
        int dirY = 0;
        if (direction == CardinalDirections.LEFT) {
            dirX = 1;
            dirY = 0;
        }
        else if (direction == CardinalDirections.UP) {
            dirX = 0;
            dirY = 1;
        }
        else if (direction == CardinalDirections.RIGHT) {
            dirX = -1;
            dirY = 0;
        }
        else if (direction == CardinalDirections.DOWN) {
            dirX = 0;
            dirY = -1;
        }
        return getDirectedSpeed(xFrom, yFrom, xFrom + dirX, yFrom + dirY, direction);
    }

    float fmin = 0.1f, fmax = 5.0f, smin = -10.0f, smax = 10.0f, densityMin = 0, densityMax = 10.0f, flowspeedmin = 0.001f;

    public float getDirectedSpeed(int xFrom, int yFrom, int xTo, int yTo, int direction) {
        float tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xTo, yTo) - smin) / (smax - smin) * (fmin - fmax);
        float tempFlow;
        if (xFrom != xTo)
            tempFlow = Mathf.Max(densityMin, Mathf.Sign(xTo - xFrom) * this[xTo, yTo].avgVelocity.x); //sign of nodeVelocity depends on direction of movement
        else
            tempFlow = Mathf.Max(densityMin, Mathf.Sign(yTo - yFrom) * this[xTo, yTo].avgVelocity.y); //sign of nodeVelocity depends on direction of movement
        return tempSpeed + (this[xTo, yTo].density - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
    }


    public float calculateDirectedSpeedAndCost(int xFrom, int yFrom, int xTo, int yTo) {
        float directedSpeed = getDirectedSpeed(xFrom, yFrom, xTo, yTo);
        return (directedSpeed /*speed in direction*/ + 1 /*time*/ + this[xTo, yTo].discomfort /*discomfort*/) / directedSpeed /*speed in direction*/;
    }

    public Vector4 calculateSpeedAndCost(int xFrom, int yFrom) {
        Vector4 returnVector = Vector4.zero;
        Vector4 directedSpeed = Vector4.zero;
        float tempSpeed, tempFlow;

        tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xFrom + 1, yFrom) - smin) / (smax - smin) * (fmin - fmax);
        tempFlow = Mathf.Max(flowspeedmin, this[xFrom + 1, yFrom].avgVelocity.x); //sign of nodeVelocity depends on direction of movement
        directedSpeed[0] = tempSpeed + (Mathf.Clamp(this[xFrom + 1, yFrom].density, densityMin, densityMax)  - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[0] = (directedSpeed[0] /*speed in direction*/ + 1 /*time*/ + this[xFrom + 1, yFrom].discomfort /*discomfort*/) / directedSpeed[0] /*speed in direction*/;

        tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xFrom, yFrom + 1) - smin) / (smax - smin) * (fmin - fmax);
        tempFlow = Mathf.Max(flowspeedmin, this[xFrom, yFrom + 1].avgVelocity.y); //if movement is in a positive x or y direction we don't reverse the sign
        directedSpeed[1] = tempSpeed + (Mathf.Clamp(this[xFrom, yFrom + 1].density, densityMin, densityMax) - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[1] = (directedSpeed[1] /*speed in direction*/ + 1 /*time*/ + this[xFrom, yFrom + 1].discomfort /*discomfort*/) / directedSpeed[1] /*speed in direction*/;

        tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xFrom - 1, yFrom) - smin) / (smax - smin) * (fmin - fmax);
        tempFlow = Mathf.Max(flowspeedmin, -this[xFrom - 1, yFrom].avgVelocity.x); //if movement is in a negative x or y direction we do reverse the sign
        directedSpeed[2] = tempSpeed + (Mathf.Clamp(this[xFrom - 1, yFrom].density, densityMin, densityMax)  - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[2] = (directedSpeed[2] /*speed in direction*/ + 1 /*time*/ + this[xFrom - 1, yFrom].discomfort /*discomfort*/) / directedSpeed[2] /*speed in direction*/;

        tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xFrom, yFrom - 1) - smin) / (smax - smin) * (fmin - fmax);
        tempFlow = Mathf.Max(flowspeedmin, -this[xFrom, yFrom - 1].avgVelocity.y); //the idea is that the same velocity can either speed us up or slow us down depending on direction of movement
        directedSpeed[3] = tempSpeed + (Mathf.Clamp(this[xFrom, yFrom - 1].density, densityMin, densityMax)  - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[3] = (directedSpeed[3] /*speed in direction*/ + 1 /*time*/ + this[xFrom, yFrom - 1].discomfort /*discomfort*/) / directedSpeed[3] /*speed in direction*/;

        this[xFrom, yFrom].speed = directedSpeed;

        return returnVector;
    }


    public Vector4 calculateAveragedSpeedAndCost(int xFrom, int yFrom) {
        Vector4 returnVector = Vector4.zero;
        float tempSpeed, tempFlow, directedSpeed;
        Vector2 heightGrads = getAveragedNodeGradients(xFrom, yFrom);

        tempSpeed = fmin + ( (-heightGrads.x - smin) / (smax - smin) ) * (fmax - fmin);
        tempFlow = Mathf.Max(fmin, this[xFrom + 1, yFrom + 1].avgVelocity.x); //sign of nodeVelocity depends on direction of movement
        directedSpeed = tempSpeed + (this[xFrom + 1, yFrom].density - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[0] = (directedSpeed /*speed in direction*/ + 1 /*time*/ + this[xFrom + 1, yFrom].discomfort /*discomfort*/) / directedSpeed /*speed in direction*/;

        tempSpeed = fmin + ((-heightGrads.y - smin) / (smax - smin)) * (fmax - fmin);
        tempFlow = Mathf.Max(0, this[xFrom, yFrom + 1].avgVelocity.y); //if movement is in a positive x or y direction we don't reverse the sign
        directedSpeed = tempSpeed + (this[xFrom, yFrom + 1].density - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[1] = (directedSpeed /*speed in direction*/ + 1 /*time*/ + this[xFrom, yFrom + 1].discomfort /*discomfort*/) / directedSpeed /*speed in direction*/;

        tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xFrom - 1, yFrom) - smin) / (smax - smin) * (fmin - fmax);
        tempFlow = Mathf.Max(0, -this[xFrom - 1, yFrom].avgVelocity.x); //if movement is in a negative x or y direction we do reverse the sign
        directedSpeed = tempSpeed + (this[xFrom - 1, yFrom].density - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[2] = (directedSpeed /*speed in direction*/ + 1 /*time*/ + this[xFrom - 1, yFrom].discomfort /*discomfort*/) / directedSpeed /*speed in direction*/;

        tempSpeed = fmax + (getDirectedNodeGradient(xFrom, yFrom, xFrom, yFrom - 1) - smin) / (smax - smin) * (fmin - fmax);
        tempFlow = Mathf.Max(0, -this[xFrom, yFrom - 1].avgVelocity.y); //the idea is that the same velocity can either speed us up or slow us down depending on direction of movement
        directedSpeed = tempSpeed + (this[xFrom, yFrom - 1].density - densityMin) / (densityMax - densityMin) * (tempFlow - tempSpeed);
        returnVector[3] = (directedSpeed /*speed in direction*/ + 1 /*time*/ + this[xFrom, yFrom - 1].discomfort /*discomfort*/) / directedSpeed /*speed in direction*/;

        return returnVector;
    }

    // Get list of neighbouring nodes
    public List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();
        
        //search in a 3x3 block of nodes
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(this[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    //Get list of straight neighbouring nodes
    private Node[] neighbours = new Node[4];
    public Node[] GetNeighboursImmediate(Node node) {

        //search block of 4 immediate nodes
        int j = 0;
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if ((x + y) % 2 == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours[j++] = (this[checkX, checkY]);
                }
                else {
                    neighbours[j++] = outOfBoundsNode;
                }
            }
        }

        return neighbours;
    }

    // Get indices of node in a given location
    public Node NodeFromWorldPoint(Vector2 worldPosition) {
        return NodeFromWorldPoint(new Vector3(worldPosition.x, 0, worldPosition.y));
    }
    // Get indices of node in a given location
    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        // Get percentages  of how far along the grid, the node is located (x and y)
        float percentX = (worldPosition.x - worldBottomLeft.x) / gridWorldSize.x;
        float percentY = (worldPosition.z + worldBottomLeft.z) / gridWorldSize.y;
        // Clamp percentages between 0 and 1
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        // Get x,y indices of the grid array (-1 because it's zero indexed)
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return this[x, y];
    }


    //return node at grid[x,y]
    Node outOfBoundsNode = new Node(false, new Vector3(-1000,-1000,-1000), -1, -1, Mathf.Infinity);
    public Node this[int x, int y] {
        get {
            if (x < 0 || y < 0 || x >= gridSizeX || y >= gridSizeY) {
                return outOfBoundsNode;
            }
            else return grid[x + gridSizeX * y];
        }
    }

    public Node this[int x] {
        get {
            if (x < 0 || x >= gridSizeX * gridSizeY) {
                return outOfBoundsNode;
            }
            return grid[x];
        }
    }

    public Node this[Vector2 pos]
    {
        get
        {
            return this[(int)pos.x, (int)pos.y];
        }
    }

    void SetHeights() {
        maxHeight = 0;
        minHeight = Mathf.Infinity;

        v2SampleStart = new Vector2(Random.Range(0.0f, (float)gridSizeX), Random.Range(0.0f, (float)gridSizeY));
        Noise noise = new Noise();
        float xCoord, zCoord;
        for (int z = 0; z < gridSizeX; z++) {
            for (int x = 0; x < gridSizeY; x++) {
                xCoord = v2SampleStart.x + x * nodeDiameter * scale;
                zCoord = v2SampleStart.y + z * nodeDiameter * scale;
                float coord1 = power * Mathf.PerlinNoise(xCoord, zCoord);
                float coord2 = power * Mathf.PerlinNoise(xCoord + nodeDiameter * scale, zCoord);
                float coord3 = power * Mathf.PerlinNoise(xCoord, zCoord + nodeDiameter * scale);
                float coord4 = power * Mathf.PerlinNoise(xCoord + nodeDiameter * scale, zCoord + nodeDiameter * scale);
                float avgHeight = randHeightsPerlin ? (coord1 + coord2 + coord3 + coord4) / 4.0f : 0.0f;

                this[x, z].height = avgHeight;
                this[x, z].worldPosition.y = avgHeight;

                maxHeight = maxHeight > avgHeight ? maxHeight : avgHeight;
                float currMin = Mathf.Min(new float[] { coord1, coord2, coord3, coord4 });
                minHeight = minHeight < avgHeight ? minHeight: avgHeight;
            }
        }
        maxHeight -= minHeight;
        for (int z = 0; z < gridSizeX; z++) {
            for (int x = 0; x < gridSizeY; x++) {
                this[x, z].height -= minHeight;
            }
        }

    }

    public float getMaxGCost() {
        float maxGCost = 0;

        for (int z = 0; z < gridSizeX; z++) {
            for (int x = 0; x < gridSizeY; x++) {
                maxGCost = maxGCost > this[x, z].potentialCost || this[x, z].potentialCost == Mathf.Infinity ? maxGCost : this[x, z].potentialCost;
            }
        }
        return maxGCost;
    }
}
