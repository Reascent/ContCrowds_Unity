using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(TileMap))]
public class TileMapMouse : MonoBehaviour {

    TileMap _tileMap;
    Grid _grid;
    GroupManager _groupManager = null;

    public bool outputMouseCoords = false;
    
    private Vector3 currentTileCoord, olddttile;
    private Vector3 startingPos;
    private Vector3 selectionBoxMin;
    private Vector3 selectionBoxMax;
    private Vector3 stdSize;
    private Vector3 stdPos;

    private Vector2 currentTile;
    private Vector2 oldTile;

    public Transform selectionCube;
    public Transform selectionRectangle;

    //Vector3 bottomLeftTilePos;
    private Vector3 relativeTilemapPos;

    void Awake() {
        _tileMap = GetComponent<TileMap>();
        _grid = FindObjectOfType<Grid>();
        currentTile = new Vector2(0f, 0f);
        oldTile = new Vector2(0f, 0f);
        relativeTilemapPos = _tileMap.transform.position;
        olddttile = startingPos = new Vector3(0f, 0f, 0f);
        selectionBoxMin = new Vector3(0f, 0f, 0f);
        selectionBoxMax = new Vector3(0f, 0f, 0f);
        stdSize = new Vector3(1f, 1f, 1f);
        stdPos = new Vector3(0f, 0f, 0f);
        //Invoke("LoadInfoVars", 1);
        StartCoroutine(LoadInfoVars());
        //grid = GetComponent<Grid>();
        //selectionCube.transform.localScale.Set(_tileMap.tileSize, 1.0f, _tileMap.tileSize);

    }

    bool _canAddAgentsManually;
    public bool canAddAgentsManually
    {
        get
        {
            return _canAddAgentsManually;
        }
        set
        {
            EditState = EditStates.ADDAGENTMANUALLY;
            _canAddAgentsManually = value;
            //EditState = _canAddAgentsManually ? EditStates.ADDAGENTMANUALLY : EditState;
        }
    }

    bool _isEditingEnabled = false;
    public bool isEditingEnabled
    {
        get
        {
            return _isEditingEnabled;
        }
        set
        {
            _isEditingEnabled = value;
        }
    }

    public void UIToggleIsEditingEnabled()
    {
        isEditingEnabled = !isEditingEnabled;
    }

    Text uiPointerInfo;
    InputField agentCountField;

    InputField discomfortBrushField;
    float discomfortBrushVal = 0.0f;
    public float DiscomfortBrushVal
    {
        get
        {
            return discomfortBrushVal;
        }

        set
        {
            discomfortBrushVal = value;
        }
    }

    public enum EditStates
    {
        NONE,
        ADDWALLS,
        REMOVEWALLS,
        ADDSTARTNODES,
        REMOVESTARTNODES,
        ADDENDNODES,
        REMOVEENDNODES,
        EDITDISCOMFORT,
        ADDAGENTMANUALLY,
    }

    private EditStates _editState;
    public EditStates EditState
    {
        get
        {
            return _editState;
        }

        set
        {
            _editState = value;
        }
    }

    int uiGroupNum = 0;
    public int UiGroupNum
    {
        get
        {
            return uiGroupNum;
        }

        set
        {
            uiGroupNum = value;
        }
    }



