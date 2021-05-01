using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainGameLoop : MonoBehaviour {

    Grid grid;
    GroupManager groupManager;

    int _uiGroupNum = 0;

    void Start () {
        grid = FindObjectOfType<Grid>();
        groupManager = FindObjectOfType<GroupManager>();

        SetupUI();
        Invoke(nameof(UpdateUIComputationFields), 1);
        Invoke(nameof(UIUpdateAgentCountField), 1);
    }


    public enum RunStates
    {
        RESET,
        PAUSE,
        PLAY,
        STEP,
    }

    public void SetRunningState(int intstate)
    {
        RunStates state = (RunStates)intstate;

        switch (state)
        {
            case RunStates.RESET:
                groupManager.ResetAllGroupsPositions();
                counter = 0;
                break;
            case RunStates.PAUSE:
            case RunStates.PLAY:
                run = !run;
                step = false;
                break;
            case RunStates.STEP:
                run = true;
                step = true;
                break;
            default:
                break;
        }
    }

    int counter = 0;
    bool run = false;
    bool step = false;

    public int UiGroupNum
    {
        get
        {
            return _uiGroupNum;
        }
        set
        {
            _uiGroupNum = value;
            UpdateUIComputationFields();
            UIUpdateAgentCountField();
        }
    }

    void Update () {
        // while not paused
        // for each unitgroup
        // calculate height gradients
        // calculate density and average velocity fields
        // calculate velocity field (the vectors)
        // move units according to velocity (speed and potential field)

        if (Input.GetKeyDown(KeyCode.N))
            step = !step;
        if (Input.GetKeyDown(KeyCode.M))
            run = !run;
        if (run) {
            groupManager.RunContCrowdsLoopOnce();
            
            groupManager.MoveAllUnits();
            
            if (step)
                run = false;
            if (!CheckIfAllParked())
                counter++;
        }
        UIUpdateStepsText();
    }

    InputField weightLengthField, weightTimeField, weightDiscomfortField;
    InputField minDensityField, maxDensityField;
    InputField densityCoefficientField, discomfortCoefficientField;
    InputField maxSpeedField, discomfortFutureStepsField;
    InputField agentCountField;

    Text stepsCount;

    void SetupUI ()
    {
        InputField[] inputFields;
        inputFields = InputField.FindObjectsOfType<InputField>();

        foreach (InputField inputField in inputFields)
        {
            if (string.Compare(inputField.name, "InputField_Length") == 0)
            {
                weightLengthField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_Time") == 0)
            {
                weightTimeField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_Discomfort") == 0)
            {
                weightDiscomfortField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_MinDensity") == 0)
            {
                minDensityField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_MaxDensity") == 0)
            {
                maxDensityField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_DensityCoefficient") == 0)
            {
                densityCoefficientField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_FutureSteps") == 0)
            {
                discomfortFutureStepsField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_DiscomfortCoefficient") == 0)
            {
                discomfortCoefficientField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_MaxSpeed") == 0)
            {
                maxSpeedField = inputField;
            }
            else if (string.Compare(inputField.name, "InputField_AgentCount") == 0)
            {
                agentCountField = inputField;
            }
        }

        Text[] texts;
        texts = Text.FindObjectsOfType<Text>();

        foreach (Text text in texts)
        {
            if (string.Compare(text.name, "Text_NumOfSteps") == 0)
            {
                stepsCount = text;
            }
        }

    }

    public void UpdateUIComputationFields()
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[UiGroupNum].ccnmb;
        UnitGroup group = groupManager.groups[UiGroupNum];

        weightLengthField.text          = ccnmb.Weight_length.ToString();
        weightTimeField.text            = ccnmb.Weight_time.ToString();
        weightDiscomfortField.text      = ccnmb.Weight_discomfort.ToString();
        minDensityField.text            = ccnmb.DensityMin.ToString();
        maxDensityField.text            = ccnmb.DensityMax.ToString();
        densityCoefficientField.text    = group.groupDensityCoefficient.ToString();
        discomfortCoefficientField.text = group.groupDiscomfortCoefficient.ToString();
        discomfortFutureStepsField.text = ccnmb.StepsInTheFuture.ToString();
        maxSpeedField.text              = ccnmb.Fmax.ToString();
    }

    public void SetComputationFields(int action)
    {
        ContinuumCrowdsNoMB ccnmb = groupManager.groups[UiGroupNum].ccnmb;
        UnitGroup group = groupManager.groups[UiGroupNum];

        float fOutParam;
        int dOutParam;

        if (action == 0 && float.TryParse(weightLengthField.text, out fOutParam))
        {
            ccnmb.Weight_length = fOutParam >= 0 ? fOutParam : 0;
            weightLengthField.text = ccnmb.Weight_length.ToString();
        }
        else if (action == 1 && float.TryParse(weightTimeField.text, out fOutParam))
        {
            ccnmb.Weight_time = fOutParam >= 0 ? fOutParam : 0;
            weightTimeField.text = ccnmb.Weight_time.ToString();
        }
        else if (action == 2 && float.TryParse(weightDiscomfortField.text, out fOutParam))
        {
            ccnmb.Weight_discomfort = fOutParam >= 0 ? fOutParam : 0;
            weightDiscomfortField.text = ccnmb.Weight_discomfort.ToString();
        }
        else if (action == 3 && float.TryParse(minDensityField.text, out fOutParam))
        {
            foreach (UnitGroup unitgroup in groupManager.groups)
            {
                unitgroup.ccnmb.DensityMin = fOutParam >= 0 && fOutParam <= ccnmb.DensityMax ? fOutParam : (fOutParam >= 0 ? ccnmb.DensityMax : 0);
            }
            minDensityField.text = ccnmb.DensityMin.ToString();
        }
        else if (action == 4 && float.TryParse(maxDensityField.text, out fOutParam))
        {
            foreach (UnitGroup unitgroup in groupManager.groups)
            {
                unitgroup.ccnmb.DensityMax = fOutParam >= ccnmb.DensityMin ? fOutParam : ccnmb.DensityMin;
            }
            maxDensityField.text = ccnmb.DensityMax.ToString();
        }
        else if (action == 5 && float.TryParse(densityCoefficientField.text, out fOutParam))
        {
            group.groupDensityCoefficient = fOutParam >= 0 ? fOutParam : 0;
        }
        else if (action == 6 && int.TryParse(discomfortFutureStepsField.text, out dOutParam))
        {
            foreach (UnitGroup unitgroup in groupManager.groups)
            {
                unitgroup.ccnmb.StepsInTheFuture = dOutParam >= 0 ? dOutParam : 0;
            }
            discomfortFutureStepsField.text = ccnmb.StepsInTheFuture.ToString();
        }
        else if (action == 7 && float.TryParse(discomfortCoefficientField.text, out fOutParam))
        {
            group.groupDiscomfortCoefficient = fOutParam >= 0 ? fOutParam : 0;
            discomfortCoefficientField.text = group.groupDiscomfortCoefficient.ToString();
        }
        else if (action == 8 && float.TryParse(maxSpeedField.text, out fOutParam))
        {
            ccnmb.Fmax = fOutParam >= 0 ? fOutParam : 0;
            maxSpeedField.text = ccnmb.Fmax.ToString();
        }
    }

    public void UIUpdateAgentCountField()
    {
        agentCountField.text = groupManager.GetAgentCount(UiGroupNum).ToString();
    }

    public void UIUpdateStepsText()
    {
        stepsCount.text = counter.ToString();
    }

    public void SetUnitCount()
    {
        int.TryParse(agentCountField.text, out int dOutParam);
        if (dOutParam >= 0)
        {
            groupManager.SetAgentCount(UiGroupNum, dOutParam);
        }
    }

    bool CheckIfAllParked()
    {
        return (groupManager.parkedUnits == groupManager.GetUnitCount() && !groupManager.ImmediateRestart);
    }
}
