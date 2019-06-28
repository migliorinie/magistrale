using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Test_2_text : MonoBehaviour {
	
	[Space(10)]
	public applyShader leftEye;
	public applyShader rightEye;
	public Camera controlCam;
	public Text targetText;
	
	[Space(10)]
	public Language lang;
	[Tooltip("The amount of iterations FOR EACH RESOLUTION")]
	public int runsPerRes = 5;
	public int runsPerAngle = 20;
	//public int timeLimit = 5;
	
	[Space(10)]
	[Range(0.0f, 90.0f)]
	[Tooltip("VERTICAL maximum angle.")]
	public float fixedAngle = 3f; // in degrees
	
	[Space(10)]
	public string subject;
	
	private float deltaTime;
	private List<string> texts;
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

	void ScaleTextToAngle(Text t, string s, float f) {
		float targetHeight = Mathf.Abs(2f*Mathf.Sin(ToRad(f)/2f)*(GameObject.Find("ControlCam").transform.position - t.transform.position).magnitude);
		int targetSize = (int)Mathf.Min(Mathf.Floor(targetHeight*24f/13f), 64); // Figures are determined empirically
		t.fontSize = targetSize;
		//Note: the font size is apparently capped at 32
	}
	
	/*
	private IEnumerator Timeout(float seconds) {
		//while(true) {
			yield return new WaitForSeconds(seconds);
			timeout = true;
		//}
	}
	*/
	
	void Quit() {
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
	
	// Use this for initialization
	void Start () {
		
		Debug.Log("Ding-dong! Remember that you didn't change the angle calculations. Dong-ding!");
		
		fpsColl = new List<float>();
		Cursor.lockState = CursorLockMode.Locked;
		controlCam.aspect = 1.33333f;
		
		// Hardcoding? At this time of year, at this time of day,
		// in this part of the country, localized entirely within your MonoBehaviour?!
		string addr = "Assets/Custom/textsITA.txt";
		/*
		switch(lang) {
			case Language.ITA:
				addr = "Assets/Custom/commonITA.txt";
				break;
			case Language.FRA:
				addr = "Assets/Custom/commonFRA.txt";
				break;
			case Language.ENG:
				addr = "Assets/Custom/commonENG.txt";
				break;
		}
		*/
		
		texts = new List<string> (System.IO.File.ReadAllLines(addr));
		
		resolutions = new List<Vector2> {new Vector2(40, 60), new Vector2(60, 90), new Vector2(80, 120), new Vector2(120, 150)};
		int len = runsPerRes*runsPerAngle*resolutions.Count;
		
		runCount = 0;
		
		//Shuffle<string>(texts);
		texts = texts.GetRange(0, len);
		
		// This is just for reading in order.
		texts.Reverse();
		
		string s = Pop(texts);
		targetText.text = s;
		ScaleTextToAngle(targetText, s, fixedAngle);
		
		begin = System.DateTime.Now;
		answered = false;
		readyToStart = false;
		
		timestamp = begin.ToString("yyyy_MM_dd_HH_mm");
		sw = new StreamWriter(Application.dataPath + "/../Logs/Test_2_text/Test_2_" + timestamp + ".json");
		
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
				
				controlCam.transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "";
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
			
			// To change the resolution: get the script and add a counter in module. Possibly group texts under resolution.
			// HOWEVER, first I need some more props.
			if (((Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.N)) && answered)) {
				//begin = System.DateTime.Now;
				answered = false;
				// item
				sw.WriteLine("\t\t\t\t\t{");
				sw.WriteLine("\t\t\t\t\t\t\"name\" : \"" + targetText.text + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"result\" : \"" + (Input.GetKeyDown(KeyCode.Y) ? "CORRECT" : "WRONG") + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"elapsed\" : \"" + elapsed + "\",");
				sw.WriteLine("\t\t\t\t\t\t\"angle\" : \"" + fixedAngle + "\"");
				if ((runCount)%(runsPerRes*runsPerAngle) != 0) {
					sw.WriteLine("\t\t\t\t\t},");
				} else {
					sw.WriteLine("\t\t\t\t\t}");
				}
				elapsed = "";
				
				if(texts.Count > 0) {
					// Create a new item
					if((runCount)%(runsPerRes*runsPerAngle) == 0 && resolutions.Count > 0) {
						Debug.Log(currRes);
						Debug.Log(fpsColl.Average());
						fpsColl = new List<float>();
						
						currRes = Pop(resolutions);
						if (leftEye != null && rightEye != null) {
							leftEye.SetResolution(currRes);
							rightEye.SetResolution(currRes);
						}
						fixedAngle = 3f;
						// runs
						sw.WriteLine("\t\t\t\t]");
						// sets
						sw.WriteLine("\t\t\t},");
						// set
						sw.WriteLine("\t\t\t{");
						sw.WriteLine("\t\t\t\t\"resolution\" : \"" + currRes.x.ToString() + "/" + currRes.y.ToString() + "\",");
						sw.WriteLine("\t\t\t\t\"runs\" : [");
					}
					else if((runCount)%runsPerAngle == 0) {
						fixedAngle += 1;
					}
					Debug.Log(runCount);
					runCount++;
					
					readyToStart = false;
					controlCam.transform.Find("Canvas").Find("Text").GetComponent<Text>().text = "Press Space to start";
					
					string s = Pop(texts);
					targetText.text = s;
					ScaleTextToAngle(targetText, s, fixedAngle);
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
		
		//System.Diagnostics.Process.Start(Application.dataPath + "/../Logs/Test_2_text/Test_2_" + timestamp + ".json");
	}
}
