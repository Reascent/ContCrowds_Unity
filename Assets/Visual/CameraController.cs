using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

     Camera cam;

    Vector3 originalPosition;

    private Vector3 v3Pos;
    //private float threshold = 9;

    Vector3 movementVect;

    bool _allowCameraMovement = true;

    public bool allowCameraMovement
    {
        get
        {
            return _allowCameraMovement;
        }

        set
        {
            _allowCameraMovement = value;
        }
    }


    // Use this for initialization
    void Start () {
        cam = Camera.allCameras[0];
        originalPosition = Input.mousePosition;
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButton(2))
        {
            movementVect = Input.mousePosition - originalPosition;
            movementVect.z = movementVect.y;
            movementVect.y = 0;

            cam.transform.position -= 0.5f * movementVect;

            originalPosition = Input.mousePosition;
        }
        originalPosition = Input.mousePosition;
        if (allowCameraMovement)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                if (cam.orthographicSize > 5)
                    cam.orthographicSize -= 2;
            }
            if (Input.mouseScrollDelta.y < 0)
            {
                if (cam.orthographicSize  < 180 )
                    cam.orthographicSize += 2;
            }
        }
    }


    public void SetAllowCameraMovement()
    {
        _allowCameraMovement = true;
    }
    public void SetDisallowCameraMovement ()
    {
        _allowCameraMovement = false;
    }


}
