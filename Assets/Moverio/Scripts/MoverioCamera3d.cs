/**
Copyright (c) 2015 Yusuke Kurokawa

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the 
"Software"), to deal in the Software without restriction, including 
without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to 
permit persons to whom the Software is furnished to do so, subject to 
the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/


using UnityEngine;
using System.Collections;

/// <summary>
/// Camera for Moverio BT-200.
// </summary>
public class MoverioCamera3d : MonoBehaviour
{
    /*--------------------- enum ----------------------*/
    /// <summary>
    /// Which sensor used for moving camera.
    /// </summary>
    public enum ECameraMovement
    {
        NONE,       //!< No sensor.
        GYRO,       //!< Use gyro sensor
        COMPASS,    //!< Use magnetic field sensor.( this uses gyro sensor too.)
    };
    /// <summary>
    /// If you use sensors.You can lock Rotation.
    /// </summary>
    public enum ELockRotation
    { 
        NONE,           //!< No lock
        LOCK_YAW,       //!< Lock Y rotation
        LOCK_ROLL,      //!< Lock Z rotation
        ONLY_ROLL,      //!< Lock X,Y rotation
        ONLY_PITCH,     //!< Lock Y,Z rotation
        ONLY_YAW,       //!< Lock X,Z rotation
    }


    /*--------------------- propeties ----------------------*/
    /// left-camera and right-camera distance. If this is 0.0f ,set 2d mode.
    public float cameraDistance = 0.2f;
    /// if you cliping with circle, this should be true.
    public bool useCircleBoard = true;
    /// Which sensor do you use for moving camera.
    public ECameraMovement cameraMove = ECameraMovement.NONE;
    /// the type of rotation( if cameraMove is not "NONE", this is available )
    public ELockRotation lockRotation = ELockRotation.NONE;
    /// if you want to use "OnWatchStart","OnWatchEnd" and "OnWatching" event, this should be true.  
    public bool isOnWatchEventEnable = true;

    /*--------------------- private vars ----------------------*/
    /// remember last update camera distance
    private float cameraDistOld = -0.1f;
    /// left camera instance
    private Camera lCam;
    /// right camera instance
    private Camera rCam;
    /// icon that indicate watching.This is alway in center of screen.
    private Transform watchingIcon = null;
    /// parameter for gyro Adjust rotation
    private float gyroRotationY = 180.0f;
    /// left circle board( if "useCircleBoard" is true,this is used )
    private Transform lBoard;
    /// right circle board( if "useCircleBoard" is true,this is used )
    private Transform rBoard;
    /// Gameobject that watched now.
    private GameObject watchingObject;
    /// Counting time how long the gameObject is watched.
    private float watchingTime;
    /// Adjust a Rotation flag
    private bool adjustFlag = false;

#if UNITY_EDITOR || (!UNITY_IPHONE  &&  !UNITY_ANDROID)
    /// remember Mouse position for simulate sensors.
    private Vector3 mouseOldPos;
    /// シミュレーションした回転
    private Quaternion simulateQuaternion;
