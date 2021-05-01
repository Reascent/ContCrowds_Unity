using UnityEngine;
using System.Collections;

public class DiscomfortField  {

    float[] _discomfortField;
    float[] _discomfortSumField;
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

    public DiscomfortField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _discomfortField = new float[sizeX * sizeY];
        _discomfortSumField = new float[sizeX * sizeY];

        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _discomfortField[i] = 0.0f;
            _discomfortSumField[i] = 0.0f;
        }
    }

    public float this[int pos]
    {
        get
        {
            return _discomfortSumField[pos];
        }
        set
        {
            _discomfortSumField[pos] = value;
        }
    }

    public float this[int x, int y]
    {
        get
        {
            if (!(x >= gridSizeX || x < 0 || y >= gridSizeY || y < 0))
                return _discomfortSumField[x + y * gridSizeX];
            else
                return Mathf.Infinity;
        }
        set
        {
            if (!(x >= gridSizeX || x < 0 || y >= gridSizeY || y < 0))
            {
                if (value >= 0)
                    _discomfortSumField[x + y * gridSizeX] = value;
                else
                    _discomfortSumField[x + y * gridSizeX] = 0.0f;
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

    public void SetBaseDiscomfort (int posX, int posY, float value)
    {
        if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
        {
            _discomfortField[posX + posY * gridSizeX] = value;
            _discomfortSumField[posX + posY * gridSizeX] = value;
        }
        else
        {
            _discomfortField[posX + posY * gridSizeX] = Mathf.Infinity;
            _discomfortSumField[posX + posY * gridSizeX] = Mathf.Infinity;
        }
    }
    public void SetBaseDiscomfort (int pos, float value)
    {
        if (!(pos < 0 || pos >= gridSizeX * gridSizeY))
        {
            _discomfortField[pos] = value;
            _discomfortSumField[pos] = value;
        }
        else
        {
            _discomfortField[pos]    = Mathf.Infinity;
            _discomfortSumField[pos] = Mathf.Infinity;
        }
    }

    public void ResetNode (int posX, int posY)
    {
        this[posX + posY * gridSizeX] = _discomfortField[posX + posY * gridSizeX];
    }
    public void ResetNode (int pos)
    {
        this[pos] = _discomfortField[pos];
    }
}
