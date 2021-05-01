using UnityEngine;
using System.Collections;

public class CostField  {

    Vector4[] _costField;
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

    public CostField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _costField = new Vector4[sizeX * sizeY];

        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _costField[i] = new Vector4(float.PositiveInfinity, 
                                        float.PositiveInfinity, 
                                        float.PositiveInfinity,
                                        float.PositiveInfinity);
        }
    }

    public Vector4 this[int pos]
    {
        get
        {
            return _costField[pos];
        }
        set
        {
            _costField[pos] = value;
        }
    }

    public Vector4 this[int posX, int posY]
    {
        get
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
                return _costField[posX + posY * gridSizeX];
            else
                return new Vector4(float.PositiveInfinity,
                                        float.PositiveInfinity,
                                        float.PositiveInfinity,
                                        float.PositiveInfinity);
        }
        set
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
            {
                _costField[posX + posY * gridSizeX] = value;
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
