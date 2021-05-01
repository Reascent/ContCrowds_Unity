using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OverlayVectorGrid : MonoBehaviour {

    public Material lineMaterial;

    Texture2D tex; // grid texture
    Color[] pix; //current grid tile colors
    Color[] basePix; //base tile colors to revert instantly if need be
    Gradient potentialGrad;

    float inverseColorChangeRateDiscomfortUpper = 4;
    float inverseColorChangeRateDensityUpper = 4;
    float inverseColorChangeRateSpeedUpper = 128;
    float inverseColorChangeRateCostUpper = 128;
    float inverseColorChangeRatePotentialUpper = 2048;
    float inverseColorChangeRatePotentialWaveUpper = 8;
    float inverseColorChangeRateDiscomfortLower = 0;
    float inverseColorChangeRateDensityLower = 0;
    float inverseColorChangeRateSpeedLower = 0;
    float inverseColorChangeRateCostLower = 0;
    float inverseColorChangeRatePotentialLower = 0;

    public enum DisplayFieldTypes
    {
        NONE,
        DISCOMFORT,
        DENSITY,
        AVG_VELOCITY,
        SPEED,
        COST,
        POTENTIAL,
        POTENTIALWAVES,
        GRADIENT,
    }

    public enum DisplayVectorTypes
    {
        NONE,
        AVG_VELOCITY,
        GRADIENT,
    }

    public enum DisplayAnisoDirectionTypes
    {
        EAST,
        NORTH,
        WEST,
        SOUTH,
    }

    private DisplayFieldTypes _displayFieldType = DisplayFieldTypes.NONE;
    public DisplayFieldTypes displayFieldType
    {
        get
        {
            return _displayFieldType;
        }
        set
        {
            _displayFieldType = value;
        }
    }

    private DisplayVectorTypes _displayVectorType = DisplayVectorTypes.NONE;
    public DisplayVectorTypes displayVectorType
    {
        get
        {
            return _displayVectorType;
        }
        set
        {
            _displayVectorType = value;
        }
    }

    private DisplayAnisoDirectionTypes _displayAnisoDirectionType = DisplayAnisoDirectionTypes.WEST;
    public DisplayAnisoDirectionTypes displayAnisoDirectionType
    {
        get
        {
            return _displayAnisoDirectionType;
        }
        set
        {
            _displayAnisoDirectionType = value;
        }
    }

    bool _displayStartAndEndNodes = false;
    public bool displayStartAndEndNodes {
        get
        {
            return _displayStartAndEndNodes;
        }
        set
        {
            _displayStartAndEndNodes = value;
        }
    }

    public Texture2D densityLegend;
    public Texture2D discomfortLegend;
    public Texture2D avgVelocityLegend;
    public Texture2D speedLegend;
    public Texture2D costLegend;
    public Texture2D potentialLegend;
    public Texture2D gradientLegend;

    private Vector3 startVertex;
    private Vector3 mousePos;

    private int _groupNum;
    public int groupNum
    {
        get { return _groupNum; }

        set {
            if (groupManager != null)
            {
                if (value >= 0 && value < groupManager.numOfGroups)
                {
                    _groupNum = value;
                }   
            }
        }
    }

    public bool allowKeyboardControls
    {
        get
        {
            return _allowKeyboardControls;
        }

        set
        {
            _allowKeyboardControls = value;
        }
    }

    Grid grid;
    TileMap tileMap;
    GroupManager groupManager;

    bool _allowKeyboardControls = true;


    void Start()
    {
        grid = FindObjectOfType<Grid>();
        tileMap = FindObjectOfType<TileMap>();
        groupManager = FindObjectOfType<GroupManager>();
        StartCoroutine(InitializeParams());

        CreatePotentialWaveGradient();

        SetupUI();
    }

    void SetupUI()
    {
        InputField[] inputFields;
        inputFields = InputField.FindObjectsOfType<InputField>();

        foreach (InputField inputField in inputFields)
        {
            if (string.Compare(inputField.name, "InputField_UpperLimit") == 0)
            {
                upperLimitField = inputField;
            }
            if (string.Compare(inputField.name, "InputField_LowerLimit") == 0)
            {
                lowerLimitField = inputField;
            }
        }
    }

    IEnumerator InitializeParams()
    {
        
        while (tileMap == null)
        {
            yield return null;
        }

        tex = new Texture2D(grid.gridSizeX, grid.gridSizeY, TextureFormat.RGB24, false);
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        pix = new Color[tex.width * tex.height];
        basePix = new Color[tex.width * tex.height];

        tileMap.GetComponent<Renderer>().sharedMaterials[0].mainTexture = tex;
        
        bool checkerboard = false;
        for (int y = 0 ; y < tex.height ; y++)
        {
            for (int x = 0 ; x < tex.width ; x++)
            {
                pix[y * tex.width + x] = grid[x, y].isWalkable ? (checkerboard ? new Color(0.8f, 0.8f, 1.0f, 1) : new Color(0.7f, 0.7f, 1.0f, 1)) : new Color(0.9f, 0.2f, 0.2f, 1);
                basePix[y * tex.width + x] = new Color(pix[y * tex.width + x].r, pix[y * tex.width + x].g, pix[y * tex.width + x].b, 1);
                checkerboard = !checkerboard;
            }
            checkerboard = !checkerboard;
        }

        if (pix.Length != 0)
        {
            tex.SetPixels(pix);
            tex.Apply();
        }

        yield break;
    }

    public void UpdateBaseTexture ()
    {
        if (!tex)
        {
            tex = new Texture2D(grid.gridSizeX, grid.gridSizeY, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            pix = new Color[tex.width * tex.height];
            basePix = new Color[tex.width * tex.height];
            tileMap.GetComponent<Renderer>().sharedMaterials[0].mainTexture = tex;
        }

        bool checkerboard = false;
        for (int y = 0 ; y < tex.height ; y++)
        {
            for (int x = 0 ; x < tex.width ; x++)
            {
                pix[y * tex.width + x] = grid[x, y].isWalkable ? (checkerboard ? new Color(0.8f, 0.8f, 1.0f, 1) : new Color(0.7f, 0.7f, 1.0f, 1)) : new Color(0.9f, 0.2f, 0.2f, 1);
                basePix[y * tex.width + x] = new Color(pix[y * tex.width + x].r, pix[y * tex.width + x].g, pix[y * tex.width + x].b, 1);
                checkerboard = !checkerboard;
            }
            checkerboard = !checkerboard;
        }

        if (true)
        {
            foreach (Vector2 node in groupManager.groups[0].startingNodes)
            {
                pix[(int)node.y * tex.width + (int)node.x] =
                    Color.Lerp(new Color(0.1f, 0.9f, 0.1f, 1), pix[(int)node.y * tex.width + (int)node.x], 0.5f);
            }
            foreach (Vector2 node in groupManager.groups[0].targetNodes)
            {
                pix[(int)node.y * tex.width + (int)node.x] =
                    Color.Lerp(new Color(0.9f, 0.9f, 0.1f, 1), pix[(int)node.y * tex.width + (int)node.x], 0.5f);
            }
            foreach (Vector2 node in groupManager.groups[1].startingNodes)
            {
                pix[(int)node.y * tex.width + (int)node.x] =
                    Color.Lerp(new Color(0.1f, 0.9f, 0.1f, 1), pix[(int)node.y * tex.width + (int)node.x], 0.5f);
            }
            foreach (Vector2 node in groupManager.groups[1].targetNodes)
            {
                pix[(int)node.y * tex.width + (int)node.x] =
                    Color.Lerp(new Color(0.1f, 0.9f, 0.9f, 1), pix[(int)node.y * tex.width + (int)node.x], 0.5f);
            }
        }
    }

    void CreatePotentialWaveGradient ()
    {
        potentialGrad = new Gradient();
        GradientColorKey[] potentialGradColorKey = new GradientColorKey[3];
        GradientAlphaKey[] potentialGradAlphaKey = new GradientAlphaKey[2];

        potentialGradAlphaKey[0].alpha = 0.1f;
        potentialGradAlphaKey[0].time = 0.0f;
        potentialGradAlphaKey[1].alpha = 0.1f;
        potentialGradAlphaKey[1].time = 1.0f;

        potentialGradColorKey[0].color = Color.Lerp(Color.white, Color.gray, 0.25f);
        potentialGradColorKey[0].time = 0.0f;
        potentialGradColorKey[1].color = Color.Lerp(Color.black, Color.gray, 1.0f);
        potentialGradColorKey[1].time = 0.5f;
        potentialGradColorKey[2].color = Color.Lerp(Color.white, Color.gray, 0.25f);
        potentialGradColorKey[2].time = 0.28572f;

        potentialGrad.SetKeys(potentialGradColorKey, potentialGradAlphaKey);
    }


    InputField upperLimitField, lowerLimitField;

    public void UpdateUILimitInputFields()
    {
        switch(displayFieldType)
        {
            case DisplayFieldTypes.DISCOMFORT:
                upperLimitField.text = inverseColorChangeRateDiscomfortUpper.ToString();
                lowerLimitField.text = inverseColorChangeRateDiscomfortLower.ToString();
                upperLimitField.interactable = true;
                lowerLimitField.interactable = true;
                break;
            case DisplayFieldTypes.DENSITY:
                upperLimitField.text = inverseColorChangeRateDensityUpper.ToString();
                lowerLimitField.text = inverseColorChangeRateDensityLower.ToString();
                upperLimitField.interactable = true;
                lowerLimitField.interactable = true;
                break;
            case DisplayFieldTypes.SPEED:
                upperLimitField.text = inverseColorChangeRateSpeedUpper.ToString();
                lowerLimitField.text = inverseColorChangeRateSpeedLower.ToString();
                upperLimitField.interactable = true;
                lowerLimitField.interactable = true;
                break;
            case DisplayFieldTypes.COST:
                upperLimitField.text = inverseColorChangeRateCostUpper.ToString();
                lowerLimitField.text = inverseColorChangeRateCostLower.ToString();
                upperLimitField.interactable = true;
                lowerLimitField.interactable = true;
                break;
            case DisplayFieldTypes.POTENTIAL:
                upperLimitField.text = inverseColorChangeRatePotentialUpper.ToString();
                lowerLimitField.text = inverseColorChangeRatePotentialLower.ToString();
                upperLimitField.interactable = true;
                lowerLimitField.interactable = true;
                break;
            case DisplayFieldTypes.POTENTIALWAVES:
                upperLimitField.text = inverseColorChangeRatePotentialWaveUpper.ToString();
                upperLimitField.interactable = true;
                lowerLimitField.interactable = false;
                break;
            default:
                upperLimitField.text = 0.0f.ToString();
                lowerLimitField.text = 0.0f.ToString();
                upperLimitField.interactable = false;
                lowerLimitField.interactable = false;
                break;
        }
    }

    void Update()
    {

    }

    void InputFunc()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            groupNum = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            groupNum = 1;
        }

        //
        // Choose display field
        //

        if (Input.GetKeyDown(KeyCode.A))
        {
            displayFieldType = DisplayFieldTypes.NONE;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            displayFieldType = DisplayFieldTypes.DISCOMFORT;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            displayFieldType = DisplayFieldTypes.DENSITY;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            displayFieldType = DisplayFieldTypes.AVG_VELOCITY;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            displayFieldType = DisplayFieldTypes.SPEED;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            displayFieldType = DisplayFieldTypes.COST;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            displayFieldType = DisplayFieldTypes.POTENTIAL;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            displayFieldType = DisplayFieldTypes.POTENTIALWAVES;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            displayFieldType = DisplayFieldTypes.GRADIENT;
        }

        //
        // Choose display vectors for 2d vector fields
        //

        if (Input.GetKeyDown(KeyCode.Q))
        {
            displayVectorType = DisplayVectorTypes.NONE;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            displayVectorType = DisplayVectorTypes.AVG_VELOCITY;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            displayVectorType = DisplayVectorTypes.GRADIENT;
        }

        //
        // Choose display direction for anisotropic fields
        //

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            displayAnisoDirectionType = DisplayAnisoDirectionTypes.EAST;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            displayAnisoDirectionType = DisplayAnisoDirectionTypes.NORTH;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            displayAnisoDirectionType = DisplayAnisoDirectionTypes.WEST;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            displayAnisoDirectionType = DisplayAnisoDirectionTypes.SOUTH;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            groupManager.groups[groupNum].EnableOrDisableGroupRendering();
        }
    }

    public void SetDisplayFieldType(int type)
    {
        displayFieldType = (DisplayFieldTypes)type;
    }
    public void SetDisplayVectorFieldType (int type)
    {
        displayVectorType = (DisplayVectorTypes)type;
    }
    public void SetDisplayAnisoDirectionType (int type)
    {
        displayAnisoDirectionType = (DisplayAnisoDirectionTypes)type;
    }

    public void SetCurrentFieldLimitsUpper ()
    {
        switch (displayFieldType)
        {
            case DisplayFieldTypes.DISCOMFORT:
                float.TryParse(upperLimitField.text, out inverseColorChangeRateDiscomfortUpper);
                break;
            case DisplayFieldTypes.DENSITY:
                float.TryParse(upperLimitField.text, out inverseColorChangeRateDensityUpper);
                break;
            case DisplayFieldTypes.SPEED:
                float.TryParse(upperLimitField.text, out inverseColorChangeRateSpeedUpper);
                break;
            case DisplayFieldTypes.COST:
                float.TryParse(upperLimitField.text, out inverseColorChangeRateCostUpper);
                break;
            case DisplayFieldTypes.POTENTIAL:
                float.TryParse(upperLimitField.text, out inverseColorChangeRatePotentialUpper);
                break;
            case DisplayFieldTypes.POTENTIALWAVES:
                float.TryParse(upperLimitField.text, out inverseColorChangeRatePotentialWaveUpper);
                break;
            default:
                break;
        }
    }
    public void SetCurrentFieldLimitsLower ()
    {
        switch (displayFieldType)
        {
            case DisplayFieldTypes.DISCOMFORT:
                float.TryParse(lowerLimitField.text, out inverseColorChangeRateDiscomfortLower);
                break;
            case DisplayFieldTypes.DENSITY:
                float.TryParse(lowerLimitField.text, out inverseColorChangeRateDensityLower);
                break;
            case DisplayFieldTypes.SPEED:
                float.TryParse(lowerLimitField.text, out inverseColorChangeRateSpeedLower);
                break;
            case DisplayFieldTypes.COST:
                float.TryParse(lowerLimitField.text, out inverseColorChangeRateCostLower);
                break;
            case DisplayFieldTypes.POTENTIAL:
                float.TryParse(lowerLimitField.text, out inverseColorChangeRatePotentialLower);
                break;
            default:
                break;
        }
    }



    void OnPostRender ()
    {
        if (!lineMaterial) {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        switch (displayFieldType)
        {
            case DisplayFieldTypes.NONE:
                DrawNone();
                break;
            case DisplayFieldTypes.DISCOMFORT:
                DrawDiscomfort(groupNum);
                break;
            case DisplayFieldTypes.DENSITY:
                DrawDensity(groupNum);
                break;
            case DisplayFieldTypes.AVG_VELOCITY:
                DrawAvgVelocity(groupNum);
                break;
            case DisplayFieldTypes.SPEED:
                DrawSpeed(groupNum, displayAnisoDirectionType);
                break;
            case DisplayFieldTypes.COST:
                DrawCost(groupNum, displayAnisoDirectionType);
                break;
            case DisplayFieldTypes.POTENTIAL:
                DrawPotential(groupNum);
                break;
            case DisplayFieldTypes.POTENTIALWAVES:
                DrawPotentialWaves(groupNum);
                break;
            case DisplayFieldTypes.GRADIENT:
                DrawGradient(groupNum);
                break;
            default:
                DrawNone();
                break;
        }

        switch (displayVectorType)
        {
            case DisplayVectorTypes.NONE:
                break;
            case DisplayVectorTypes.AVG_VELOCITY:
                DrawAvgVelocityLines(groupNum);
                break;
            case DisplayVectorTypes.GRADIENT:
                DrawGradientLines(groupNum);
                break;
            default:
                break;
        }
    }

    void DrawNone ()
    {
        UpdateBaseTexture();

        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawDiscomfort (int groupNum)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        DiscomfortField discomfortField = ccnmb.discomfortField;
        float colorIntensity;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                float disc = discomfortField[x, y];
                if (grid[x, y].discomfort > 0 && grid[x, y].discomfort > discomfortField[x, y])
                {
                    disc += grid[x, y].discomfort;
                }
                colorIntensity = (disc - inverseColorChangeRateDiscomfortLower) / inverseColorChangeRateDiscomfortUpper;
                pix[y * tex.width + x] = discomfortLegend.GetPixelBilinear(0, colorIntensity);

            }
        }

        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawDensity (int groupNum)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        DensityField densityField = ccnmb.densityField;
        float colorIntensity;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                colorIntensity = (densityField[x, y] - inverseColorChangeRateDensityLower) / inverseColorChangeRateDensityUpper;
                pix[y * tex.width + x] = densityLegend.GetPixelBilinear(0, colorIntensity);

            }
        }

        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawAvgVelocity (int groupNum)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        VelocitySumField velocitySumField = ccnmb.velocitySumField;
        DensityField densityField = ccnmb.densityField;
        Vector2 avgVelocityVal;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                avgVelocityVal = (densityField[x, y] == 0.0f ? new Vector2(0, 0) : velocitySumField[x, y] / densityField[x, y]);
                pix[y * tex.width + x] =
                        avgVelocityLegend.GetPixelBilinear((avgVelocityVal.normalized.x * 0.48f + 0.5f), (avgVelocityVal.normalized.y * 0.48f + 0.5f));
            }
        }

        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawSpeed(int groupNum, DisplayAnisoDirectionTypes direction)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        SpeedField speedField = ccnmb.speedField;
        float colorIntensity;
        potentialLegend.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                colorIntensity = (speedField[x, y][(int)direction] - inverseColorChangeRateSpeedLower) / inverseColorChangeRateSpeedUpper;
                if (grid[x, y].isWalkable)
                {
                    pix[y * tex.width + x] = speedLegend.GetPixelBilinear(0, 1 - colorIntensity);
                }
                else
                    pix[y * tex.width + x] = Color.black;
            }
        }
        potentialLegend.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawCost(int groupNum, DisplayAnisoDirectionTypes direction)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        CostField costField = ccnmb.costField;
        float colorIntensity;
        potentialLegend.wrapMode = TextureWrapMode.Repeat;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                colorIntensity = (costField[x, y][(int)direction] - inverseColorChangeRateCostLower) / inverseColorChangeRateCostUpper;
                pix[y * tex.width + x] = costLegend.GetPixelBilinear(0, 1-colorIntensity);

            }
        }
        potentialLegend.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawPotential(int groupNum)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        PotentialField potentialField = ccnmb.potentialField;
        float colorIntensity;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                colorIntensity = (potentialField[x, y] - inverseColorChangeRatePotentialLower) / inverseColorChangeRatePotentialUpper;

                if (potentialField[x, y] == 0)
                    pix[y * tex.width + x] = Color.black;
                else
                    pix[y * tex.width + x] = potentialLegend.GetPixelBilinear(0, colorIntensity);
               
            }
        }
        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawPotentialWaves(int groupNum)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        PotentialField potentialField = ccnmb.potentialField;
        GradientField gradientField = ccnmb.gradientField;
        float colorIntensity;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                colorIntensity = (potentialField[x, y] % inverseColorChangeRatePotentialWaveUpper) / (float)inverseColorChangeRatePotentialWaveUpper;
                if (grid[x, y].isWalkable)
                {
                    pix[y * tex.width + x] = potentialGrad.Evaluate(colorIntensity);
                }
                else
                    pix[y * tex.width + x] = Color.black;
            }
        }

        tex.SetPixels(pix);
        tex.Apply();
    }


    void DrawGradient(int groupNum)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        GradientField gradientField = ccnmb.gradientField;
        for (int y = 0 ; y < grid.gridSizeY ; y++)
        {
            for (int x = 0 ; x < grid.gridSizeX ; x++)
            {
                pix[y * tex.width + x] =
                    gradientLegend.GetPixelBilinear((gradientField[x, y].normalized.x * 0.48f + 0.5f)/* * 0.5f */, (gradientField[x, y].normalized.y * 0.48f + 0.5f)/* * 0.5f */);
            }
        }

        tex.SetPixels(pix);
        tex.Apply();
    }

    void DrawGradientLines(int groupNum)
    {
        Vector3 from = new Vector3();
        Vector3 to = new Vector3();

        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        GradientField gradientField = ccnmb.gradientField;
        Vector2 grad;

        Node n;

        GL.Flush();
        GL.PushMatrix();
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        GL.Color(Color.black);
        for (int i = 0 ; i < grid.gridSizeX ; i++)
        {
            for (int j = 0 ; j < grid.gridSizeY ; j++)
            {
                n = grid[i, j];

                from.Set((n.worldPosition.x), 0, (n.worldPosition.z));
                grad = gradientField[i, j].normalized;
                to.Set((n.worldPosition.x + -grad[0] * 0.8f), 0, (n.worldPosition.z + -grad[1] * 0.8f));

                GL.Vertex(from);
                GL.Vertex(to);
            }
        }
        GL.End();
        GL.PopMatrix();
    }

    void DrawAvgVelocityLines(int groupNum)
    {
        Vector3 from = new Vector3();
        Vector3 to = new Vector3();

        ContinuumCrowdsNoMB ccnmb = groupManager.groups[groupNum].ccnmb;
        VelocitySumField velocitySumField = ccnmb.velocitySumField;
        DensityField densityField = ccnmb.densityField;
        Vector2 velocityVec;

        Node n;

        GL.Flush();
        GL.PushMatrix();
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        GL.Color(Color.black);
        for (int i = 0 ; i < grid.gridSizeX ; i++)
        {
            for (int j = 0 ; j < grid.gridSizeY ; j++)
            {
                n = grid[i, j];

                from.Set((n.worldPosition.x), 0, (n.worldPosition.z));
                velocityVec = velocitySumField[i, j].normalized;
                to.Set((n.worldPosition.x + velocityVec[0] * 0.8f), 0, (n.worldPosition.z + velocityVec[1] * 0.8f));

                GL.Vertex(from);
                GL.Vertex(to);
            }
        }
        GL.End();
        GL.PopMatrix();
    }
}