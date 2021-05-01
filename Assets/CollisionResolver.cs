using UnityEngine;
using System.Collections;

public class CollisionResolver
{

    const float INTERVAL_SIZE = 1.0f;
    readonly Vector2 ZERO_OFFSET = Vector2.zero;
    const int BUCKET_ENTRIES = 4;

    int _bucketsCount;
    Unit[] buckets;

    int[] agentCounts;

    int gridSizeX, gridSizeY;

    GroupManager _groupManager;

    Random.State oldState;

    public int BucketsCount
    {
        get
        {
            return _bucketsCount;
        }

        set
        {
            _bucketsCount = value;
        }
    }

    public GroupManager groupManager
    {
        get
        {
            return _groupManager;
        }

        set
        {
            _groupManager = value;
        }
    }

    public CollisionResolver(int bucketsCount, int gridSizeX, int gridSizeY, GroupManager groupManager)
    {
        BucketsCount = bucketsCount;
        buckets = new Unit[BucketsCount * BUCKET_ENTRIES];

        agentCounts = new int[BucketsCount];

        for (var i = 0 ; i < agentCounts.Length ; i++)
        {
            agentCounts[i] = 0;
        }

        this.gridSizeX = gridSizeX;
        this.gridSizeY = gridSizeY;

        this.groupManager = groupManager;

        Random.InitState(Random.Range(1, 100));
        oldState = Random.state;
    }

    public void ResetBucketCounts()
    {
        for (var i = 0 ; i < agentCounts.Length ; i++)
        {
            agentCounts[i] = 0;
        }
        Random.state = oldState;
    }

    public void SortAgentsIntoBuckets (Unit unit)
    {
        Vector2 agentPosition = unit.AgentPosition;

        if (!unit.isParked)
        {
            // Compute cell the agent resides in

            int agentCellX = (int)agentPosition.x;
            int agentCellY = (int)agentPosition.y;

            int agentCell = agentCellX + agentCellY * gridSizeX;

            int bucket = agentCell * BUCKET_ENTRIES;

            // Increment agent counter for this cell and retrieve old count
            int oldCount = agentCounts[agentCell]++;

            // If the bucket is not full
            if (oldCount < BUCKET_ENTRIES)
            {
                // Write the agent's index into the bucket

                buckets[bucket + oldCount] = unit;
            }
        }
    }

   public void ResolveCollisions (Unit unit)
    {
        Vector2 agentPosition = unit.AgentPosition;
        UnitGroup group = unit.group;

        if (!unit.isParked)
        {
            float radius = unit.AgentRadius;

            float maximumDiameter = radius + radius;
            float maximumDiameterSquared = maximumDiameter * maximumDiameter;

            Vector2 offset = ZERO_OFFSET;


            // Process rectangular cell area around agent cell

            int agentCellX = (int)agentPosition.x;
            int agentCellY = (int)agentPosition.y;

            int additionalCellCount = Mathf.CeilToInt(maximumDiameter / INTERVAL_SIZE);

            // Bounds are inclusive

            int lowerBoundX = Mathf.Max(agentCellX - additionalCellCount, 0);
            int upperBoundX = Mathf.Min(agentCellX + additionalCellCount, gridSizeX - 1);

            int lowerBoundY = Mathf.Max(agentCellY - additionalCellCount, 0);
            int upperBoundY = Mathf.Min(agentCellY + additionalCellCount, gridSizeY - 1);

            // Iterate over buckets around agent's bucket and resolve collisions

            for (var y = lowerBoundY ; y <= upperBoundY ; y++)
            {
                for (var x = lowerBoundX ; x <= upperBoundX ; x++)
                {
                    int cell = y * gridSizeX + x;
                    int bucketIndex = cell * BUCKET_ENTRIES;

                    int agentCount = Mathf.Min(agentCounts[cell], BUCKET_ENTRIES);

                    for (var i = 0 ; i < agentCount ; i++)
                    {
                        Unit otherAgent = buckets[bucketIndex + i];

                        // If this is not our own entry in the bucket

                        if (!Object.ReferenceEquals(unit, otherAgent))
                        {
                            Vector2 otherAgentPosition = otherAgent.AgentPosition;
                            Vector2 difference = agentPosition - otherAgentPosition;

                            bool isOneNotZero = (difference.x != 0.0f) || (difference.y != 0.0f);

                            // If agents not in the same spot

                            if (isOneNotZero)
                            {
                                float distanceSquared = difference.x * difference.x + difference.y * difference.y;

                                if (distanceSquared < maximumDiameterSquared)
                                {
                                    float radiiSum = otherAgent.AgentRadius + radius;

                                    float radiiSumSquared = radiiSum * radiiSum;

                                    if (distanceSquared < radiiSumSquared)
                                    {
                                        float distance = Mathf.Sqrt(distanceSquared);

                                        Vector2 direction = difference / distance;

                                        // Check if the agent is about to be pushed into a wall by this agent

                                        float offsetDistance = (radiiSum - distance) * 0.5f;

                                        Vector2 samplePosition = agentPosition + direction * (offsetDistance + radius);
                                        int gridPosX = (int)samplePosition.x;
                                        int gridPosY = (int)samplePosition.y;

                                        float potential = group.ccnmb.potentialField[gridPosX, gridPosY];

                                        bool canPushForThisAgent = !float.IsInfinity(potential);

                                        Vector2 agentOffset = direction * offsetDistance;

                                        // Check if offset sum for all involved agents would put agent into wall

                                        Vector2 tempOffset = offset + agentOffset;

                                        gridPosX = (int)(agentPosition.x + tempOffset.x);
                                        gridPosY = (int)(agentPosition.y + tempOffset.y);

                                        potential = group.ccnmb.potentialField[gridPosX, gridPosY];

                                        bool canPushForAllAgents = !float.IsInfinity(potential);

                                        bool canSeparate = canPushForThisAgent && canPushForAllAgents;

                                        /* 
                                         * Compute and add offset to agent to resolve collision on our side.
                                         * The other agent will do this itself.
                                         */

                                        offset = canSeparate ? offset + agentOffset : offset;
                                    }
                                }
                            }
                            else
                            {
                                /*
                                 * Resolve agents in the same spot
                                 */

                                Vector2 direction = Random.insideUnitCircle.normalized;

                                /* 
                                 * Compute and override offset of agent to resolve collision on our side.
                                 */

                                offset = direction * radius;
                            }
                        }
                    }
                }
            }

            // Apply offset to agent
            unit.AgentPosition += offset;
        }
    }

    public void RunCollisionResolver()
    {

    }

}