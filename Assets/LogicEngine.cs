using UnityEngine;
using System.Collections;
using SequenceTrainLogic;
using System;
using System.Text;

public class LogicEngine : MonoBehaviour {
	public GameObject curved, doubleCurved, straight, doubleStraight;
	public int width, length;
	public ulong seed;
	public bool trainCanCrashWithSelf;
	public int blockSections = 100;
	public new Camera camera;
	private float tileSize = 10;
	private SequenceTrainEngine engine;
	private GameObject[,] grid;
	private bool doTick = true;

	// Use this for initialization
	void Start () {
		if (camera == null){
			throw new InvalidOperationException("camera is null!");
		}
		camera.orthographic = true;
		EngineOptions options = new EngineOptions();
#if !DEBUG
		if (seed == 0){
			byte[] foo = BitConverter.GetBytes(UnityEngine.Random.value);
			byte[] bar = new byte[foo.Length * 2];
			foo.CopyTo(bar, 0);
			foo = BitConverter.GetBytes(UnityEngine.Random.value);
			foo.CopyTo(bar, foo.Length);
			seed = BitConverter.ToUInt64(bar, 0);
		}
#endif
		options.seed = seed;
		options.gridWidth = width;
		options.gridHeight = length;
		options.trainCanCrashWithSelf = trainCanCrashWithSelf;
		options.blockSections = blockSections;
		engine = new SequenceTrainEngine(options);
		grid = new GameObject[width, length];
		float cameraWidth = tileSize * width;
		float cameraHeight = tileSize * length;
		float newCameraSize;// = mainCamera.orthographicSize;
		if (camera.aspect > 1){
			newCameraSize = cameraHeight / 2;
		}
		else{
			newCameraSize = ((1 / camera.aspect) * cameraWidth) / 2;
		}
		camera.orthographicSize = newCameraSize;
		//fill the grid
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < length; y++) {
				GameObject item = generateTrackPiece(x, y);
				grid[x, y] = item;
			}
		}
		//place the camera
		Vector3 cameraPos = new Vector3(cameraWidth / 2, 10, cameraHeight / 2 - 5);
		camera.transform.position = cameraPos;
	}

	/// <summary>
	/// Create a GameObject for the TrackBlock at this location
	/// </summary>
	/// <returns>The track piece.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	private GameObject generateTrackPiece(int x, int y){
		GameObject factoryObject;
		AbstractTrackBlock trackBlock = engine[x, y];
		switch (trackBlock.getTrackType()){
			case TrackType.Curved:
				factoryObject = curved;
				break;
			case TrackType.Straight:
				factoryObject = straight;
				break;
			case TrackType.DoubleCurved:
				factoryObject = doubleCurved;
				break;
			case TrackType.DoubleStraight:
				factoryObject = doubleStraight;
				break;
			case TrackType.Station:
				throw new NotImplementedException("Creating Station blocks is " +
					"not implemented");
			default:
				throw new InvalidOperationException("Unknown track type: " +
					trackBlock.getTrackType());
		}
		GameObject newObject = UnityEngine.Object.Instantiate(factoryObject);
		placeItemInView(newObject, x, y);
		//rotate it so it matches the data backing.
		Rotater rotater = newObject.GetComponent<Rotater>();
		rotater.buildArray();
		rotater.parent = this;
		rotater.x = x;
		rotater.y = y;
		bool ok;
		TrackEnds[] rotatedArray = trackBlock.getTrackEnds();
//		StringBuilder logBuilder = new StringBuilder(100);
//		for(int i = 0; i < rotatedArray.Length; i++) {
//			if (i != 0){
//				logBuilder.Append(',');
//			}
//			logBuilder.Append(rotatedArray[i]);
//		}
//		Debug.Log(logBuilder);
//		Debug.Log(trackBlock.getTrackType());
		int offset = 0;
		do {
			ok = true;
			for (int i = 0; i < 4; i++) {
				if (rotatedArray[i] != rotater.ends[(i + offset) % 4]){
					ok = false;
					break;
				}
			}
			if (!ok){
				offset++;
				DoRotate(rotater, true, true);
//				Debug.Log("Roated " + newObject + " clockwize");
			}
		} while (offset < 4 && !ok);
		//I sure hope it is rotated correctly by now....
		rotater.rotationOffset = offset;
		return newObject;
	}

	void placeItemInView(GameObject item, int x, int y) {
		float newX = x * tileSize;
		float newZ = y * tileSize;
		Vector3 pos;
		Vector3 add = new Vector3(newX, 0, newZ);
		pos = add;
		item.transform.position = pos;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (engine == null || !doTick){
			return;
		}
		try{
			if (!engine.tick()){
				//nothing changed
				return;
			}
		}
		catch (GameOverException goe){
			doTick = false;
			if (GameOver != null){
				GameOver(this, new GameOverArgs(goe));
			}
		}
	}

	public class GameOverArgs : EventArgs{
		public readonly GameOverException e;

		public GameOverArgs(GameOverException e){
			this.e = e;
		}
	}

	public event EventHandler<GameOverArgs> GameOver;
	/// <summary>
	/// Rotate the given rotater.
	/// </summary>
	/// <param name="rotater">Rotater.</param>
	/// <param name="clockWize">If set to <c>true</c> clock wize.</param>
	/// <param name="init">If set to <c>true</c> we are rotating to the data
	/// backing, so don't rotate the data backing</param>
	private void DoRotate(Rotater rotater, bool clockWize, bool init){
		
		int sign;
		if (clockWize){
			sign = 1;
		}
		else{
			sign = -1;
		}
		rotater.transform.rotation *= Quaternion.AngleAxis(90 * sign, Vector3.up);
//		Debug.Log("Rotated " + rotater.x + ", " + rotater.y);
		if (!init){
			engine[rotater.x, rotater.y].rotate(clockWize);
		}
	}

	public void DoRotate(Rotater rotater, bool clockWize) {
		DoRotate(rotater, clockWize, false);
	}
}
