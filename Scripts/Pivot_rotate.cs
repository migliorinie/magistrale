using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlScheme {
	MOUSE,
	KEYBOARD,
	GAMEPAD
}

public class Pivot_rotate : MonoBehaviour {
	
	public ControlScheme control = ControlScheme.MOUSE;
	public bool active = true;
	// Use this for initialization
	void Start () {

	}
	// Update is called once per frame
	void Update () {
		if(active) {
			Vector3 input = new Vector3(0.0f, 0.0f, 0.0f);
			switch (control) {
				case ControlScheme.MOUSE: {
					input = new Vector3 {x = -10f*Input.GetAxis("Mouse Y"),
										 y = 10f*Input.GetAxis("Mouse X"),
										 z = 100f*Input.GetAxis("Mouse ScrollWheel")};
					break;
				}
				
				case ControlScheme.KEYBOARD: {
					// Vertical means rotation across the X axis, same for Horizontal
					input = new Vector3 {x = -10f*Input.GetAxis("Vertical"),
										 y = 10f*Input.GetAxis("Horizontal"),
										 z = 10f*Input.GetAxis("Orthogonal")};
					// The camera is tethered to an empty transform in the center, so there is no need to compute rotations.
					// Rotate allows me to rotate on the view's axis, rather than the world axes.
					break;
				}
				case ControlScheme.GAMEPAD: {
					// Vertical means rotation across the X axis, same for Horizontal
					input = new Vector3 {x = -20f*Input.GetAxis("Vertical"),
										 y = 20f*Input.GetAxis("Horizontal"),
										 z = 20f*Input.GetAxis("Orthogonal")};
					// The camera is tethered to an empty transform in the center, so there is no need to compute rotations.
					// Rotate allows me to rotate on the view's axis, rather than the world axes.
					break;
				}
			}
			this.transform.Rotate(input);
		}
	}
}
