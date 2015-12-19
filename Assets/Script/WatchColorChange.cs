using UnityEngine;
using System.Collections;

public class WatchColorChange : MonoBehaviour
{
   private Material mat;

	// Use this for initialization
	void Start () {
        Renderer render = this.GetComponent<Renderer>();
        this.mat = render.material;
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnWatchStart()
    {
        if (this.mat != null) {
            this.mat.color = Color.yellow;
        }
    }
    void OnWatching(float time)
    {
    }
    void OnWatchEnd()
    {
        if (this.mat != null)
        {
            this.mat.color = Color.white;
        }
    }
}
