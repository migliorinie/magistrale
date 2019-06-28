using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Test_3_Surface : MonoBehaviour {
	
	public Transform banana;
	public Transform chef_knife;
	public Transform coffee_mug;
	public Transform cola;
	public Transform fish;
	public Transform glasses;
	public Transform lightbulb;
	public Transform lotus_flower;
	public Transform moka;
	public Transform mushroom;
	public Transform palette;
	public Transform photo_camera;
	public Transform pliers;
	public Transform rose;
	public Transform scissors;
	public Transform teacup;
	public Transform tie;
	
	[Space(10)]
	public Transform axe;
	public Transform bottle;
	public Transform cat;
	public Transform extinguisher;
	public Transform fedora;
	public Transform hammer;
	public Transform jug;
	public Transform lamp;
	public Transform shoe;
	public Transform skull;
	public Transform teddy_bear;
	public Transform violin;
	public Transform working_boots;

	[Space(10)]
	public Transform coffee_table;
	public Transform dog;
	public Transform fan;
	public Transform jacket;
	public Transform jeans;
	public Transform laptop;
	public Transform microwave;
	public Transform pot_small;
	public Transform printer;
	public Transform pumpkin;
	public Transform soccer_ball;
	public Transform traffic_cone;
	public Transform trumpet;
	public Transform wine_flask;

	[Space(10)]
	public Transform marksphere;
	
	[Space(10)]
	public applyShader leftEye;
	public applyShader rightEye;
	public Camera viewCam;
	public ControlCam controlCam;
	
	[Space(10)]
	public int rows = 3;
	public int columns = 4;
	[Range(0, 360)]
	public int arcWidth = 120;
	
	[Space(10)]
	public int runsPerRes = 5;
	public string subject;
	
	private float deltaTime;
	private List<Transform> lightItems;
	private List<Transform> oneHandedItems;
	private List<Transform> twoHandedItems;
	private List<Transform> instantiated;
	private Transform targ;
	
	private int len;
	private int runCount;
	private List<Vector2> resolutions;
	private Vector2 currRes;
	//private bool timeout;
	//private IEnumerator timer;
	
	private List<float> fpsColl;
	
	private StreamWriter sw;
	
	private System.DateTime begin;
	private System.DateTime end;
	private string timestamp;
	private string elapsed;
	private bool answered;
	private bool readyToStart;
	
	// Utility functions for dealing with Vector3s
	// To make it prettier I could save these statically somewhere, or have these classes inherit something.
	public Vector3 ElemwiseProduct(Vector3 first, Vector3 second) {
		return new Vector3(first.x*second.x, first.y*second.y, first.z*second.z);
	}

	public Vector3 Absolute(Vector3 input) {
		return new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
	}
	
	float ToRad(float deg) {
		return deg/180*3.14159f;
	}
	
	// In-place swap elements in a list
	public void Swap<T>(List<T> list, int indexA, int indexB) {
		T tmp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = tmp;
	}

	// Fisher-Yates (Durstenfeld-Knuth) in-place shuffle
	public void Shuffle<T>(List<T> inp) {
		int elems = inp.Count;
		for (int i = elems-1; i>0; i--) {
			int j = (int)Mathf.Floor(Random.value*(i+1));
			Swap<T>(inp, i, j);
		}
	}
	
	public T Pop<T>(List<T> inp) {
		int i = inp.Count-1;
		if(i >= 0) {
			T r = inp[i];
			inp.RemoveAt(i);
			return r;
		} else {
			return default(T);
		}
	}
	
	/*
	private IEnumerator Timeout(float seconds) {
		//while(true) {
			yield return new WaitForSeconds(seconds);
			timeout = true;
		//}
	}
	*/
	
	Bounds GetBounds(Transform obj) {
		Renderer renderer = obj.GetComponent<Renderer>();
		Bounds combinedBounds;
		if(renderer != null) {// If the object has a renderer, it's not a group.
			combinedBounds = renderer.bounds;
		}
		else {
			combinedBounds = new Bounds(obj.position, new Vector3(0, 0, 0));
		}
		// There are objects with a renderer whose children are objects with a renderer
		foreach (Transform child in obj.transform) {
			combinedBounds.Encapsulate(GetBounds(child));
		}
		return combinedBounds;
	}
	
	void ScaleToAngle(Transform t, float f, Vector3 dest) {
		float diagonal = GetBounds(t).size.magnitude;
		float target = Mathf.Abs(2f*Mathf.Sin(ToRad(f)/2f)*dest.magnitude);
		Bounds origBounds = GetBounds(t);
		t.localScale = t.localScale*(target/diagonal);
		t.position = ElemwiseProduct(origBounds.center, t.localScale)*(-1f);
	}
	
	Transform CreateObjects() {
		int cat = (int)Mathf.Floor(Random.value*(3f));
		List<Transform> itlist;
		switch(cat) {
		case 0: {
			itlist = lightItems;
			break;}
		case 1:{
			itlist = oneHandedItems;
			break;}
		case 2:{
			itlist = twoHandedItems;
			break;}
		default:{
			Debug.Log("Error in the random values!");
			itlist = new List<Transform>();
			break;}
		}
		itlist = itlist.GetRange(0, rows*columns);
		
		// Set up the list of targeted
		Transform target = itlist[(int)Mathf.Floor(Random.value*(rows*columns))];
		Shuffle(itlist);
		
		float arc = arcWidth/(columns+1);
		for(int i = 0; i < rows*columns; i++) {
			Vector3 pos = new Vector3(Mathf.Cos(ToRad((270f - (360f-arcWidth)/2) - arc*(i%columns+1))), Mathf.Sin(ToRad(15f*((i/columns)-(rows/2)))), Mathf.Sin(ToRad((270f - (360f-arcWidth)/2) - arc*(i%columns+1))))*6;
			Transform inst = Instantiate(itlist[i]);
			ScaleToAngle(inst, 15f, pos);
			inst.position = pos;
			instantiated.Add(inst);
		}
		
		return target;
	}
	
	void Quit() {
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
	
	// Use this for initialization
	void Start () {
		
		Debug.Log("---------------");
		Debug.Log("You need to add a controlCam like in Kitchen and Living");
		Debug.Log("---------------");
		
		fpsColl = new List<float>();
		Cursor.lockState = CursorLockMode.Locked;
		viewCam.aspect = 1.33333f;
		
		lightItems = new List<Transform>{banana, photo_camera, chef_knife, coffee_mug, cola, fish, glasses, lightbulb, lotus_flower, moka, mushroom, palette, pliers, rose, scissors, teacup, tie};
		oneHandedItems = new List<Transform>{axe, bottle, cat, extinguisher, fedora, hammer, jug, lamp, shoe, skull, teddy_bear, violin, working_boots};
		twoHandedItems = new List<Transform>{coffee_table, dog, fan, jacket, jeans, laptop, microwave, pot_small, printer, pumpkin, soccer_ball, traffic_cone, trumpet, wine_flask};
		instantiated = new List<Transform>();
		
		resolutions = new List<Vector2> {new Vector2(40, 60), new Vector2(60, 90), new Vector2(80, 120), new Vector2(120, 150)};
		len = runsPerRes*resolutions.Count;
		if (lightItems.Count < rows*columns || oneHandedItems.Count < rows*columns || twoHandedItems.Count < rows*columns) {
			Debug.Log("Error! All the objects lists must have more than " + (rows*columns).ToString() + " objects!");
			Quit();
		}
		
		runCount = 0;
		
		targ = CreateObjects();
		controlCam.SetTarget(targ);
		
		begin = System.DateTime.Now;
		answered = false;
		readyToStart = false;
		
		timestamp = begin.ToString("yyyy_MM_dd_HH_mm");
		sw = new StreamWriter(Application.dataPath + "/../Logs/Test_3_Surface/Test_3_Surface_" + timestamp + ".json");
		
		// C#'s JSON serializer is just unpleasant. I'll analyze this data with Python anyways
		sw.WriteLine("{");
		sw.WriteLine("\t\"test\": {");
		sw.WriteLine("\t\t\"subject\" : \"" + subject + "\",");
		sw.WriteLine("\t\t\"date\" : \"" + begin.ToString("yyyy-MM-dd") + "\",");
		sw.WriteLine("\t\t\"time\" : \"" + begin.ToString("HH:mm:ss") + "\",");
		sw.WriteLine("\t\t\"canny\" : " + ((leftEye.canny && rightEye.canny)? "1" : "0") + ",");
		sw.WriteLine("\t\t\"noisy\" : " + ((leftEye.brokenChance > float.Epsilon && rightEye.brokenChance > float.Epsilon)? "1" : "0") + ",");
		sw.WriteLine("\t\t\"experiments\" : [");
	}
	
	// Update is called once per frame
	void Update () {
		if(!answered) {
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			float fps = 1.0f / deltaTime;
			//Debug.Log(fps);
			fpsColl.Add(fps);
		}
		
		// Can't do it at start or I'd have a race condition
		if (runCount == 0) {
			currRes = Pop(resolutions);
			if (leftEye != null && rightEye != null) {
				leftEye.SetResolution(currRes);
				rightEye.SetResolution(currRes);
			}
			// sets
			sw.WriteLine("\t\t\t{");
			sw.WriteLine("\t\t\t\t\"resolution\" : \"" + currRes.x.ToString() + "/" + currRes.y.ToString() + "\",");
			sw.WriteLine("\t\t\t\t\"runs\" : [");
			runCount++;
			// not ready to start yet
			leftEye.SetBlackening(true);
			rightEye.SetBlackening(true);
		}
		
		if (!readyToStart) {
			if (Input.GetKeyDown(KeyCode.Space)) {
				begin = System.DateTime.Now;
				readyToStart = true;
				leftEye.SetBlackening(false);
				rightEye.SetBlackening(false);
				
				//timeout = false;
				//timer = Timeout((float)timeLimit);
				//StartCoroutine(timer);
				
				controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "";
			}
		} else {
			// Time's up
			/*
			if(timeout && !answered) {
				end = System.DateTime.Now;
				elapsed = (end - begin).ToString();
				string[] tmp = elapsed.Split(':');
				elapsed = tmp[1] + ":" + tmp[2].Substring(0, 6);
				
				leftEye.SetBlackening(true);
				rightEye.SetBlackening(true);
				
				//StopCoroutine(timer);
			}*/
			foreach (Transform item in instantiated) {
				item.rotation = Quaternion.Euler(item.rotation.eulerAngles + new Vector3(0, 10, 0));
			}
			
			// "Answer button". Can compare to zero because of dead zone.
			if (Input.GetAxis("Confirm") != 0 && !answered) {
				answered = true;
				end = System.DateTime.Now;
				elapsed = (end - begin).ToString();
				string[] tmp = elapsed.Split(':');
				elapsed = tmp[1] + ":" + tmp[2].Substring(0, 6);
				controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Y if CORRECT,\nN if WRONG";
				
				leftEye.SetBlackening(true);
				rightEye.SetBlackening(true);
			}
			
			// To change the resolution: get the script and add a counter in module. Possibly group items under resolution.
			// HOWEVER, first I need some more props.
			if (((Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.N)) && answered)) {
				//begin = System.DateTime.Now;
				answered = false;
				// item
				sw.WriteLine("\t\t\t\t\t{");
				sw.WriteLine("\t\t\t\t\t\t\"name\" : \"" + targ.name.Split('(')[0] + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"result\" : \"" + (Input.GetKeyDown(KeyCode.Y) ? "CORRECT" : "WRONG") + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"elapsed\" : \"" + elapsed + "\",");
				if ((runCount)%(runsPerRes) != 0) {
					sw.WriteLine("\t\t\t\t\t},");
				} else {
					sw.WriteLine("\t\t\t\t\t}");
				}
				elapsed = "";
				
				if(runCount < len) {
					// Create a new item
					foreach (Transform inst in instantiated) {
						Destroy(inst.gameObject);
						controlCam.Clear();
					}
					instantiated.Clear();
					targ = CreateObjects();
					controlCam.SetTarget(targ);
		
					if((runCount)%(runsPerRes) == 0 && resolutions.Count > 0) {
						Debug.Log(currRes);
						Debug.Log(fpsColl.Average());
						fpsColl = new List<float>();
						
						currRes = Pop(resolutions);
						if (leftEye != null && rightEye != null) {
							leftEye.SetResolution(currRes);
							rightEye.SetResolution(currRes);
						}
						// runs
						sw.WriteLine("\t\t\t\t]");
						// sets
						sw.WriteLine("\t\t\t},");
						// set
						sw.WriteLine("\t\t\t{");
						sw.WriteLine("\t\t\t\t\"resolution\" : \"" + currRes.x.ToString() + "/" + currRes.y.ToString() + "\",");
						sw.WriteLine("\t\t\t\t\"runs\" : [");
					}
					Debug.Log(runCount);
					runCount++;
					
					readyToStart = false;
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Space to start";
					
				} else {
					
					Debug.Log(currRes);
					Debug.Log(fpsColl.Average());
					fpsColl = new List<float>();
					Quit();
				}
			}
		}
	}
	
	// In case of accidental exit, properly closes JSON
	void OnApplicationQuit() {
		// runs
		sw.WriteLine("\t\t\t\t]");
		// set
		sw.WriteLine("\t\t\t}");
		// experiments
		sw.WriteLine("\t\t]");
		// test
		sw.WriteLine("\t}");
		// main object
		sw.WriteLine("}");
		sw.Close();
		
		//System.Diagnostics.Process.Start(Application.dataPath + "/../Logs/Test_3_Surface/Test_3_Surface_" + timestamp + ".json");
	}
}
