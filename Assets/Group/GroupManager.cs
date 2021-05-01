using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GroupManager : MonoBehaviour, IEnumerable<Transform>
{
    public int numOfGroups = 2;

    public int numOfUnits = 0;

    int _parkedUnits = 0;
    public int parkedUnits
    {
        get { return _parkedUnits; }
        set { _parkedUnits = value; }
    }

    private Grid _grid;

    public const int MAX_GROUP_NUM = 2;

    public UnitGroup[] groups;

    CollisionResolver collisionResolver;

    private bool _advancedMovement = true;
    public bool AdvancedMovement
    {
        get { return _advancedMovement; }
        set { _advancedMovement = value; }
    }

    private bool _immediateRestart = true;
    public bool ImmediateRestart
    {
        get { return _immediateRestart; }
        set { _immediateRestart = value; }
    }

    private bool _isCheckingWhereToTurn = false;
    public bool IsCheckingWhereToTurn
    {
        get { return _isCheckingWhereToTurn; }
        set { _isCheckingWhereToTurn = value; }
    }

    private bool _isResolvingCollisions = true;
    public bool IsResolvingCollisions
    {
        get { return _isResolvingCollisions; }
        set { _isResolvingCollisions = value; }
    }

    public Transform[] prefab;

    public ContinuumCrowdsNoMB[] ccnmb;

    void Start ()
    {
        if (numOfGroups > MAX_GROUP_NUM || numOfGroups <= 0)
        {
            numOfGroups = MAX_GROUP_NUM;
        }
        _grid = FindObjectOfType<Grid>();
        groups = new UnitGroup[numOfGroups];
        ccnmb = new ContinuumCrowdsNoMB[numOfGroups];

        UnityEngine.Random.InitState(numOfGroups);

        for (int i = 0; i < numOfGroups; i++) {
            groups[i] = new UnitGroup(prefab[i], _grid);
            ccnmb[i] = new ContinuumCrowdsNoMB(_grid, this, i);
            groups[i].ccnmb = ccnmb[i];
        }

        collisionResolver = new CollisionResolver(_grid.gridSizeX * _grid.gridSizeY, _grid.gridSizeX, _grid.gridSizeY, this);

    }
	

    public void RunContCrowdsLoopOnce()
    {
        for (int i = 0 ; i < numOfGroups ; i++)
        {
            ccnmb[i].RunCC();
        }
    }

    public void ResetAllGroupsPositions()
    {
        foreach (UnitGroup group in groups)
        {
            group.ResetGroupPositions();
        }
    }

    public void EnableOrDisableGroupRendering (int groupNum)
    {
        groups[groupNum].EnableOrDisableGroupRendering();
    }


    public int GetUnitCount()
    {
        var sum = 0;
        foreach (UnitGroup group in groups)
        {
            sum += group.units.Count;
        }
        return sum;
    }


    public List<Transform> getUnitsAtNode(int x, int y) {
        List<Transform> nodeUnits = new List<Transform>();
        foreach (UnitGroup untgrp in groups) {
            foreach (Transform unit in untgrp.units) {
                Node unitGridNode = _grid.NodeFromWorldPoint(unit.transform.position);
                if (unitGridNode.gridX == x && unitGridNode.gridY == y)
                    nodeUnits.Add(unit);
            }
        }
        return nodeUnits;
    }

    public void MoveAllUnits()
    {
        for (int i = 0 ; i < numOfGroups ; i++)
        {
            groups[i].MoveGroup();
        }
        if (IsResolvingCollisions)
            ResolveCollisions();
        UpdateAllUnitTransforms();
    }

    public void ToggleResolveCollisions() { 
}

    public void ResolveCollisions()
    {
        collisionResolver.ResetBucketCounts();
        foreach (Transform unit in this)
        {
            collisionResolver.SortAgentsIntoBuckets(unit.GetComponent<Unit>());
        }
        foreach (Transform unit in this)
        {
            collisionResolver.ResolveCollisions(unit.GetComponent<Unit>());
        }

    }

    public void UpdateAllUnitTransforms()
    {
        foreach (UnitGroup group in groups)
        {
            group.UpdateTransformPositions();
        }
    }

    public int GetAgentCount(int groupNum)
    {
        return groups[groupNum].units.Count;
    }
    public void SetAgentCount(int groupNum, int newCount)
    {
        if (newCount > groups[groupNum].units.Count)
        {
            while (newCount > groups[groupNum].units.Count)
            {
                groups[groupNum].AddUnitToRandomStartPos();
            }
        }
        if (newCount < groups[groupNum].units.Count)
        {
            while (newCount < groups[groupNum].units.Count)
            {
                groups[groupNum].RemoveUnit();
            }
        }
    }

    public IEnumerable<Transform> getListOfUnits() {
        List<Transform> nodeUnits = new List<Transform>();
        nodeUnits.Concat(groups[0].units);
        nodeUnits.Concat(groups[1].units);
        return nodeUnits;
    }

    public IEnumerator<Transform> GetEnumerator() {
        foreach (UnitGroup group in groups) {
            foreach (Transform unit in group) {
                yield return unit;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
