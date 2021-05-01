using UnityEngine;
using System.Collections;

public class VelocitySumField {

    Vector2[] _velocitySumField;
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

    public VelocitySumField(int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _velocitySumField = new Vector2[sizeX * sizeY];
        
        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _velocitySumField[i] = new Vector2(0.0f, 0.0f);
        }
    }

    public Vector2 this[int pos]
    {
        get
        {
            //Debug.Log(pos);
            return _velocitySumField[pos];
        }
        set
        {
            _velocitySumField[pos] = value;
        }
    }

    public Vector2 this[int posX, int posY]
    {
        get
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
                return _velocitySumField[posX + posY * gridSizeX];
            else
                return new Vector2(0.0f, 0.0f);
        }
        set
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
            {
                _velocitySumField[posX + posY * gridSizeX] = value;
            }
        }
    }

    public Vector2 this[Vector2 pos]
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