    void Update () {
        Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (GetComponent<Collider>().Raycast(ray, out hitInfo, Mathf.Infinity)) {
            currentTileCoord.x = Mathf.FloorToInt(hitInfo.point.x / _tileMap.tileSize);
            currentTileCoord.y = Mathf.FloorToInt(hitInfo.point.y / _tileMap.tileSize);
            currentTileCoord.z = Mathf.FloorToInt(hitInfo.point.z / _tileMap.tileSize);

            if (_groupManager != null)
                uiPointerInfo.text = ("coords: (" + hitInfo.point.x + ", " + hitInfo.point.z + ")"
                    + "\n"
                    + "group 0"
                    + " | discomfort: " + _groupManager.groups[0].ccnmb.discomfortField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | density: " + _groupManager.groups[0].ccnmb.densityField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | avg velocity: " + _groupManager.groups[0].ccnmb.velocitySumField[(int)currentTileCoord.x, (int)currentTileCoord.z] / (_groupManager.groups[0].ccnmb.densityField[(int)currentTileCoord.x, (int)currentTileCoord.z] == 0 ? 1 : _groupManager.groups[0].ccnmb.densityField[(int)currentTileCoord.x, (int)currentTileCoord.z])
                    + " | speed: " + _groupManager.groups[0].ccnmb.speedField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | cost: " + _groupManager.groups[0].ccnmb.costField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | potential: " + _groupManager.groups[0].ccnmb.potentialField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | gradient: (" + _groupManager.groups[0].ccnmb.gradientField[(int)currentTileCoord.x, (int)currentTileCoord.z].x + ", " + -_groupManager.groups[0].ccnmb.gradientField[(int)currentTileCoord.x, (int)currentTileCoord.z].y + ")"
                    + " | bilerp grad: " + Bilerp(new Vector2(hitInfo.point.x / _tileMap.tileSize, hitInfo.point.z / _tileMap.tileSize), 0)
                    + "\n"
                    + "group 1"
                    + " | discomfort: " + _groupManager.groups[1].ccnmb.discomfortField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | density: " + _groupManager.groups[1].ccnmb.densityField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | avg velocity: " + _groupManager.groups[1].ccnmb.velocitySumField[(int)currentTileCoord.x, (int)currentTileCoord.z] / (_groupManager.groups[1].ccnmb.densityField[(int)currentTileCoord.x, (int)currentTileCoord.z] == 0 ? 1 : _groupManager.groups[1].ccnmb.densityField[(int)currentTileCoord.x, (int)currentTileCoord.z])
                    + " | speed: " + _groupManager.groups[1].ccnmb.speedField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | cost: " + _groupManager.groups[1].ccnmb.costField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | potential: " + _groupManager.groups[1].ccnmb.potentialField[(int)currentTileCoord.x, (int)currentTileCoord.z]
                    + " | gradient: (" + _groupManager.groups[1].ccnmb.gradientField[(int)currentTileCoord.x, (int)currentTileCoord.z].x + ", " + -_groupManager.groups[1].ccnmb.gradientField[(int)currentTileCoord.x, (int)currentTileCoord.z].y + ")"
                    + " | bilerp grad: " + Bilerp(new Vector2(hitInfo.point.x / _tileMap.tileSize, hitInfo.point.z / _tileMap.tileSize), 1)
                    );

            TileSelector(hitInfo.point);
        }
        else {
            // hide selection cube
        }
	}

    private void GetMouseDown (Vector2 editTile)
    {     
        switch (EditState)
        {
            case EditStates.NONE:
                break;
            case EditStates.ADDWALLS:
                _grid[(int)editTile.x, (int)editTile.y].MakeWall();
                break;
            case EditStates.REMOVEWALLS:
                if (!((int)editTile.x <= 0 || (int)editTile.x >= _grid.gridSizeX - 1 || (int)editTile.y <= 0 || (int)editTile.y >= _grid.gridSizeX - 1))
                    _grid[(int)editTile.x, (int)editTile.y].MakeWalkable();
                break;
            case EditStates.ADDSTARTNODES:
                if (_grid[(int)editTile.x, (int)editTile.y].isWalkable)
                    _groupManager.groups[uiGroupNum].AddStartingNode(new Vector2((int)editTile.x, (int)editTile.y));
                break;
            case EditStates.REMOVESTARTNODES:
                _groupManager.groups[uiGroupNum].RemoveStartingNode(new Vector2((int)editTile.x, (int)editTile.y));
                break;
            case EditStates.ADDENDNODES:
                if (_grid[(int)editTile.x, (int)editTile.y].isWalkable)
                    _groupManager.groups[uiGroupNum].AddTargetNode(new Vector2((int)editTile.x, (int)editTile.y));
                break;
            case EditStates.REMOVEENDNODES:
                _groupManager.groups[uiGroupNum].RemoveTargetNode(new Vector2((int)editTile.x, (int)editTile.y));
                break;
            case EditStates.EDITDISCOMFORT:
                if (_grid[(int)editTile.x, (int)editTile.y].isWalkable) { 
                    if (DiscomfortBrushVal != 0) { 
                        _groupManager.groups[uiGroupNum].ccnmb.discomfortField.SetBaseDiscomfort(
                            (int)editTile.x,
                            (int)editTile.y,
                            _groupManager.groups[uiGroupNum].ccnmb.discomfortField[
                                (int)editTile.x,
                            (int)editTile.y] + DiscomfortBrushVal
                            );
                    }
                    else { 
                        _groupManager.groups[uiGroupNum].ccnmb.discomfortField.SetBaseDiscomfort(
                            (int)editTile.x,
                            (int)editTile.y,
                            0);
                    }
                }
                else
                {
                    _groupManager.groups[0].ccnmb.discomfortField.SetBaseDiscomfort(
                        (int)editTile.x,
                            (int)editTile.y,
                        float.PositiveInfinity);
                    _groupManager.groups[1].ccnmb.discomfortField.SetBaseDiscomfort(
                        (int)editTile.x,
                            (int)editTile.y,
                        float.PositiveInfinity);
                }
                break;

            default:
                break;
        }
        
    }

