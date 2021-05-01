using UnityEngine;

public class Node : IHeapItem<Node> {

    public static int count;

    public enum States {
        FAR,
        NARROW_BAND,
        FROZEN
    }

    States state;
    public States State {
        get { return this.state; }
        set { this.state = value; }
    }

    public enum GroundTypes {
        ROAD = 1,
        GROUND = 2,
        BOG = 3,
        WALL = 255
    }


    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public Vector2 flowDir;
    public float height;
    private float _discomfort;
    public float discomfort
    {
        get
        {
            return _discomfort;
        }
        set
        {
            if (value >= 0.0f)
                _discomfort = value;
            else
                _discomfort = 0.0f;
        }
    }

    float _density = 0;
    Vector2 _velocitySum = Vector2.zero;

    bool resetDensityFlag = true;
    bool resetAvgVelocityFlag = true;

    public Vector4 speed;

    public int nodeType;

    private float gcost; // Default == 0
    private float hcost; // Default == 0

    public Node parent;
    int heapIndex;

    public float density {
        get {
            if (resetDensityFlag == true) {
                resetDensityFlag = false;
                _density = 0;
            }
            return _density;
        }
        set {
            _density = value;
        }
    }

    public Vector2 velocitySum {
        get {
            if (resetAvgVelocityFlag) {
                resetAvgVelocityFlag = false;
                _velocitySum = Vector2.zero;
            }
            return _velocitySum;
        }
        set {
            _velocitySum = value;
        }
    }

    public Vector2 avgVelocity {
        get {
            //if (isTargetNode)
             //   return Vector2.zero;
            return velocitySum / (density != 0 ? density : 1); }
    }

    public float unitCost {
        get { return gcost; }
        set { gcost = value; }// * (float)nodeType;
    }
    public float potentialCost {
        get { if (this.isWalkable) return hcost; else return Mathf.Infinity; }
        set { hcost = value; }// * (float)nodeType;
    }

    public Node(bool walkable, Vector3 worldPos, int _gridX, int _gridY) {
        if (isWalkable = walkable) {
            density = 0;
            height = 0;
            discomfort = 0;
        }
        else {
            density = 0;
            height = 0;
            discomfort = Mathf.Infinity;
            potentialCost = Mathf.Infinity;
        }
        worldPosition = worldPos;
        gridX = _gridX;
        gridY = _gridY;
        //nodeType = count;
        nodeType = (int)GroundTypes.ROAD;
        //nodeType = Random.Range(0, 256);
        unitCost = Mathf.Infinity;
        potentialCost = Mathf.Infinity;
        parent = null;
        state = States.FAR;
        flowDir = new Vector2(0, 0);
    }

    public Node(bool walkable, Vector3 worldPos, int _gridX, int _gridY, float _height) {
        if (isWalkable = walkable) {
            density = 0;
            height = _height;
            discomfort = 0;
        }
        else {
            density = 0;
            height = _height;
            discomfort = Mathf.Infinity;
            potentialCost = Mathf.Infinity;
        }
        worldPosition = worldPos;
        gridX = _gridX;
        gridY = _gridY;
        
        nodeType = (int)GroundTypes.ROAD;
        unitCost = Mathf.Infinity;
        potentialCost = Mathf.Infinity;
        parent = null;
        state = States.FAR;
        flowDir = new Vector2(0, 0);
    }

    public Node(bool walkable, Vector3 worldPos, int _gridX, int _gridY, float _height, int _nodeType) {
        if (isWalkable = walkable) {
            density = 0;
            height = _height;
            discomfort = 0;
        }
        else {
            density = 0;
            height = _height;
            discomfort = Mathf.Infinity;
            potentialCost = Mathf.Infinity;
        }
        worldPosition = worldPos;
        gridX = _gridX;
        gridY = _gridY;

        nodeType = _nodeType;
        unitCost = Mathf.Infinity;
        potentialCost = Mathf.Infinity;
        parent = null;
        state = States.FAR;
        flowDir = new Vector2(0, 0);
    }

    public void SetState(States _state) {
        this.state = _state;
    }

    public void ResetNode() {
        //potentialCost = Mathf.Infinity;
        unitCost = Mathf.Infinity;
        state = States.FAR;
        if (!isWalkable) {
            density = 0;
            potentialCost = Mathf.Infinity;
            velocitySum = Vector2.zero;
        }
        else {
            resetDensityFlag = true;
            resetAvgVelocityFlag = true;
        }
    }
    public void resetDensity() {
        if (isWalkable) {
            density = 0;
        }
        else {
            density = 0;
        }
        velocitySum = Vector2.zero;
    }

    public float fCost {
        get {
            return unitCost + potentialCost;
        }
    }

    public int HeapIndex {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    public void MakeWall()
    {
        if (isWalkable)
        {
            isWalkable = false;
            discomfort = Mathf.Infinity;
        }
    }
    public void MakeWalkable ()
    {
        if (!isWalkable)
        {
            isWalkable = true;
            discomfort = 0.0f;
        }
    }

    public int CompareTo(Node nodeToCompare) {
        int compare = potentialCost.CompareTo(nodeToCompare.potentialCost);
        if (compare == 0) {
            compare = potentialCost.CompareTo(nodeToCompare.potentialCost);
        }
        return -compare;
    }

    public static float HeightDiff(Node node1, Node node2) {
        return node2.height - node1.height;
    }

}
