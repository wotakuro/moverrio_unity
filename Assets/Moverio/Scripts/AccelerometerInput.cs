using UnityEngine;
using System.Collections;

public class AccelerometerInput : MonoBehaviour {
    /*---- vars ----*/
    private Vector3[] accData = new Vector3[32];
    private int index;
    private bool updateFlag;

    private bool lastJudge = false;
    private bool currentJudge = false;


    private static AccelerometerInput instance = null;

    /// <summary>
    /// checking device is shaked just now
    /// </summary>
    /// <returns></returns>
    public static bool IsDownInput(){
        if( instance == null ){
            return false;
        }
        return ( (!instance.lastJudge) && (instance.currentJudge) );
    }

    // Use this for initialization
	void Start () {
        instance = this;
        updateFlag = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }


    /// <summary>
    /// on destroy
    /// </summary>
    void OnDestroy(){
        instance = null;
        Screen.sleepTimeout = 0;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (updateFlag)
        {
            this.UpdateGraph();
            this.lastJudge = this.currentJudge;
            this.currentJudge = this.JudgeInput();
        }
	}
    void UpdateGraph()
    {
        Vector3 data = Input.acceleration;
        int length = accData.Length;

        this.accData[this.index % length] = data;
        ++this.index;
    }

    private bool JudgeInput()
    {
        if (this.index < 4) {
            return false;
        }
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 1; i < 4; ++i)
        {
            int tmpIdx = (index + accData.Length - i) % accData.Length;
            min.x = Mathf.Min(accData[tmpIdx].x, min.x);
            min.y = Mathf.Min(accData[tmpIdx].y, min.y);
            min.z = Mathf.Min(accData[tmpIdx].z, min.z);

            min.x = Mathf.Max(accData[tmpIdx].x, max.x);
            max.y = Mathf.Max(accData[tmpIdx].y, max.y);
            max.z = Mathf.Max(accData[tmpIdx].z, max.z);
        }
        float param = 0.35f;
        if (max.x - min.x > param)
        {
            return true;
        }
        if (max.y - min.y > param)
        {
            return true;
        }
        if (max.z - min.z > param)
        {
            return true;
        } 
        return false;
    }
}
