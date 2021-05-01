using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UnitGroup : IEnumerable<Transform> {

    private static int numOfGroups = 0;

    public readonly int groupID;

    public Texture2D unitTexture;

    public float groupDensityCoefficient = 1;

    public float groupDiscomfortCoefficient = 1;

    public ContinuumCrowdsNoMB ccnmb;

    private Transform _prefab;
    public Transform prefab {
        private set { _prefab = value; }
        get { return _prefab; }
    }
    public List<Transform> units;
    public List<Vector2> positions;
    public List<Vector2> velocities;

    public List<Vector2> targetNodes;

    public List<Vector2> startingNodes;

    public int numOfUnits = 0;

    private Grid _grid;

    public enum directions {
        LEFT,
        UP,
        RIGHT,
        DOWN,
    }

    public float[,] densityField;
    public Vector2[,] velocitySumField;
    public Vector4[,] speedField;
    public Vector4[,] costField;
    public float[,] potentialField;

    private Color _groupColor;
    public Color groupColor {
        get { return _groupColor; }
        private set { _groupColor = value; }
    }

    public UnitGroup(Transform prefab, Grid grid)
    {

        groupID = numOfGroups;
        numOfGroups++;

        _grid = grid;

        int gridSizeX = grid.gridSizeX;
        int gridSizeY = grid.gridSizeY;

        this.prefab = prefab;

        densityField = new float[gridSizeX, gridSizeY];
        speedField = new Vector4[gridSizeX, gridSizeY];
        costField = new Vector4[gridSizeX, gridSizeY];
        potentialField = new float[gridSizeX, gridSizeY];

        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++) {
                densityField[i, j] = 0f;
                speedField[i, j] = Vector4.zero;
                costField[i, j] = Vector4.zero;
                potentialField[i, j] = 0f;
            }
        }
        speedField.Initialize();

        targetNodes = new List<Vector2>();
        startingNodes = new List<Vector2>();

        if (groupID == 0) {
            AddTargetNode(new Vector2(80 + 5, 40 + 10));
            AddTargetNode(new Vector2(80 + 5, 41 + 10));
            AddTargetNode(new Vector2(80 + 5, 42 + 10));
            AddTargetNode(new Vector2(80 + 5, 43 + 10));
            AddTargetNode(new Vector2(81 + 5, 40 + 10));
            AddTargetNode(new Vector2(81 + 5, 41 + 10));
            AddTargetNode(new Vector2(81 + 5, 42 + 10));
            AddTargetNode(new Vector2(81 + 5, 43 + 10));
            AddTargetNode(new Vector2(82 + 5, 40 + 10));
            AddTargetNode(new Vector2(82 + 5, 41 + 10));
            AddTargetNode(new Vector2(82 + 5, 42 + 10));
            AddTargetNode(new Vector2(82 + 5, 43 + 10));
        }                 
        if (groupID == 1)
        { 
            AddTargetNode(new Vector2(30 + 5, 40 - 26));
            AddTargetNode(new Vector2(30 + 5, 41 - 26));
            AddTargetNode(new Vector2(30 + 5, 42 - 26));
            AddTargetNode(new Vector2(30 + 5, 43 - 26));
            AddTargetNode(new Vector2(31 + 5, 40 - 26));
            AddTargetNode(new Vector2(31 + 5, 41 - 26));
            AddTargetNode(new Vector2(31 + 5, 42 - 26));
            AddTargetNode(new Vector2(31 + 5, 43 - 26));
            AddTargetNode(new Vector2(32 + 5, 40 - 26));
            AddTargetNode(new Vector2(32 + 5, 41 - 26));
            AddTargetNode(new Vector2(32 + 5, 42 - 26));
            AddTargetNode(new Vector2(32 + 5, 43 - 26));
        }

        units = new List<Transform>();
        positions = new List<Vector2>();
        velocities = new List<Vector2>();

        for (int i = 0; i < 20; i++) {
            AddStartingNode(new Vector2(24.3f + 54f + (float)i % 20, -32.8f + 54f + groupID * 2));
        }
        for (int i = 0 ; i < 200 ; i++)
        {
            AddUnitToRandomStartPos();
        }

    }
    
    public void MoveGroup() {
        foreach (Transform unit in units) {
            unit.transform.GetComponent<Unit>().MoveUnit();
        }
    }

    public void EnableOrDisableGroupRendering()
    {
        Renderer renderer;
        foreach (Transform unit in units)
        {
            renderer = unit.transform.GetComponent<Renderer>();
            renderer.enabled = !renderer.enabled;
        }
    }

    public void AddTargetNode (Vector2 position)
    {
        position = RoundVector2ElementsToInt(position);
        if (!targetNodes.Contains(position) && position.x < _grid.gridSizeX && position.y < _grid.gridSizeY)
        {
            targetNodes.Add(position);
        }
    }
    public void RemoveTargetNode (Vector2 position)
    {
        position = RoundVector2ElementsToInt(position);
        targetNodes.Remove(position);
    }

    public void AddStartingNode (Vector2 position)
    {
        position = RoundVector2ElementsToInt(position);
        if (!startingNodes.Contains(position) && position.x < _grid.gridSizeX && position.y < _grid.gridSizeY)
        {
            startingNodes.Add(position);
        }
    }

    public void RemoveStartingNode (Vector2 position)
    {
        position = RoundVector2ElementsToInt(position);
        startingNodes.Remove(position);
    }

    public void AddUnit(Vector3 position) {
        if (!_grid[(int)position.x, (int)position.z].isWalkable)
            return;
        Transform clone = (Transform)GameObject.Instantiate(this.prefab, position, this.prefab.rotation);
        clone.transform.GetComponent<Unit>().group = this;
        clone.transform.GetComponent<Unit>().groupUnitIndex = numOfUnits++;
        Unit.overallCreatedUnits++;
        units.Add(clone);
        positions.Add(new Vector2(clone.position.x, clone.position.z));
        velocities.Add(Vector2.zero);
    }

    public void AddUnit (Vector2 position)
    {
        if (!_grid[(int)position.x, (int)position.y].isWalkable)
            return;
        Vector3 unitpos = new Vector3(position.x, 0.1f, position.y);
        Transform clone = (Transform)GameObject.Instantiate(this.prefab, unitpos, this.prefab.rotation);
        clone.transform.GetComponent<Unit>().group = this;
        clone.transform.GetComponent<Unit>().groupUnitIndex = numOfUnits++;
        Unit.overallCreatedUnits++;
        units.Add(clone);
        positions.Add(new Vector2(clone.position.x, clone.position.z));
        velocities.Add(Vector2.zero);
    }

    public void AddUnitToRandomStartPos()
    {

        uint stNodeIndex = (uint)LCGRand.Next((uint)units.Count) % (uint)startingNodes.Count;
        Vector2 startingNode = startingNodes[(int)stNodeIndex];

        // Randomize position inside starting node
        float random1 = Mathf.PerlinNoise(startingNode.y + 1, startingNode.y - 1);
        float random2 = Mathf.PerlinNoise(startingNode.x + 1, startingNode.x - 1);
        float perlRan = Mathf.PerlinNoise(random1, random2);
        float posCoefX = Mathf.PerlinNoise(units.Count + 42, perlRan);
        float posCoefY = Mathf.PerlinNoise(perlRan, units.Count - 42);

        posCoefX = Mathf.Abs(posCoefX) / 0.9f;
        posCoefY = Mathf.Abs(posCoefY) / 0.9f;

        Vector3 positionInStartingNode = new Vector3(startingNode.x + posCoefX, 0.1f, startingNode.y + posCoefY);

        Transform clone = (Transform)GameObject.Instantiate(this.prefab, positionInStartingNode, this.prefab.rotation);
        clone.transform.GetComponent<Unit>().group = this;
        clone.transform.GetComponent<Unit>().groupUnitIndex = numOfUnits++;

        units.Add(clone);
        positions.Add(new Vector2(clone.position.x, clone.position.z));
        velocities.Add(Vector2.zero);
    }

    public void RemoveUnit(Transform unit) {
        if (units.Contains(unit)) {
            units.Remove(unit);
            Transform.Destroy(unit);
        }
    }

    public void RemoveUnit()
    {
        Transform unit = units[units.Count - 1];
        units.RemoveAt(units.Count - 1);
        GameObject.Destroy(unit.gameObject);
        Unit.numOfUnits--;
    }

    public void AttachContCrowds(ContinuumCrowdsNoMB cc)
    {
        ccnmb = cc;
    }

    public void ResetGroupPositions()
    {
        for (int i = 0 ; i < units.Count ; i++)
        {
            units[i].transform.GetComponent<Unit>().ResetUnitToStartPosition();
        }
    }

    public IEnumerator<Transform> GetEnumerator() {
        return ((IEnumerable<Transform>)units).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<Transform>)units).GetEnumerator();
    }


    public void UpdateTransformPositions()
    {
        foreach (Transform unit in units)
        {
            unit.GetComponent<Unit>().UpdateTransformPosition();
        }
    }

    private Vector2 RoundVector2ElementsToInt (Vector2 vect)
    {
        return new Vector2((int)vect.x, (int)vect.y);
    }

}

public static class CardinalDirections {
    public const int LEFT = 0;
    public const int UP = 1;
    public const int RIGHT = 2;
    public const int DOWN = 3;
}