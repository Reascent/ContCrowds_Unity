using UnityEngine;
using System.Collections;

public class GradientField {

    Vector2[] _gradientField;
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

    public GradientField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _gradientField = new Vector2[sizeX * sizeY];
        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _gradientField[i] = new Vector2(0f, 0f);
        }
    }

    public Vector2 this[int pos]
    {
        get
        {
            return _gradientField[pos];
        }
        set
        {
            _gradientField[pos] = value;
        }
    }

    public Vector2 this[int posX, int posY]
    {
        get
        {
            if (!(posX < 0 || posX >= gridSizeX || posY < 0 || posY >= gridSizeY))
                return _gradientField[posX + posY * gridSizeX];
            else
                return new Vector2(Mathf.Infinity, Mathf.Infinity);
        }
        set
        {
            _gradientField[posX + posY * gridSizeX] = value;
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
