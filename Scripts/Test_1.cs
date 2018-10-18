using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Test_1 : MonoBehaviour {
	
	public Transform ac_guitar;
	public Transform axe;
	public Transform banana;
	public Transform bottle;
	public Transform cat;
	public Transform chef_knife;
	public Transform cherry;
	public Transform coffee_mug;
	public Transform coffee_table;
	public Transform cola;
	public Transform dog;
	public Transform door;
	public Transform ele_guitar;
	public Transform extinguisher;
	public Transform fan;
	public Transform fedora;
	public Transform fish;
	public Transform hammer;
	public Transform heart;
	public Transform horse;
	public Transform jug;
	public Transform kitchen_chair;
	public Transform kitchen_table;
	public Transform lamp;
	public Transform laptop;
	public Transform lotus_flower;
	public Transform microwave;
	public Transform moka;
	public Transform mushroom;
	public Transform pan;
	public Transform pear;
	public Transform pinapple;
	public Transform pliers;
	public Transform pot_small;
	public Transform printer;
	public Transform pumpkin;
	public Transform rose;
	public Transform scissors;
	public Transform shoe;
	public Transform star;
	public Transform teacup;
	public Transform teddy_bear;
	public Transform traffic_cone;
	public Transform tree;
	public Transform trumpet;
	public Transform wine_flask;
	public Transform working_boot;
	public Transform wrench;
	
	[Space(10)]
	public Transform pivot;
	public applyShader leftEye;
	public applyShader rightEye;
	public Camera controlCam;
	
	[Space(10)]
	[Range(0.0f, 90.0f)]
	public float fixedAngle = 20f; // in degrees
	
	[Space(10)]
	public string subject;
	
	private List<Transform> items;
	private Transform instantiated;
	private int runsPerRes;
	private int runCount;
	private List<Vector2> resolutions;
	private Vector2 currRes;
	
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

	void ScaleToAngle(Transform t, float f) {
		float diagonal = GetBounds(t).size.magnitude;
		float target = Mathf.Abs(2f*Mathf.Sin(ToRad(f)/2f)*pivot.Find("Fove Interface").transform.position.magnitude);
		Bounds origBounds = GetBounds(t);
		t.localScale = t.localScale*(target/diagonal);
		t.position = ElemwiseProduct(origBounds.center, t.localScale)*(-1f);
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
		Cursor.lockState = CursorLockMode.Locked;
		controlCam.aspect = 1.33333f;
		
		// Hardcoding? At this time of year, at this time of day, at this part of the country, localized entirely within your MonoBehaviour?!
		items = new List<Transform> {ac_guitar, axe, banana, bottle, cat, chef_knife, cherry, coffee_mug, coffee_table, cola, dog, door,
									ele_guitar, extinguisher, fan, fedora, fish, hammer, heart, horse, jug, kitchen_chair, kitchen_table, lamp,
									laptop, lotus_flower, microwave, moka, mushroom, pan, pear, pinapple, pliers, pot_small, printer, pumpkin,
									rose, scissors, shoe, star, teacup, teddy_bear, traffic_cone, tree, trumpet, wine_flask, working_boot, wrench};
		resolutions = new List<Vector2> {new Vector2(40, 60), new Vector2(60, 90), new Vector2(80, 120), new Vector2(120, 150)};
		runsPerRes = items.Count/resolutions.Count;
		runCount = 0;
		
		Shuffle<Transform>(items);
		Transform t = Pop(items);
		instantiated = Instantiate(t, new Vector3(0.0f, 0.0f, 0.0f), t.rotation);
		ScaleToAngle(instantiated, fixedAngle);
		
		begin = System.DateTime.Now;
		answered = false;
		readyToStart = false;
		
		timestamp = begin.ToString("yyyy_MM_dd_HH_mm");
		sw = new StreamWriter(Application.dataPath + "/../Logs/Test_1/Test_1_" + timestamp + ".json");
		
		// C#'s JSON serializer is just unpleasant. I'll analyze this data with Python anyways
		sw.WriteLine("{");
		sw.WriteLine("\t\"test\": {");
		sw.WriteLine("\t\t\"subject\" : \"" + subject + "\",");
		sw.WriteLine("\t\t\"date\" : \"" + begin.ToString("yyyy-MM-dd") + "\",");
		sw.WriteLine("\t\t\"time\" : \"" + begin.ToString("HH:mm:ss") + "\",");
		sw.WriteLine("\t\t\"canny\" : " + ((leftEye.canny && rightEye.canny)? "1" : "0") + ",");
		sw.WriteLine("\t\t\"experiments\" : [");
	}
	
	// Update is called once per frame
	void Update () {
		
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
				
				controlCam.transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "";
			}
		} else {
			// "Answer button". Can compare to zero because of dead zone.
			if (Input.GetAxis("Confirm") != 0 && !answered) {
				answered = true;
				end = System.DateTime.Now;
				elapsed = (end - begin).ToString();
				string[] tmp = elapsed.Split(':');
				elapsed = tmp[1] + ":" + tmp[2].Substring(0, 6);
				controlCam.transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "Press Y if CORRECT,\nN if WRONG";
				
				leftEye.SetBlackening(true);
				rightEye.SetBlackening(true);
			}
			
			// To change the resolution: get the script and add a counter in module. Possibly group items under resolution.
			// HOWEVER, first I need some more props.
			if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.N)) {
				//begin = System.DateTime.Now;
				answered = false;
				// item
				sw.WriteLine("\t\t\t\t\t{");
				sw.WriteLine("\t\t\t\t\t\t\"name\" : \"" + instantiated.name.Split('(')[0] + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"result\" : \"" + (Input.GetKeyDown(KeyCode.Y) ? "CORRECT" : "WRONG") + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"elapsed\" : \"" + elapsed + "\"");
				if ((runCount)%runsPerRes != 0) {
					sw.WriteLine("\t\t\t\t\t},");
				} else {
					sw.WriteLine("\t\t\t\t\t}");
				}
				elapsed = "";
				
				if(items.Count > 0) {
					// Create a new item
					if((runCount)%runsPerRes == 0 && resolutions.Count > 0) {
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
					runCount++;
					
					readyToStart = false;
					controlCam.transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "Press Space to start";
					
					Destroy(instantiated.gameObject);
					Transform t = Pop(items);
					instantiated = Instantiate(t, new Vector3(0.0f, 0.0f, 0.0f), t.rotation);
					ScaleToAngle(instantiated, fixedAngle);
					pivot.rotation = Quaternion.identity;
				} else {
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
		
		System.Diagnostics.Process.Start(Application.dataPath + "/../Logs/Test_1/Test_1_" + timestamp + ".json");
	}
}