#endif

    /*--------------------- functions ----------------------*/
    /// <summary>
    /// Use this for initialization
    /// </summary>
    protected void Start()
    {
        // iinitialize vars
        Transform lCamTrans = transform.Find("l_camera");
        Transform rCamTrans = transform.Find("r_camera");
        this.rCam = rCamTrans.GetComponent<Camera>();
        this.lCam = lCamTrans.GetComponent<Camera>();
        this.lBoard = lCamTrans.Find("board");
        this.rBoard = rCamTrans.Find("board");
        this.watchingIcon = transform.Find("nowWatching");

        // set circle board in front of any thing(
        this.lBoard.GetComponent<Renderer>().material.renderQueue = 10000;
        this.rBoard.GetComponent<Renderer>().material.renderQueue = 10000;

        // set board draw flag
        this.setCircleBoardFlag(this.useCircleBoard);

        // device only 
#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
    // set gyro sensor
    if( this.cameraMove == ECameraMovement.GYRO ){
        Input.gyro.enabled = true;
        this.adjustFlag = true;
    }
    // set magnetic field sensor
    if( this.cameraMove == ECameraMovement.COMPASS ){
        Input.compass.enabled = true;
        Input.gyro.enabled = true;
    }
#endif
            // simulateonly
#if UNITY_EDITOR && (UNITY_IPHONE  ||  UNITY_ANDROID)
        simulateQuaternion = Quaternion.identity;
#endif
        this.cameraDistOld = this.cameraDistance - 1.0f;
    }

    /// @brief pause/resume 
    protected void OnApplicationPause(bool pState)
    {
    // device only
#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
        if(pState){
            MoverioUtil.setDisplayMode(  MoverioUtil.DISPLAY_MODE_2D );
        }else{
            this.cameraDistOld = this.cameraDistance - 1.0f;
            this.adjustFlag = true;
        }
#endif
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    protected void Update()
    {
        Quaternion rotate = Quaternion.identity;
        if (this.adjustFlag) {
            this.AdjustGyroRotationY();
            this.adjustFlag = false;
        }
        this.updateCameraDistance();
        if ( this.cameraMove == ECameraMovement.GYRO)
        {
            rotate = this.getGyroRotation();
        }
        else if (this.cameraMove == ECameraMovement.COMPASS) {
            rotate = this.getCompassRotation();
        }
        // lock rotation
        rotate = this.updateLockRotation(rotate);
        transform.rotation = rotate;
        // raycast for "OnWatchStart"/"OnWatchEnd"/"OnWatching" Event
        this.updateWatchEvent();

        //  set board fla
        this.setCircleBoardFlag(this.useCircleBoard);
    }

    /// <summary>
    /// update camera distance
    /// </summary>
    private void updateCameraDistance()
    {
        // if last update distance is same, nothing to do.
        if (this.cameraDistance == this.cameraDistOld)
        {
            return;
        }

        if (this.cameraDistance <= 0.0f)
        {
            this.cameraDistance = 0.0f;
            this.set2DCameraMode();
        }
        else if (this.cameraDistOld <= 0.0f)
        {
            this.set3DCameraMode();
        }
        lCam.transform.localPosition = new Vector3(-this.cameraDistance * 0.5f, 0.0f, 0.0f);
        rCam.transform.localPosition = new Vector3(this.cameraDistance * 0.5f, 0.0f, 0.0f);
        this.cameraDistOld = this.cameraDistance;
    }

    /// <summary>
    /// Update WatchEvent using raycast.
    /// </summary>
    private void updateWatchEvent() {
        if (!this.isOnWatchEventEnable) {
            return;
        }
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;
        GameObject currentWatch = null;
        if (Physics.Raycast(ray, out hitInfo)) {
            currentWatch = hitInfo.collider.gameObject;
        }
        if (this.watchingObject != null) {
            this.watchingObject.SendMessage("OnWatching", watchingTime, SendMessageOptions.DontRequireReceiver);
        }
        if (currentWatch != this.watchingObject)
        {
            if (currentWatch != null)
            {
                currentWatch.SendMessage("OnWatchStart", SendMessageOptions.DontRequireReceiver);
            }
            if (this.watchingObject != null)
            {
                this.watchingObject.SendMessage("OnWatchEnd", SendMessageOptions.DontRequireReceiver);
            }
            watchingTime = 0.0f;
        }
        else {
            watchingTime += Time.deltaTime;
        }
        this.watchingObject = currentWatch;
    }

    /// <summary>
    /// change 3D camera mode
    /// </summary>
    private void set3DCameraMode()
    {
        // Mov\erio Camera Set
#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
    MoverioUtil.setDisplayMode(  MoverioUtil.DISPLAY_MODE_3D );
#endif
        // both of camera is enabled
        lCam.enabled = true;
        rCam.enabled = true;
        // set camera render rect
        lCam.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
        rCam.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
        //  set camera aspect
        lCam.aspect = ((float)Screen.width / (float)Screen.height);
        rCam.aspect = ((float)Screen.width / (float)Screen.height);
    }

    /// <summary>
    /// change 2D camera mode
    /// </summary>
    private void set2DCameraMode()
    {
        // Moverio Camera Set
#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
    MoverioUtil.setDisplayMode(  MoverioUtil.DISPLAY_MODE_2D );
#endif
        // left camera only enabled
        lCam.enabled = true;
        rCam.enabled = false;
        // set camera render rect
        lCam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        //  set camera aspect
        lCam.aspect = ((float)Screen.width / (float)Screen.height);
    }

    /// <summary>
    /// update transform using gyro sensor.
    /// </summary>
    private Quaternion getGyroRotation()
    {
#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
        //
        Quaternion gyro = Input.gyro.attitude;
        gyro.x = -gyro.x;
        gyro.y = -gyro.y;

        Quaternion rotation = Quaternion.Euler(0, 0, 180) * Quaternion.Euler(-90, gyroRotationY , 0);
        return rotation * gyro;
        //transform.rotation = gyro;

#else
        return this.simulateCameraMove();
#endif
    }


    /// <summary>
    /// Adjust a gyro rotationY parameter
    /// </summary>
    private void AdjustGyroRotationY()
    {
        Quaternion gyro = Input.gyro.attitude;
        gyro.x = -gyro.x;
        gyro.y = -gyro.y;
        Quaternion rotation = Quaternion.Euler(0, 0, 180) * Quaternion.Euler(-90, 0, 0);
        Vector3 tmpVec = (rotation * gyro ) * Vector3.forward;
        this.gyroRotationY = Mathf.Atan2(tmpVec.x, tmpVec.z) * Mathf.Rad2Deg;
    }



#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
    private Quaternion _correction = Quaternion.identity;
    private Quaternion _targetCorrection = Quaternion.identity;
    private Quaternion _compassOrientation = Quaternion.identity;
#endif
    /// <summary>
    /// update transform using magnetic field sensor.
    /// </summary>
    private Quaternion getCompassRotation()
    {
#if !UNITY_EDITOR &&(UNITY_IPHONE  ||  UNITY_ANDROID)
        Vector3 gravity = Input.gyro.gravity.normalized;
        Vector3 flatNorth = Input.compass.rawVector - Vector3.Dot(gravity, Input.compass.rawVector) * gravity;
        Quaternion gyroOrientation = Quaternion.Euler(0, 0, -180) * Input.gyro.attitude * Quaternion.Euler(0, 0, 180);
 
         _compassOrientation = Quaternion.Euler (0, 0, -180) * 
             Quaternion.Inverse(Quaternion.LookRotation(-flatNorth, -gravity)) *
             Quaternion.Euler(0, 0, 180);

        // Calculate the target correction factor
        _targetCorrection = _compassOrientation * Quaternion.Inverse(gyroOrientation);
        // Jump straight to the target correction if it's a long way; otherwise, slerp towards it very slowly
        if (Quaternion.Angle(_correction, _targetCorrection) < 15)
            _correction = _targetCorrection;
        else
            _correction = Quaternion.Slerp(_correction, _targetCorrection, 0.02f);

        // Easy bit
        return _correction * gyroOrientation;

#else
        return this.simulateCameraMove();
#endif
    }

    /// <summary>
    /// update transform using magnetic field sensor.
    /// </summary>
    private Quaternion updateLockRotation(Quaternion rot)
    {
        switch (this.lockRotation)
        { 
            case ELockRotation.NONE:
                break;
            case ELockRotation.ONLY_PITCH:
                {
                    Vector3 tmpVec  = rot * Vector3.forward;
                    Vector3 tmpVec2 = tmpVec;
                    tmpVec2.z = Mathf.Sqrt(tmpVec2.z * tmpVec2.z + tmpVec2.x * tmpVec2.x);
                    tmpVec2.x = 0.0f;
                    tmpVec.Normalize();
                    tmpVec2.Normalize();

                    Quaternion tmpRot = Quaternion.FromToRotation(tmpVec2 , tmpVec);

                    rot = Quaternion.LookRotation( tmpVec2, tmpRot * Vector3.up);
                }
                break;
            case ELockRotation.ONLY_YAW:
                {
                    rot = getLockYaw( rot );
                }
                break;
            case ELockRotation.ONLY_ROLL:
                {
                }
                break;
            case ELockRotation.LOCK_YAW:
                {
                    Quaternion invRot = getLockYaw(rot);
                    rot = Quaternion.Inverse( invRot ) * rot;
                }
                break;
            case ELockRotation.LOCK_ROLL:
                {
                    Vector3 tmp = transform.forward;
                    tmp.y = 0.0f;
                }
                break;
            default:
                break;
        }
        return rot;
    }

    private Quaternion getLockYaw(Quaternion rot)
    {
        Vector3 forwardVec = rot * Vector3.forward;
        if (forwardVec.x != 0.0f && forwardVec.z != 0.0f)
        {
            forwardVec.y = 0.0f;
        }
        return Quaternion.LookRotation(forwardVec.normalized, Vector3.up);
    }

    /// <summary>
    /// simulate Camera move for pc.
    /// </summary>
#if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID)
    private Quaternion simulateCameraMove()
    { 
        if (Input.GetMouseButtonDown(0))
        {
            mouseOldPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - mouseOldPos;
            delta *= 0.1f;
            this.simulateQuaternion = Quaternion.Euler(-delta.y, delta.x, delta.z) * this.simulateQuaternion;
            mouseOldPos = Input.mousePosition;
        }
        this.simulateQuaternion = Quaternion.Euler(0.0f,0.0f, (float)Input.mouseScrollDelta.y) * this.simulateQuaternion;
        return simulateQuaternion;
    }
#endif

    /// <summary>
    /// set cliping cliping circle board flag.
    /// </summary>
    /// <param name="flag">If use cliping circle,this should be true.</param>
    private void setCircleBoardFlag(bool flag)
    {
        this.lBoard.GetComponent<Renderer>().enabled = this.useCircleBoard;
        this.rBoard.GetComponent<Renderer>().enabled = this.useCircleBoard;
    }

    /// <summary>
    /// convert mouse position to ray.
    /// if you want to put object on mouse position
    /// </summary>
    /// <returns>Converted ray infomation</returns>
    public Ray convertMousePositionRay()
    {
        Camera camObj = this.lCam;

        Vector3 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;
        Ray ray = camObj.ViewportPointToRay(mousePos);

        return ray;
    }
}
