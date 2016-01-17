using UnityEngine;
using System.Collections;
using SequenceTrainLogic;
using System;

public class TrainCarHandler : MonoBehaviour {
	private GameObject trainObject;
	public AbstractTrainCar trainCar;
	public LogicEngine parent;
	// Use this for initialization
	void Start () {
		resizeLength(parent.calculatedCarLenth);
	}
	
	// Update is called once per frame
	void Update () {
		positionSelf();
	}

	internal void positionSelf() {
		//get tile pos
		float newX = trainCar.x * parent.tileSize;
		float newZ = -trainCar.y * parent.tileSize;
		Vector3 add = new Vector3(newX, 5, newZ);
		//do more things later....
		trainObject.transform.position = add;
		var rotation = trainObject.transform.rotation;
		var cardinal = trainCar.entry;
		int angle = (int)cardinal;
//		multiplier = (multiplier + 3) % 4;
		//adjust things so north is north and east is east
		switch (cardinal) {
			case TrackSide.North:
				angle = 0;
				break;
			case TrackSide.East:
				angle = -90;
				break;
			case TrackSide.South:
				angle = 180;
				break;
			case TrackSide.West:
				angle = 90;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		rotation = Quaternion.AngleAxis(angle, Vector3.up);// multiplier * 90;
		trainObject.transform.rotation = rotation;
	}

	public static GameObject createGameObject(AbstractTrainCar trainCar,
	                                          LogicEngine parent) {
		GameObject trainObject;
		//create my game object!
		if(trainCar.trainIndex == 0) {
			//engine
			trainObject = UnityEngine.Object.Instantiate(parent.trainEngine);
		}
		else if(trainCar.trainIndex == -1) {
			//caboose
			trainObject = UnityEngine.Object.Instantiate(parent.caboose);
		}
		else {
			trainObject = UnityEngine.Object.Instantiate(parent.trainCar);
			parent.setColorForGameObject(trainObject, trainCar.trainIndex);
		}
		var carHandler = trainObject.AddComponent<TrainCarHandler>();
		carHandler.trainCar = trainCar;
		carHandler.trainObject = trainObject;
		carHandler.parent = parent;
		return trainObject;
	}

	public void resizeLength(float length){
		var localScale = trainObject.transform.localScale;
		localScale.z = length;
		localScale.x = parent.tileSize - parent.tileSize / 4;
		trainObject.transform.localScale = localScale;
	}
}
