using UnityEngine;
using System.Collections;

public class DensityField {

    float[] _densityField;
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

    public DensityField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _densityField = new float[sizeX * sizeY];

        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _densityField[i] = 0f;
        }
    }

    public float this[int pos]
    {
        get
        {
            return _densityField[pos];
        }
        set
        {
            _densityField[pos] = value;
        }
    }

    public float this[int posX, int posY]
    {
        get
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
                return _densityField[posX + posY * gridSizeX];
            else
                return Mathf.Infinity;
        }
        set
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
            {
                _densityField[posX + posY * gridSizeX] = value;
            }
        }
    }

    public float this[Vector2 pos]
    {
        get
        {
            return this[(int)pos.x, (int)pos.y];
        }
        set
        {
            this[(int)pos.x, (int)pos.y] = value;
        }
    }

}
