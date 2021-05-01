using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class ContinuumCrowdsNoMB
{

    enum States
    {
        FAR,
        NARROW_BAND,
        FROZEN
    }

    Grid _grid;
    GroupManager groupManager;

    int dimX, dimY;

    States[] nodeStates;

    public WallField wallField;
    public DiscomfortField discomfortField;
    public DensityField densityField;
    public VelocitySumField velocitySumField;
    public SpeedField speedField;
    public CostField costField;
    public PotentialField potentialField;
    public GradientField gradientField;

    int groupIndex;

    bool[] resetNode;

    public Transform target;
    public List<Node> targetNodes;

    HeapItemFloatKey2Dindex[] heapItems;
    Heap<HeapItemFloatKey2Dindex> openSet;
    HashSet<HeapItemFloatKey2Dindex> closedSet;

    void ResetGrid()
    {
        for (int i = 0; i < dimX * dimY; i++)
        {
            densityField[i] = 0.0f;
            if (!_grid[i].isWalkable)
                discomfortField.SetBaseDiscomfort(i, _grid[i].discomfort);
            else
                if (!float.IsInfinity(discomfortField[i]))
            {
                discomfortField.ResetNode(i);
            }
            else
                discomfortField.SetBaseDiscomfort(i, _grid[i].discomfort);

            velocitySumField[i] = new Vector2(0, 0);
            nodeStates[i] = States.FAR;
            speedField[i] = new Vector4(Fmin, Fmin, Fmin, Fmin);
            costField[i] = new Vector4(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            gradientField[i] = new Vector2(0, 0);
            if (potentialField[i] != 0.0f) //if not target node
                potentialField[i] = Mathf.Infinity;
            resetNode[i] = false;
        }
        closedSet.Clear();
    }


    public ContinuumCrowdsNoMB(Grid grid, GroupManager gm, int groupNum)
    {
        _grid = grid;
        groupManager = gm;
        groupIndex = groupNum;
        dimX = _grid.gridSizeX;
        dimY = _grid.gridSizeY;
        wallField = new WallField(dimX, dimY);
        discomfortField = new DiscomfortField(dimX, dimY);
        densityField = new DensityField(dimX, dimY);
        velocitySumField = new VelocitySumField(dimX, dimY);
        speedField = new SpeedField(dimX, dimY);
        costField = new CostField(dimX, dimY);
        potentialField = new PotentialField(dimX, dimY);
        gradientField = new GradientField(dimX, dimY);
        nodeStates = new States[dimX * dimY];
        resetNode = new bool[dimX * dimY];

        for (int i = 0; i < dimX * dimY; i++)
        {
            wallField[i] = _grid[i].isWalkable;
            if (wallField[i] == false)
            {
                densityField[i] = float.PositiveInfinity;
                discomfortField.SetBaseDiscomfort(i, float.PositiveInfinity);
            }
            else
            {
                densityField[i] = 0.0f;
                discomfortField.SetBaseDiscomfort(i, _grid[i].discomfort);
            }
            nodeStates[i] = States.FAR;
            resetNode[i] = true;
        }

        targetNodes = new List<Node>();

        heapItems = new HeapItemFloatKey2Dindex[dimX * dimY];
        for (int i = 0; i < dimY; i++)
        {
            for (int j = 0; j < dimX; j++)
            {
                heapItems[j + dimX * i] = new HeapItemFloatKey2Dindex(j, i);
            }
        }
        openSet = new Heap<HeapItemFloatKey2Dindex>(dimX * dimY);
        closedSet = new HashSet<HeapItemFloatKey2Dindex>();
    }

    public bool closeThread = false;
    public bool doneCalculations = false;

    public void RunCC()
    {
        CalculateDensityAndVelocity(groupIndex);
        CalculateSpeedAndCostFields(groupIndex);
        FastMarchingMethod();
    }

    public void FastMarchingMethod()
    {
        closedSet.Clear();

        // add all targetNodes to closedSet
        foreach (Vector2 tNode in groupManager.groups[groupIndex].targetNodes)
        {
            if (!_grid[tNode].isWalkable)
            {
                potentialField[tNode] = Mathf.Infinity;
                gradientField[tNode] = new Vector2(0f, 0f);
            }
            else
            {
                potentialField[tNode] = 0f;
                heapItems[(int)tNode.x + dimX * (int)tNode.y].Key = 0f;
                closedSet.Add(heapItems[(int)tNode.x + dimX * (int)tNode.y]);
                nodeStates[(int)tNode.x + (int)tNode.y * dimX] = States.FROZEN;
            }
        }

        // Initial openSet loop 2p0!!
        foreach (HeapItemFloatKey2Dindex tNode in closedSet)
        {
            InitiateOpenSetNode(tNode.x + 1, tNode.y);
            InitiateOpenSetNode(tNode.x, tNode.y + 1);
            InitiateOpenSetNode(tNode.x - 1, tNode.y);
            InitiateOpenSetNode(tNode.x, tNode.y - 1);
        }

        // while open != 0 do
        HeapItemFloatKey2Dindex currentNode;
        int posX, posY;
        while (openSet.Count > 0)
        {
            // S := open.Pop();
            currentNode = openSet.RemoveFirst();
            posX = currentNode.x;
            posY = currentNode.y;
            // closed := closed U {S};
            closedSet.Add(currentNode); //on first iteration freezes start node
            nodeStates[posX + posY * dimX] = States.FROZEN;
            InitiateOpenSetNode(posX + 1, posY);
            InitiateOpenSetNode(posX, posY + 1);
            InitiateOpenSetNode(posX - 1, posY);
            InitiateOpenSetNode(posX, posY - 1);
        }

        foreach (HeapItemFloatKey2Dindex n in closedSet)
        {
            posX = n.x;
            posY = n.y;
            CalcuateGradientForNode(posX, posY);
            if (float.IsInfinity(potentialField[posX + 1, posY]))
                CalcuateGradientForNode(posX + 1, posY);
            if (float.IsInfinity(potentialField[posX, posY + 1]))
                CalcuateGradientForNode(posX, posY + 1);
            if (float.IsInfinity(potentialField[posX - 1, posY]))
                CalcuateGradientForNode(posX - 1, posY);
            if (float.IsInfinity(potentialField[posX, posY - 1]))
                CalcuateGradientForNode(posX, posY - 1);
            
        }

    }

    private void CalcuateGradientForNode(int posX, int posY)
    {
        Vector2 positive = Vector2.zero;
        Vector2 negative = Vector2.zero;
        Vector2 center = Vector2.zero;
        Vector2 differenceQuotientMinus = Vector2.zero;
        Vector2 differenceQuotientPlus = Vector2.zero;
        Vector2 gradVect = Vector2.zero;

        positive.x = potentialField[posX + 1, posY];
        positive.y = potentialField[posX, (posY + 1)];
        negative.x = potentialField[posX - 1, posY];
        negative.y = potentialField[posX, (posY - 1)];
        center.x = potentialField[posX, posY];
        center.y = potentialField[posX, posY];

        differenceQuotientMinus = center - negative;
        differenceQuotientPlus = positive - center;

        gradVect.x = negative.x <= positive.x ? differenceQuotientMinus.x : differenceQuotientPlus.x;
        gradVect.y = negative.y <= positive.y ? differenceQuotientMinus.y : differenceQuotientPlus.y;

        // in the case of 1-wide passages
        if (float.IsInfinity(positive.x) && float.IsInfinity(negative.x))
        {
            gradVect.x = 0;
        }
        if (float.IsInfinity(positive.y) && float.IsInfinity(negative.y))
        {
            gradVect.y = 0;
        }

        gradientField[posX, posY] = gradVect;
    }

    private void InitiateOpenSetNode(int x, int y)
    {
        int nbrPos = x + y * dimX;
        if (x < 0 || y < 0 || x >= dimX || y >= dimY)
        {
            UnityEngine.Debug.Log("Sentinel");
        }

        if (nodeStates[nbrPos] == States.FROZEN || x < 0 || y < 0 || x >= dimX || y >= dimY)
        {
            return;
        }
        else
        { // ((x <= 0 || y <= 0 || x >= dimX-1 || y >= dimY-1))
            if ((!_grid[x, y].isWalkable))
            {
                potentialField[nbrPos] = Mathf.Infinity;
                gradientField[nbrPos] = new Vector2(0f, 0f);
                return;
            }

            if (nodeStates[nbrPos] != States.NARROW_BAND)
            {
                potentialField[nbrPos] = Mathf.Infinity;
                ComputePotential(x, y);
                heapItems[nbrPos].Key = potentialField[nbrPos];
                openSet.Add(heapItems[nbrPos]);
                nodeStates[nbrPos] = States.NARROW_BAND;
            }
            else
            {
                ComputePotential(x, y);
                heapItems[nbrPos].Key = potentialField[nbrPos];
                openSet.UpdateItem(heapItems[nbrPos]);
            }
        }
    }

    // we want (1-agentRadius)^lambda = 0.5
    // for agentRadius = 0.4 avgRo == 0.3904161265
    static readonly float coef = Mathf.Sqrt(0.5f) - 0.4f;
    readonly float densityExponent = Mathf.Log(coef, 0.5f);
    bool isCalculatingPredictiveDiscomfort = true;
    int stepsInTheFuture = 5;
    public void CalculateDensityAndVelocity(int groupNum)
    {
        ResetGrid();
        potentialField.ResetMaxPotential();

        UnitGroup group = groupManager.groups[groupNum];
        float densityToAdd1, densityToAdd2, densityToAdd3, densityToAdd4;
        float discomfortToAdd1, discomfortToAdd2, discomfortToAdd3, discomfortToAdd4;
        Unit unitinfo;
        Vector2 unitVelocity;
        int cellX, cellY;
        float distFromCenterX, distFromCenterY;
        float radius = _grid.nodeRadius;

        float invMaxDist = 0.0002f; // at which distance from each node should we just set density and discomfort to 0

        foreach (Transform unit in groupManager)
        {
            unitinfo = unit.transform.GetComponent<Unit>();
            if (unitinfo.isParked)
            {
                continue;
            }

            unitinfo.GetPosition(out Vector2 unitPos);
            float densityCoefficient = unitinfo.group.groupDensityCoefficient;

            cellX = (int)(unitPos.x - radius); // closest cell center with coords lower...
            cellY = (int)(unitPos.y - radius); // ... than unit position
                                               //if (_grid[cellX, cellY].isTargetNode || _grid[cellX + 1, cellY].isTargetNode || _grid[cellX + 1, cellY + 1].isTargetNode || _grid[cellX, cellY + 1].isTargetNode)
                                               //    continue;

            distFromCenterX = unitPos.x - (cellX + radius);
            distFromCenterY = unitPos.y - (cellY + radius);

            distFromCenterX = distFromCenterX <= invMaxDist && distFromCenterX >= 0 ? 0.0001f : distFromCenterX;
            distFromCenterY = distFromCenterY <= invMaxDist && distFromCenterY >= 0 ? 0.0001f : distFromCenterY;

            if (!(distFromCenterX < 0 || distFromCenterY < 0 || distFromCenterX >= dimX || distFromCenterY >= dimY))
            {
                densityToAdd1 = (densityCoefficient) * Mathf.Pow(Mathf.Min(1f - distFromCenterX, 1f - distFromCenterY), densityExponent);
                densityToAdd2 = (densityCoefficient) * Mathf.Pow(Mathf.Min(distFromCenterX, 1f - distFromCenterY), densityExponent);
                densityToAdd3 = (densityCoefficient) * Mathf.Pow(Mathf.Min(distFromCenterX, distFromCenterY), densityExponent);
                densityToAdd4 = (densityCoefficient) * Mathf.Pow(Mathf.Min(1f - distFromCenterX, distFromCenterY), densityExponent);

                densityField[cellX, cellY] += densityToAdd1;
                densityField[cellX + 1, cellY] += densityToAdd2;
                densityField[cellX + 1, (cellY + 1)] += densityToAdd3;
                densityField[cellX, (cellY + 1)] += densityToAdd4;

                unitVelocity = unit.transform.GetComponent<Unit>().GetAgentVelocity();

                velocitySumField[cellX, cellY] += densityToAdd1 * unitVelocity;
                velocitySumField[cellX + 1, cellY] += densityToAdd2 * unitVelocity;
                velocitySumField[cellX + 1, (cellY + 1)] += densityToAdd3 * unitVelocity;
                velocitySumField[cellX, (cellY + 1)] += densityToAdd4 * unitVelocity;
            }
            if (isCalculatingPredictiveDiscomfort)
            {
                unitinfo.GetLinearPredictivePosition(stepsInTheFuture, out Vector2 futurePos);

                float discomfortCoefficient = unitinfo.group.groupDiscomfortCoefficient;

                cellX = (int)(futurePos.x - radius);
                cellY = (int)(futurePos.y - radius);
                distFromCenterX = futurePos.x - (cellX + radius);
                distFromCenterY = futurePos.y - (cellY + radius);

                distFromCenterX = distFromCenterX <= invMaxDist && distFromCenterX >= 0 ? 0.0001f : distFromCenterX;
                distFromCenterY = distFromCenterY <= invMaxDist && distFromCenterY >= 0 ? 0.0001f : distFromCenterY;

                if (!(distFromCenterX < 0 || distFromCenterY < 0 || distFromCenterX >= dimX || distFromCenterY >= dimY))
                {
                    discomfortToAdd1 = (discomfortCoefficient / group.groupDiscomfortCoefficient) * Mathf.Pow(Mathf.Min(1f - distFromCenterX, 1f - distFromCenterY), densityExponent);
                    discomfortToAdd2 = (discomfortCoefficient / group.groupDiscomfortCoefficient) * Mathf.Pow(Mathf.Min(distFromCenterX, 1f - distFromCenterY), densityExponent);
                    discomfortToAdd3 = (discomfortCoefficient / group.groupDiscomfortCoefficient) * Mathf.Pow(Mathf.Min(distFromCenterX, distFromCenterY), densityExponent);
                    discomfortToAdd4 = (discomfortCoefficient / group.groupDiscomfortCoefficient) * Mathf.Pow(Mathf.Min(1f - distFromCenterX, distFromCenterY), densityExponent);

                    discomfortField[cellX, cellY] += discomfortToAdd1;
                    discomfortField[cellX + 1, cellY] += discomfortToAdd2;
                    discomfortField[cellX + 1, (cellY + 1)] += discomfortToAdd3;
                    discomfortField[cellX, (cellY + 1)] += discomfortToAdd4;
                }

            }
        }
    }


    float fmin = 0.01f, fmax = 12f, smin = -10.0f, smax = 10.0f, densityMin = 0.5f, densityMax = 0.9f;
    float weight_length = 1.0f, weight_time = 1.0f, weight_discomfort = 1.0f;
    private void CalculateSpeedAndCostFields(int groupNum)
    {
        float topographicalSpeed, flowSpeed;
        Vector4 directedCost = Vector4.zero;
        Vector4 directedSpeed = Vector4.one * Fmin;// = Vector4(Fmin, Fmin, Fmin, Fmin);
        for (int i = 0; i < _grid.gridSizeX; i++)
        {
            for (int j = 0; j < _grid.gridSizeY; j++)
            {
                topographicalSpeed = Fmax;

                flowSpeed = Mathf.Max(Fmin, (densityField[i + 1, j] == 0.0f ? topographicalSpeed : velocitySumField[i + 1, j].x / densityField[i + 1, j])); //sign of nodeVelocity depends on direction of movement
                directedSpeed[0] = topographicalSpeed + (Mathf.Clamp(densityField[i + 1, j], DensityMin, DensityMax) - DensityMin) / (DensityMax - DensityMin) * (flowSpeed - topographicalSpeed);
                directedCost[0] = (weight_length * directedSpeed[0] /*speed in direction*/ + weight_time * 1 /*time*/ + weight_discomfort * discomfortField[i + 1, j] /*discomfort*/) / directedSpeed[0] /*speed in direction*/;
                if (float.IsNaN(directedCost[0]))
                    directedCost[0] = float.PositiveInfinity;
                
                flowSpeed = Mathf.Max(Fmin, (densityField[i, j + 1] == 0.0f ? topographicalSpeed : velocitySumField[i, j + 1].y / densityField[i, j + 1])); //if movement is in a positive x or y direction we don't reverse the sign
                directedSpeed[1] = topographicalSpeed + (Mathf.Clamp(densityField[i, j + 1], DensityMin, DensityMax) - DensityMin) / (DensityMax - DensityMin) * (flowSpeed - topographicalSpeed);
                directedCost[1] = (weight_length * directedSpeed[1] /*speed in direction*/ + weight_time * 1 /*time*/ + weight_discomfort * discomfortField[i, j + 1] /*discomfort*/) / directedSpeed[1] /*speed in direction*/;
                if (float.IsNaN(directedCost[1]))
                    directedCost[1] = float.PositiveInfinity;
                
                flowSpeed = Mathf.Max(Fmin, (densityField[i - 1, j] == 0.0f ? topographicalSpeed : -velocitySumField[i - 1, j].x / densityField[i - 1, j])); //if movement is in a negative x or y direction we do reverse the sign
                directedSpeed[2] = topographicalSpeed + (Mathf.Clamp(densityField[i - 1, j], DensityMin, DensityMax) - DensityMin) / (DensityMax - DensityMin) * (flowSpeed - topographicalSpeed);
                directedCost[2] = (weight_length * directedSpeed[2] /*speed in direction*/ + weight_time * 1 /*time*/ + weight_discomfort * discomfortField[i - 1, j] /*discomfort*/) / directedSpeed[2] /*speed in direction*/;
                if (float.IsNaN(directedCost[2]))
                    directedCost[2] = float.PositiveInfinity;
                
                flowSpeed = Mathf.Max(Fmin, (densityField[i, j - 1] == 0.0f ? topographicalSpeed : -velocitySumField[i, j - 1].y / densityField[i, j - 1])); //the idea is that the same velocity can either speed us up or slow us down depending on direction of movement
                directedSpeed[3] = topographicalSpeed + (Mathf.Clamp(densityField[i, j - 1], DensityMin, DensityMax) - DensityMin) / (DensityMax - DensityMin) * (flowSpeed - topographicalSpeed);
                directedCost[3] = (weight_length * directedSpeed[3] /*speed in direction*/ + weight_time * 1 /*time*/ + weight_discomfort * discomfortField[i, j - 1] /*discomfort*/) / directedSpeed[3] /*speed in direction*/;
                if (float.IsNaN(directedCost[3]))
                    directedCost[3] = float.PositiveInfinity;
                speedField[i + j * dimX] = directedSpeed;
                costField[i + j * dimX] = directedCost;
            }
        }
    }

    void ComputePotential(int posX, int posY)
    {
        ComputeSimple(posX, posY);
    }

    public float Fmin
    {
        get { return fmin; }
        set { fmin = value; }
    }

    public float Fmax
    {
        get { return fmax; }
        set { fmax = value; }
    }

    public float Smin
    {
        get { return smin; }
        set { smin = value; }
    }

    public float Smax
    {
        get { return smax; }
        set { smax = value; }
    }

    public float DensityMin
    {
        get { return densityMin; }
        set { densityMin = value; }
    }

    public float DensityMax
    {
        get { return densityMax; }
        set { densityMax = value; }
    }

    public float Weight_length
    {
        get { return weight_length; }
        set { weight_length = value; }
    }

    public float Weight_time
    {
        get { return weight_time; }
        set { weight_time = value; }
    }

    public float Weight_discomfort
    {
        get { return weight_discomfort; }
        set { weight_discomfort = value; }
    }

    public bool IsCalculatingPredictiveDiscomfort
    {
        get { return isCalculatingPredictiveDiscomfort; }
        set { isCalculatingPredictiveDiscomfort = value; }
    }

    public int StepsInTheFuture
    {
        get { return stepsInTheFuture; }
        set { stepsInTheFuture = value; }
    }

    void ComputeSimple(int posX, int posY)
    {
        Vector4 potentials = new Vector4(potentialField[posX + 1, posY],  // east
                                         potentialField[posX, posY + 1],  // north
                                         potentialField[posX - 1, posY],  // west
                                         potentialField[posX, posY - 1]); // east

        Vector4 costs = costField[posX, posY];

        bool isLessThanWE = (potentials[2] + costs[2]) <= (potentials[0] + costs[0]);

        float potentialX;
        float costX;
        if (isLessThanWE)
        {
            potentialX = potentials[2];
            costX = costs[2];
        }
        else 
        {
            potentialX = potentials[0];
            costX = costs[0];
        }

        bool isLessThanSN = (potentials[3] + costs[3]) <= (potentials[1] + costs[1]);

        float potentialY;
        float costY;
        if (isLessThanSN)
        {
            potentialY = potentials[3];
            costY = costs[3];
        }
        else
        {
            potentialY = potentials[1];
            costY = costs[1];
        }

        float solution = SolveQuadratic(potentialX, potentialY, costX, costY);

        potentialField[posX, posY] = Mathf.Min(solution, potentialField[posX, posY]);
    }

    float SolveQuadratic(float potentialX, float potentialY, float costX, float costY)
    {
        float solution;

        if (costX <= potentialX - potentialY || costY <= potentialY - potentialX)
            solution = Mathf.Min(potentialX + costX, potentialY + costY);
        else
        {
            float costXSquared = costX * costX;
            float costYSquared = costY * costY;
            float determinant = -costXSquared * costYSquared * ((potentialX - potentialY) * (potentialX - potentialY) - (costXSquared + costYSquared));

            solution = (Mathf.Sqrt(determinant) + potentialX * costYSquared + potentialY * costXSquared) / (costXSquared + costYSquared);
        }
        return solution;
    }
}
