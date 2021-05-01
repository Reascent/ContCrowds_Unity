using UnityEngine;
using System.Collections;
using System;

public class HeapItemFloatKey2Dindex : IHeapItem<HeapItemFloatKey2Dindex>
{
    private int heapIndex;
    private float key;
    //private Vector2 _vector;
    private int _x;
    private int _y;


    public HeapItemFloatKey2Dindex (int x, int y)
    {
        _x = x;
        _y = y;
    }

    public int x
    {
        get
        {
            return _x;
        }
    }
    public int y
    {
        get
        {
            return _y;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public float Key
    {
        get
        {
            return key;
        }

        set
        {
            key = value;
        }
    }

    public int CompareTo (HeapItemFloatKey2Dindex other)
    {
        int compare = key.CompareTo(other.key);
        //if (compare == 0)
        //{
        //    compare = 1;
        //}
        return -compare;
    }

}
