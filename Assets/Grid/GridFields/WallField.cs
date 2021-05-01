using UnityEngine;
using System.Collections;

public class WallField {

    bool[] _wallField;
    int _gridSizeX;
    int _gridSizeY;

    public int gridSizeX
    {
        get
        {
            return _gridSizeX;
        }
        private set
        {
            _gridSizeX = value;
        }
    }
    public int gridSizeY
    {
        get
        {
            return _gridSizeY;
        }
        private set
        {
            _gridSizeY = value;
        }
    }

    public WallField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _wallField = new bool[sizeX * sizeY];

        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _wallField[i] = false;
        }
    }

    public bool this[int pos]
    {
        get
        {
            return _wallField[pos];
        }
        set
        {
            _wallField[pos] = value;
        }
    }

    public bool this[int posX, int posY]
    {
        get
        {
            return _wallField[posX + posY * gridSizeX];
        }
        set
        {
            _wallField[posX + posY * gridSizeX] = value;
        }
    }

}
