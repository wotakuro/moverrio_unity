using UnityEngine;
using System.Collections;

public class AccelerometerInput : MonoBehaviour {
    /*---- vars ----*/
    private Vector3[] graphData = new Vector3[32];
    private int index;
    private bool updateFlag;

    private bool lastJudge = false;
    private bool currentJudge = false;


    private static AccelerometerInput instance = null;

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
        int length = graphData.Length;

        this.graphData[this.index % length] = data;
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
            int tmpIdx = (index + graphData.Length - i) % graphData.Length;
            min.x = Mathf.Min(graphData[tmpIdx].x, min.x);
            min.y = Mathf.Min(graphData[tmpIdx].y, min.y);
            min.z = Mathf.Min(graphData[tmpIdx].z, min.z);

            min.x = Mathf.Max(graphData[tmpIdx].x, max.x);
            max.y = Mathf.Max(graphData[tmpIdx].y, max.y);
            max.z = Mathf.Max(graphData[tmpIdx].z, max.z);
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
