using UnityEngine;
using System.Collections;
using SequenceTrainLogic;

public class Rotater : MonoBehaviour {
	public LogicEngine parent;
	public int x, y;
	public bool isTopObject;
	public TrackEnds north, east, south, west;
	internal TrackEnds[] ends;
	internal int rotationOffset;
	// Use this for initialization
	void Start () {
		buildArray();
	}
	internal void buildArray(){
		if (ends != null){
			return;
		}
		ends = new TrackEnds[]{
			north,
			east,
			south,
			west
		};
	}
	void OnMouseDown(){
		if (Input.GetMouseButton(0)){
			Rotate(true);
		}
		else if (Input.GetMouseButton(1)){
			Rotate(false);
		}
	}

	void Rotate(bool clockWize) {
		parent.DoRotate(this, clockWize);
	}
	
	// Update is called once per frame
	void Update () {

	}
}
