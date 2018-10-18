using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Corridor_spawning : MonoBehaviour {
	
	public Transform obstacle;
	public Transform floor;
	public Transform controller;
	
	private List<Vector3> coords;
	private StreamWriter sw;
	private string lastpos;
	private string datetime;
	
	// Hardcoded. Will generalize if it's worth to work on it.
	void Start () {
		
		coords = new List<Vector3>();
		int limit = (int)(floor.GetComponent<Renderer>().bounds.size.z/2);
		Debug.Log(limit);
		for (int i = -1*(limit-2); i<=(limit-4); i+=2) {
			// To make it harder, just take the last three.
			int spin = (int)Mathf.Floor(Random.value*5.99f);
			switch (spin) {
				case 0:
				coords.Add(new Vector3(-1.0f, 0.445f, (float)i));
				break;
				case 1:
				coords.Add(new Vector3(0.0f, 0.445f, (float)i));
				break;				
				case 2:
				coords.Add(new Vector3(1.0f, 0.445f, (float)i));
				break;				
				case 3:
				coords.Add(new Vector3(-1.0f, 0.445f, (float)i));
				coords.Add(new Vector3(0.0f, 0.445f, (float)i));
				break;				
				case 4:
				coords.Add(new Vector3(-1.0f, 0.445f, (float)i));
				coords.Add(new Vector3(1.0f, 0.445f, (float)i));
				break;				
				case 5:
				coords.Add(new Vector3(0.0f, 0.445f, (float)i));
				coords.Add(new Vector3(1.0f, 0.445f, (float)i));
				break;
			}
		}
		
		datetime = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
		sw = new StreamWriter(Application.dataPath + "/../Logs/CorridorLog" + datetime + ".txt");
		sw.WriteLine("<BUMPS>");
		foreach (Vector3 vec in coords) {
			Instantiate(obstacle, vec, Quaternion.identity);
			sw.WriteLine(vec.ToString());
		}
		sw.WriteLine("<STEPS>");
	}
	
	// Update is called once per frame
	void Update () {
		string tmp = controller.position.x.ToString("F2") + ";" + controller.position.z.ToString("F2");
		if (tmp != lastpos) {
			lastpos = tmp;
			sw.WriteLine(lastpos);
		}
	}
	
	void OnApplicationQuit()
    {
        sw.Close();
		Vector2 floorSize = new Vector2(floor.GetComponent<Renderer>().bounds.size.x, floor.GetComponent<Renderer>().bounds.size.z);
		int scaleFactor = 128;

		Vector2 bumpSize = new Vector2(obstacle.transform.Find("Top").GetComponent<Renderer>().bounds.size.x, obstacle.transform.Find("Top").GetComponent<Renderer>().bounds.size.z);
		Texture2D outText = new Texture2D((int)floorSize.x*scaleFactor, (int)floorSize.y*scaleFactor);
		for (int i = 0; i<floorSize.x*scaleFactor; i++) {
			for (int j = 0; j<floorSize.y*scaleFactor; j++) {
				outText.SetPixel(i, j, Color.white);
			}
		}
		foreach (Vector3 vec in coords) {
			Vector2 corner = new Vector2((floorSize.x/2 + vec.x - bumpSize.x/2)*scaleFactor, (floorSize.y/2 + vec.z)*scaleFactor);
			for (int i = 0; i<bumpSize.x*scaleFactor; i++) {
				for (int j = 0; j<bumpSize.y*scaleFactor; j++) {
					outText.SetPixel((int)(corner.x + i), (int)(corner.y + j), Color.black);
				}
			}
		}
		
		StreamReader sr = new StreamReader(Application.dataPath + "/../Logs/CorridorLog" + datetime + ".txt");
		
		while (sr.ReadLine() != "<STEPS>") {}
		string s = sr.ReadLine();
		while (s != null) {
			string[] tmpArr = s.Split(';');
			Vector2 vec = new Vector2(float.Parse(tmpArr[0]), float.Parse(tmpArr[1]));
			
			float stepSize = 0.5f;
			Vector2 corner = new Vector2((floorSize.x/2 + vec.x - stepSize/2)*scaleFactor, (floorSize.y/2 + vec.y - stepSize/2)*scaleFactor);
			//Debug.Log(vec);
			//Debug.Log(corner);
			//Debug.Log("--------");
			for (int i = 0; i<stepSize*scaleFactor; i++) {
				for (int j = 0; j<stepSize*scaleFactor; j++) {
					if ((int)(corner.x+i) < (int)floorSize.x*scaleFactor && (int)(corner.x+i) < (int)floorSize.y*scaleFactor) {
						outText.SetPixel((int)(corner.x + i), (int)(corner.y + j), Color.red);
					}
				}
			}
			s = sr.ReadLine();
		}
		outText.Apply();
		
		byte[] bytes = outText.EncodeToPNG();
		File.WriteAllBytes(Application.dataPath + "/../Logs/CorridorTrace" + datetime + ".png", bytes);
    }

}