    public void EditTiles(int x1, int y1, int x2, int y2)
    {
        if (x1 < 0 || x1 >= _grid.gridSizeX || y2 < 0 || y2 >= _grid.gridSizeY)
            return;

        int firstTile = (x1 + y1 * _grid.gridSizeY); //get first vert of tile
        int numTilesX = x2 - x1;
        int numTilesY = y2 - y1;

        for (int y = 0 ; y < numTilesY ; y++)
        {
            for (int x = 0 ; x < numTilesX ; x++)
            {
                GetMouseDown(new Vector2(x1 + x, y1 + y));
            }
        }
    }

    private void TileSelector(Vector3 hitinfo)
    {
        if (!isEditingEnabled)
        {
            selectionCube.transform.position = new Vector3(-50, 0, 50);
            return;
        }
        selectionCube.transform.position = currentTileCoord * _tileMap.tileSize;
        // Rectangle selection
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            if (Input.GetMouseButton(0)) {
                if (Input.GetMouseButtonDown(0) || (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))) {
                    // if LMB was pressed down on current frame
                    startingPos = currentTileCoord;
                    oldTile = currentTile;
                }
                selectionBoxMin.x = Mathf.Min(startingPos.x, currentTileCoord.x);
                selectionBoxMin.y = 0f;
                selectionBoxMin.z = Mathf.Min(startingPos.z, currentTileCoord.z);
                
                selectionBoxMax.x = Mathf.Max(startingPos.x, currentTileCoord.x) + _tileMap.tileSize;
                selectionBoxMax.y = currentTileCoord.y + 5;
                selectionBoxMax.z = Mathf.Max(startingPos.z, currentTileCoord.z) + _tileMap.tileSize;

                selectionCube.transform.localScale = selectionBoxMax - selectionBoxMin;
                selectionCube.transform.position = selectionBoxMin;
                currentTile.x = Mathf.Min(oldTile.x, currentTile.x);
                currentTile.y = Mathf.Min(oldTile.y, currentTile.y);
            }
        }
        else
        { // single tile selection
            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    oldTile = currentTile;
                }
                selectionBoxMin = selectionBoxMax = currentTileCoord;
                EditTiles((int)(currentTileCoord.x - relativeTilemapPos.x), (int)(currentTileCoord.z - relativeTilemapPos.z),
                    (int)(currentTileCoord.x - relativeTilemapPos.x + 1f), (int)(currentTileCoord.z - relativeTilemapPos.z + 1f));
            }
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            selectionCube.transform.localScale = stdSize;
            EditTiles((int)(selectionBoxMin.x - relativeTilemapPos.x), (int)(selectionBoxMin.z - relativeTilemapPos.z),
                (int)(selectionBoxMax.x - relativeTilemapPos.x), (int)(selectionBoxMax.z - relativeTilemapPos.z));

            if (_canAddAgentsManually && isEditingEnabled)
            {
                AddUnitManually(new Vector2(hitinfo.x, hitinfo.z));
            }
        }
    }


    /// <summary>
    /// return coordinates of selected tile
    /// </summary>
    /// <returns>Vector2 with x,y coordinates on grid</returns>
    public Vector2 GetSelectedTile(Vector3 tilemapCoords) {
        tilemapCoords.x = currentTileCoord.x - relativeTilemapPos.x;
        tilemapCoords.z = currentTileCoord.z - relativeTilemapPos.z;

        return oldTile;
    }

    private IEnumerator LoadInfoVars()
    {
        while (!(_groupManager = FindObjectOfType<GroupManager>())) {
            yield return null;
        }
        SetupUI();
        yield break;
    }

    void SetupUI ()
    {
        InputField[] inputFields;
        inputFields = InputField.FindObjectsOfType<InputField>();

        foreach (InputField inputField in inputFields)
        {
            if (string.Compare(inputField.name, "InputField_DiscomfortBrush") == 0)
            {
                discomfortBrushField = inputField;
            }
            if (string.Compare(inputField.name, "InputField_AgentCount") == 0)
            {
                agentCountField = inputField;
            }
        }

        Text[] texts = Text.FindObjectsOfType<Text>();
        foreach (Text text in texts)
        {
            if (string.Compare(text.name, "Text_PointerInfo") == 0)
            {
                uiPointerInfo = text;
            }
            
        }
    }

    public void SetEditState(int type)
    {
        switch (type)
        {    
            case 1:
                EditState = EditStates.ADDSTARTNODES;
                break;
            case 2:
                EditState = EditStates.REMOVESTARTNODES;
                break;
            case 3:
                EditState = EditStates.ADDENDNODES;
                break;
            case 4:
                EditState = EditStates.REMOVEENDNODES;
                break;
            case 5:
                EditState = EditStates.ADDWALLS;
                break;
            case 6:
                EditState = EditStates.REMOVEWALLS;
                break;
            case 7:
                EditState = EditStates.EDITDISCOMFORT;
                break;
            case 8:
                EditState = EditStates.ADDAGENTMANUALLY;
                break;
            default:
                break;
        }
    }

    public void AddUnitManually(Vector2 pos)
    {
        _groupManager.groups[uiGroupNum].AddUnit(pos);
        agentCountField.text = _groupManager.groups[uiGroupNum].units.Count.ToString();
    }

    public void SetDiscomfortBrushVal()
    {
        if (!float.TryParse(discomfortBrushField.text, out discomfortBrushVal))
        {
            discomfortBrushVal = 0.0f;
            discomfortBrushField.text = (0.0f).ToString();
        }
    }

    /// <summary>
    /// Interpolate gradient at given grid position for given group using bilinear interpolation
    /// </summary>
    /// <param name="gridPos"></param>
    /// <param name="groupNum"></param>
    /// <returns>Interpolated gradient vector</returns>
    private Vector2 Bilerp(Vector2 gridPos, int groupNum)
    {
        float nodeRadius = _grid.nodeRadius;
        GradientField gradientField = _groupManager.groups[groupNum].ccnmb.gradientField;

        int closestNodeX = (int)(gridPos.x - nodeRadius);
        int closestNodeY = (int)(gridPos.y - nodeRadius);

        Vector2 distFromClosestNode = new Vector2(gridPos.x - (closestNodeX + nodeRadius), gridPos.y - (closestNodeY + nodeRadius));

        Vector2 interpolatedGradient = new Vector3(0, 0);

        Vector2 gradLowerLeft = gradientField[closestNodeX, closestNodeY];
        Vector2 gradLowerRight = gradientField[closestNodeX + 1, closestNodeY];
        Vector2 gradUpperLeft = gradientField[closestNodeX, closestNodeY + 1];
        Vector2 gradUpperRight = gradientField[closestNodeX + 1, closestNodeY + 1];

        interpolatedGradient.x = ((1 - distFromClosestNode.y) * ((1 - distFromClosestNode.x) * gradLowerLeft.x
                                    + distFromClosestNode.x * gradLowerRight.x))
                                    + (distFromClosestNode.y * ((1 - distFromClosestNode.x) * gradUpperLeft.x
                                    + distFromClosestNode.x * gradUpperRight.x));
        interpolatedGradient.y = ((1 - distFromClosestNode.y) * ((1 - distFromClosestNode.x) * gradLowerLeft.y
                                    + distFromClosestNode.x * gradLowerRight.y))
                                    + (distFromClosestNode.y * ((1 - distFromClosestNode.x) * gradUpperLeft.y
                                    + distFromClosestNode.x * gradUpperRight.y));

        return interpolatedGradient;
    }
}
