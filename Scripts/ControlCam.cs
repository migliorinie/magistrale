using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlCam : MonoBehaviour {
	
	private Transform toBeFound;
	
	public void SetTarget(Transform t) {
		toBeFound = Instantiate(t, t.position, t.rotation);
		toBeFound.parent = this.transform;
		toBeFound.localPosition = Vector3.zero;
		ScaleChildToAngle(35.0f);
		toBeFound.rotation = Quaternion.Euler(toBeFound.rotation.eulerAngles + new Vector3(0, 90, 0));
		this.transform.Find("Canvas").Find("upper").GetComponent<Text>().text = "Target: " + toBeFound.name.Split('(')[0];
		// Now that it's positioned, we can detach and rotate
		toBeFound.parent = null;
		}
	
	public void Clear() {
		Destroy(toBeFound.gameObject);
	}
	
	// Utility functions for dealing with Vector3
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
	
	void ScaleChildToAngle(float f) {
		float diagonal = GetBounds(toBeFound).size.magnitude;
		float target = Mathf.Abs(2f*Mathf.Sin(ToRad(f)/2f)); // The desired distance is 1
		Bounds origBounds = GetBounds(toBeFound);
		toBeFound.localScale = toBeFound.localScale*(target/diagonal);
		
		toBeFound.localPosition = ElemwiseProduct(
									ElemwiseProduct(origBounds.center-this.transform.position, toBeFound.localScale),
								  new Vector3(-1, -1, 1)) + Vector3.forward;
								  
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
