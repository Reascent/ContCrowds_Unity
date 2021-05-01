using UnityEngine;
using System.Collections;

public class PotentialField {

    float[] _potentialField;
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

    public PotentialField (int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        _potentialField = new float[sizeX * sizeY];

        for (int i = 0 ; i < gridSizeX * gridSizeY ; i++)
        {
            _potentialField[i] = Mathf.Infinity;
        }
    }

    public float this[int pos]
    {
        get
        {
            return _potentialField[pos];
        }
        set
        {
            _potentialField[pos] = value;
        }
    }

    float maximumPotential = 0;
    public float GetMaxPotential()
    {
        return maximumPotential;
    }

    public float this[int posX, int posY]
    {
        get
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
                return _potentialField[posX + posY * gridSizeX];
            else
                return Mathf.Infinity;
        }
        set
        {
            if (!(posX >= gridSizeX || posX < 0 || posY >= gridSizeY || posY < 0))
            {
                _potentialField[posX + posY * gridSizeX] = value;
                if (maximumPotential < value && value != Mathf.Infinity)
                    maximumPotential = value;
            }
        }
    }

    public float this[Vector2 pos]
    {
        get {
            return this[(int)pos.x, (int)pos.y];
        }
        set
        {
            this[(int)pos.x, (int)pos.y] = value;
        }
    }

    public void ResetMaxPotential ()
    {
        maximumPotential = 0.0f;
    }

}
