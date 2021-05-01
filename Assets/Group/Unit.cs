using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

    public static int numOfUnits = 0;
    public static long overallCreatedUnits;
    public int universalUnitID;
    public int groupUnitIndex;

    public Vector2 blortogonalDirection;

    GroupManager _groupManager;

    private bool reachedTarget = false;

    Vector3[] path;
    int targetIndex;
    Grid _grid;
    public UnitGroup group;

    Vector2 parkPosition = new Vector2(-50f, -50f);

    private bool _isParked = false;
    public bool isParked
    {
        get { return _isParked; }
        private set {
            if (_isParked && !value)
            {
                _isParked = false;
                _groupManager.parkedUnits--;
            }
            else if (!_isParked && value)
            {
                _isParked = true;
                _groupManager.parkedUnits++;
            }
        }
    }

    public Vector2 AgentPosition
    {
        get
        {
            return agentPosition;
        }

        set
        {
            agentPosition = value;
        }
    }

    public float AgentRadius
    {
        get
        {
            return agentRadius;
        }
    }

    Vector2 sampleOffset = new Vector2(0.5f, 0.5f);
    float agentRadius = 0.4f;
    Vector2 agentDirection = new Vector2(1, 0);
    Vector2 agentPosition;
    float agentSpeed = 1f;
    Vector2 agentVelocity = new Vector2(0, 0);
    float agentTurnRate = 0.15f; //0.5 too high  0.05 too low 0.25 also too high

    Vector3 agentPositionOld;
    public int maxUnmovedCount = 75;
    int curUnmovedCount;

    float deltaTime = 1.0f / 33.0f;

    float fmin = 0.01f, fmax = 500.0f;
    float agentSpeedMod;
    Vector3 startingPosition;
    //public Vector2 oldWayPoint;
    //public Vector2 currentWayPoint;
    //public Vector2 newWayPoint;

    void Start() {
       // universalUnitID = numOfUnits++;
        _grid = FindObjectOfType<Grid>();
        agentSpeedMod = Random.value;
        _groupManager = FindObjectOfType<GroupManager>();
        //agentPosition = startPos = oldWayPoint = currentWayPoint = newWayPoint = GetPosition();
        agentPosition =  GetPosition();
        startingPosition = new Vector3(agentPosition.x, 0.1f, agentPosition.y);
        agentDirection = new Vector2(1, 0);
        agentSpeed = 0.0f;
        //sampleOffset = new Vector2(0.5f, 0.5f);
        //agentDirection.Set( 1, 0 );
        transform.forward = Vector3.Normalize(new Vector3(agentDirection.x, 0.0f, agentDirection.y));
        //agentDirection.Normalize();
        deltaTime = 1.0f / 33.0f;
        StartCoroutine(InitParams());
    }
    //void Update()
    //{
    //    deltaTime = Time.deltaTime;
    //    deltaTime = Time.smoothDeltaTime;
    //    deltaTime = Time.maximumDeltaTime;
    //    deltaTime = Time.fixedDeltaTime;
    //    deltaTime = 1.0f / 33.0f;
    //}

    //void Update()
    //{
    //    fmax = 2.5f + agentSpeedMod * _groupManager.ccnmb[group.groupID].Fmax;
    //}

    private bool _allInitiated = false;
    private IEnumerator InitParams()
    {
        while (true)
        {
            if (
                group == null
                )
                yield return null;
            else
            {
                _allInitiated = true;
                yield break;
            }
        }
    }

    public void TransformUnitPosition(Vector2 newPos, Vector2 newDir)
    {
        transform.forward = (new Vector3(newDir.x, 0.0f, newDir.y)).normalized;
        transform.position = new Vector3(newPos.x, 0.1f, newPos.y);
    }

    public void MoveUnit() {
        if (_allInitiated)
        {
            StopCoroutine(Move());
            StartCoroutine(Move());
        }
    }

    
    private IEnumerator Move()
    {
        PotentialField potentialField = group.ccnmb.potentialField;

        Vector2 position = agentPosition;
        Vector2 direction = agentDirection;

        fmax = 4f + agentSpeedMod * (group.ccnmb.Fmax - 4f);

        bool isInsideArea = (position.x >= 0.0f) && (position.y >= 0.0f)
            && (position.x < _grid.gridSizeX) && (position.y < _grid.gridSizeY);

        if (isInsideArea)
        {
            Vector2 gridPosition = RoundVector2ElementsToInt(position);

            bool isInsideWall = float.IsInfinity(potentialField[gridPosition]);

            // If agent spawned (or ended up in) wall, which should never happen

            if (isInsideWall)
            {
                // Park it
                isParked = true;
                //transform.position = new Vector3(-50.0f, 0, -50.0f);
                AgentPosition = parkPosition;
                yield break;
            }
            else
            {
                float sightRangeModifier = 0.0f;
                if ( float.IsInfinity(potentialField[(int)(position.x - 0.1f), (int)(position.y)])
                    || float.IsInfinity(potentialField[(int)(position.x + 0.1f), (int)(position.y)])
                    || float.IsInfinity(potentialField[(int)(position.x), (int)(position.y - 0.1f)])
                    || float.IsInfinity(potentialField[(int)(position.x), (int)(position.y + 0.1f)])
                    || float.IsInfinity(potentialField[(int)(position.x - 0.1f), (int)(position.y - 0.1f)])
                    || float.IsInfinity(potentialField[(int)(position.x + 0.1f), (int)(position.y - 0.1f)])
                    || float.IsInfinity(potentialField[(int)(position.x - 0.1f), (int)(position.y + 0.1f)])
                    || float.IsInfinity(potentialField[(int)(position.x + 0.1f), (int)(position.y + 0.1f)])
                    )
                {
                    sightRangeModifier = 0.2f;
                }

                // Compute offset for gradient lookup so that we don't retrieve a gradient
                // value influenced by agent itself

                Vector2 offset = direction * (agentRadius + sightRangeModifier);

                //
                // Compute sample position
                //

                Vector2 positionWithOffset = position + offset;
                Vector2 gridPositionWithOffset = RoundVector2ElementsToInt(positionWithOffset);

                // Lookup gradient

                Vector2 gradient = GetInterpolatedGradient(positionWithOffset);

                // Check if gradient was sampled inside a wall

                isInsideWall = float.IsInfinity(gradient.x) || float.IsInfinity(gradient.y)
                || float.IsNaN(gradient.x) || float.IsNaN(gradient.y);
                //if (isInsideWall)
                //{
                //    Debug.Log("damn1");
                //}

                // Check if both components are zero, if so gradient is invalidd, from
                // all infinite potentials, or from goal areas

                bool isInsideInvalidEdge = (gradient.x == 0.0f && gradient.y == 0.0f);
                //if (isInsideInvalidEdge)
                //{
                //    Debug.Log("damn2");
                //}
                //
                // Check if the agent stepped into an exit (0 potential) cell
                //

                float potential = potentialField[gridPositionWithOffset];

                if (potential == 0.0f)
                {
                    // done moving
                    reachedTarget = true;

                    if (_groupManager.ImmediateRestart)
                    {
                        //transform.position = startingPosition;
                        ResetUnitToStartPosition();
                    }
                    else
                    {
                        isParked = true;
                        AgentPosition = parkPosition;
                        //TransformUnitPosition(parkPosition, new Vector2(1f, 0f));
                        //transform.position = new Vector3(-1000f, 0, -1000f);
                    }
                    yield break;
                }
                else
                {
                    //
                    // Update agent position, speed and orientation
                    //

                    bool isFacingWall = isInsideWall || isInsideInvalidEdge;

                    //
                    // We are only interested in the direction of the gradient.
                    // The new direction points opposite of the gradient.
                    // The agent moves against the gradient towards the goal.
                    //

                    Vector2 targetDirection = -gradient.normalized;

                    // If hitting wall, set new direction to negative direction so that
                    // we turn around

                    targetDirection = isFacingWall ? -direction : targetDirection;


                    float dotProduct = Vector2.Dot(targetDirection, direction);

                    // Get the turn rate for current situation

                    float turnRate = GetTurnRate(agentTurnRate, dotProduct, isFacingWall);


                    // Let agent turn towards new direction

                    Vector2 newDirection = TurnToDirection(direction, targetDirection
                        , dotProduct, turnRate);



                    float speed = 0.0f;

                    // if the agent hit a wall, we want it to turn in place, else
                    // compute speed based on speed field

                    if (!isFacingWall)
                    {
                        //
                        // Retrieve speed based on sample position
                        // and interpolate from the nearest general directions (N, W, S, W)
                        //

                        Vector4 anisotropicSpeeds = group.ccnmb.speedField[gridPositionWithOffset];

                        // Pythagoras tells us that for unit vector x * x + y * y = 1.
                        // We can use that to retrieve speed components.

                        float xComponentFactor = newDirection.x * newDirection.x;
                        float yComponentFactor = newDirection.y * newDirection.y;

                        bool isGreaterThanEqualX = newDirection.x >= 0;
                        bool isGreaterThanEqualY = newDirection.y >= 0;

                        speed = isGreaterThanEqualX ? speed + xComponentFactor * anisotropicSpeeds[0] : speed; //east
                        speed = !isGreaterThanEqualX ? speed + xComponentFactor * anisotropicSpeeds[2] : speed; //west
                        speed = isGreaterThanEqualY ? speed + yComponentFactor * anisotropicSpeeds[1] : speed; //north
                        speed = !isGreaterThanEqualY ? speed + yComponentFactor * anisotropicSpeeds[3] : speed; //south

                    }

                    //
                    // Limit speed to maximum speed for this agent
                    //

                    speed = Mathf.Min(speed, fmax);

                    //
                    // Update position and velocity
                    //

                    Vector2 newVelocity = newDirection * speed;
                    Vector2 newPosition = position + newVelocity * deltaTime;

                    // Check if new position is inside a wall

                    Vector2 newGridPosition = RoundVector2ElementsToInt(newPosition);
                    bool isNewPositionInsideWall = float.IsInfinity(potentialField[newGridPosition]);

                    // If new Position wound put agent inside wall, stay put, don't move

                    newPosition = isNewPositionInsideWall ? position : newPosition;
                    speed = isNewPositionInsideWall ? 0.0f : speed;
                    //if (isNewPositionInsideWall)
                    //{
                    //    Debug.Log("damn3");
                    //}
                    //
                    // Write new position, direction and speed
                    //

                    //if (newPosition == agentPosition)
                    //{
                    //    curUnmovedCount++;
                    //    if (curUnmovedCount == maxUnmovedCount)
                    //    {
                    //        isParked = true;
                    //        newPosition = parkPosition;
                    //        speed = 0f;
                    //    }
                    //}
                    //else
                    //{
                    //    curUnmovedCount = 0;
                    //}

                    agentDirection = newDirection.normalized;
                    agentSpeed = speed;
                    agentPosition = newPosition;

                    //TransformUnitPosition(newPosition, newDirection);
                    //transform.forward = (new Vector3(newDirection.x, 0, newDirection.y)).normalized;
                    //transform.position = new Vector3(newPosition.x, 0.0f, newPosition.y);
                }
            }
        }
        else
        {
            //
            // Deal with parked agents
            //
            if (_groupManager.ImmediateRestart)
            {
                ResetUnitToStartPosition();
            }
            else
            {
                isParked = true;
                AgentPosition = parkPosition;
                //TransformUnitPosition(parkPosition, new Vector2(1f, 0f));
                //transform.position = new Vector3(-50.0f, 0, -50.0f);
            }
            yield break;
        }

        yield break;
    }

    private Vector2 TurnToDirection(Vector2 direction, Vector2 targetDirection
        , float dotProduct, float turnRate)
    {
        //
        // Slowly turn towards new direction
        //

        Vector2 newDirection;

        // If new direction is directly opposite

        if (dotProduct < -0.8f)
        {
            // Compute a direction perpendicular to the current direction. Then Compute
            // the offset vector from the current direction to the perpendicular one.
            // Use part of this offset direction to compute a new direction turned a bit
            // towards the perpendicular direction

            // Turn towards the left
            Vector2 orthogonalDirection = new Vector2(-direction.y, direction.x);

            //
            // Check if we need to reverse orthogonalDirection in case we rotate into a wall
            // This is about 13 seconds faster on original test scene
            //--------------------------
            if (_groupManager.IsCheckingWhereToTurn)
            {
                Vector2 offset = orthogonalDirection * agentRadius;
                Vector2 positionWithOffset = agentPosition + offset;
                Vector2 gradient = GetInterpolatedGradient(positionWithOffset);

                bool isInsideWall = float.IsInfinity(gradient.x) || float.IsInfinity(gradient.y)
                    || float.IsNaN(gradient.x) || float.IsNaN(gradient.y);
                bool isInsideInvalidEdge = (gradient.x == 0.0f && gradient.y == 0.0f);

                if (isInsideWall || isInsideInvalidEdge)
                {
                    orthogonalDirection = -orthogonalDirection;
                }
            }
            
            //--------------------------

            newDirection = (direction + (orthogonalDirection - direction).normalized * turnRate).normalized;
        }
        else
        {
            Vector2 directionOffset = targetDirection - direction;
            float directionOffsetLength = directionOffset.magnitude;

            //
            // Ensure that the agent does not turn faster than the turn rate allows.
            //
            // Covers against zero directionOffsetLength if turnRate is zero or greater.
            //

            if (directionOffsetLength > turnRate)
            {
                directionOffset /= directionOffsetLength;

                newDirection = (direction + directionOffset * turnRate).normalized;
            }
            else
            {
                newDirection = targetDirection;
            }
        }

        return newDirection;
    }

    private float GetTurnRate(float turnRate, float dotProduct, bool isFacingWall)
    {
        //
        // Choose faster turn rate if we are facing wall or trying to turn around
        //

        bool isTurningAround = dotProduct < 0.6f;

        bool isTurningFast = isFacingWall || isTurningAround;

        turnRate = isTurningFast ? turnRate * 4.0f : turnRate;

        return turnRate;
    }

    private Vector2 RoundVector2ElementsToInt(Vector2 vect)
    {
        return new Vector2((int)vect.x, (int)vect.y);
    }


    public Vector2 QuadLerp (Vector2 lowerLeft, Vector2 lowerRight, Vector2 upperLeft, Vector2 upperRight
           , float distX, float distY)
    {
        Vector2 interpVect;
        float interpVectX, interpVectY;
        interpVectX = ((1 - distY) * ((1 - distX) * lowerLeft.x + distX * lowerRight.x))
                               + (distY * ((1 - distX) * upperLeft.x + distX * upperRight.x));
        interpVectY = ((1 - distY) * ((1 - distX) * lowerLeft.y + distX * lowerRight.y))
                                + (distY * ((1 - distX) * upperLeft.y + distX * upperRight.y));

        interpVect = new Vector2(interpVectX, interpVectY);

        return interpVect;
    }

    private Vector2 GetInterpolatedGradient(Vector2 gridPos)
    {
        float nodeRadius = _grid.nodeRadius;
        GradientField gradientField = group.ccnmb.gradientField;

        int closestNodeX = (int)(gridPos.x - nodeRadius);
        int closestNodeY = (int)(gridPos.y - nodeRadius);

        Vector2 distFromClosestNode = new Vector2(gridPos.x - (closestNodeX + nodeRadius), gridPos.y - (closestNodeY + nodeRadius));

        float distX = gridPos.x - (closestNodeX + agentRadius);
        float distY = gridPos.y - (closestNodeY + agentRadius);

        Vector2 gradLowerLeft = gradientField[closestNodeX, closestNodeY];
        Vector2 gradLowerRight = gradientField[closestNodeX + 1, closestNodeY];
        Vector2 gradUpperLeft = gradientField[closestNodeX, closestNodeY + 1];
        Vector2 gradUpperRight = gradientField[closestNodeX + 1, closestNodeY + 1];

        return QuadLerp(gradLowerLeft, gradLowerRight, gradUpperLeft, gradUpperRight, distX, distY);
    }

    private float interpolateBetweenValues() {
        //int bottomLeftX = (int)Mathf.Floor(transform.position.x);
        //int bottomLeftY = (int)Mathf.Floor(transform.position.z);
        Node curNode = _grid.NodeFromWorldPoint(transform.position);
        int gridPosX = curNode.gridX;
        int gridPosY = curNode.gridY;
        Vector3 bottomLeft = _grid.NodeFromWorldPoint(transform.position).worldPosition;
        //bottomLeft.x -= grid.nodeRadius;
        //bottomLeft.z -= grid.nodeRadius;

        float xAmountRight = transform.position.x - bottomLeft.x;
        float xAmountLeft = _grid.nodeRadius * 2 - Mathf.Abs(xAmountRight);
        float yAmountTop = transform.position.z - bottomLeft.z;
        float yAmountBottom = _grid.nodeRadius * 2 - Mathf.Abs(yAmountTop);

        /// interpolate flow in horizontal direction
        float avgXTop, avgXBottom, avgXTotal;
        if (xAmountRight != 0) {
            // average of 4 vectors, basically
            avgXBottom = _grid[gridPosX, gridPosY].fCost * xAmountLeft / (_grid.nodeRadius * 2)
                + _grid[gridPosX + (int)Mathf.Sign(xAmountRight), gridPosY].fCost * Mathf.Abs(xAmountRight) / (_grid.nodeRadius * 2);
            avgXTop = _grid[gridPosX, gridPosY + (int)Mathf.Sign(yAmountTop)].fCost * xAmountLeft / (_grid.nodeRadius * 2)
                + _grid[gridPosX + (int)Mathf.Sign(xAmountRight), gridPosY + (int)Mathf.Sign(yAmountTop)].fCost * xAmountRight / (_grid.nodeRadius * 2);
            avgXTotal = avgXTop * Mathf.Abs(yAmountTop) / (_grid.nodeRadius * 2) + avgXBottom * yAmountBottom / (_grid.nodeRadius * 2);
        }
        else
            avgXTotal = _grid[gridPosX, gridPosY].fCost;


        return avgXTotal;
        ;
    }


    public Vector2 GetPosition() {
        Vector2 retVec;
        retVec.x = transform.position.x - _grid.worldBottomLeft.x;
        retVec.y = transform.position.z - _grid.worldBottomLeft.z;
        return retVec;
    }

    public void GetPosition(out Vector2 gridPos) {
        gridPos.x = transform.position.x - (_grid.worldBottomLeft.x);
        gridPos.y = transform.position.z - (_grid.worldBottomLeft.z);
    }

    public Vector2 GetAgentVelocity()
    {
        return agentDirection * agentSpeed;
    }

    private Vector2 FindClosestWalkablwCell(float x, float y)
    {
        Vector2 retVal;
        int retX = (int)x;
        int retY = (int)y;

        if (_grid[retX, retY].isWalkable)
        {
            retVal.x = retX;
            retVal.y = retY;
            return retVal;
        }

        float right = 1f - (x - (int)x);
        float left = 1f - right;
        float up = 1f - (y - (int)y);
        float down = 1f - up;

        if (_grid[retX, retY + 1].isWalkable)
        {
            if (up <= right && up <= left && up <= down)
            {
                retVal.x = retX;
                retVal.y = retY + 1;
                return retVal;
            }
        }
        if (_grid[retX, retY - 1].isWalkable)
        {
            if (down <= right && down <= left && down <= up)
            {
                retVal.x = retX;
                retVal.y = retY - 1;
                return retVal;
            }
        }
        if (_grid[retX + 1, retY].isWalkable)
        {
            if (left <= right && left <= up && left <= down)
            {
                retVal.x = retX + 1;
                retVal.y = retY;
                return retVal;
            }
        }
        if (_grid[retX - 1, retY].isWalkable)
        {
            if (right <= left && right <= up && right <= down)
            {
                retVal.x = retX - 1;
                retVal.y = retY;
                return retVal;
            }
        }

        retVal.x = retX;
        retVal.y = retY;
        return retVal;
    }

    public Vector2 GetLinearPredictivePosition(int stepsInTheFuture)
    {
        Vector2 curPos = agentPosition;
        Vector2 futurePos = curPos + stepsInTheFuture * deltaTime * GetAgentVelocity();
        return futurePos;
    }

    public void GetLinearPredictivePosition (int stepsInTheFuture, out Vector2 futurePos)
    {
        Vector2 curPos = GetPosition();// agentPosition;
        futurePos = curPos + stepsInTheFuture * deltaTime * GetAgentVelocity();
    }

    public void ResetUnitToStartPosition()
    {
        isParked = false;
        transform.position = startingPosition;
        agentPosition = GetPosition();
        agentDirection = new Vector2(1, 0);
        agentSpeed = 0.0f;
        //sampleOffset = new Vector2(0.5f, 0.5f);
        //agentDirection.Set( 1, 0 );
        transform.forward = Vector3.Normalize(new Vector3(agentDirection.x, 0.0f, agentDirection.y));
        //agentDirection.Normalize();
    }

    public void UpdateTransformPosition()
    {
        transform.forward = (new Vector3(agentDirection.x, 0, agentDirection.y)).normalized;
        transform.position = new Vector3(agentPosition.x, 0.1f, agentPosition.y);
    }

}
