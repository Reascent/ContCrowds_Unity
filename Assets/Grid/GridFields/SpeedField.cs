using UnityEngine;
using System.Collections;

public class SpeedField {

    Vector4[] _speedField;
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

    public SpeedField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _speedField = new Vector4[sizeX * sizeY];

        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _speedField[i] = new Vector4(0, 0, 0, 0);
        }
    }

    public Vector4 this[int pos]
    {
        get
        {
            return _speedField[pos];
        }
        set
        {
            _speedField[pos] = value;
        }
    }

    public Vector4 this[int posX, int posY]
    {
        get
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
                return _speedField[posX + posY * gridSizeX];
            else
                return new Vector4(0, 0, 0, 0);
        }
        set
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
            {
                _speedField[posX + posY * gridSizeX] = value;
            }
        }
    }

    public Vector4 this[Vector2 pos]
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
