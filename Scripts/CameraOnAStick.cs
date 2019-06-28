using System;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityStandardAssets.CrossPlatformInput;

public class CameraOnAStick : MonoBehaviour
{
	public Camera FOVEInterface;
	public Transform leftEye;
	public Transform rightEye;
	public Transform cCam;
	
	public Vector3 adjustment;
	
	static CameraOnAStick instance;
 
	void Awake()
	{
		if(instance == null)
		{    
			instance = this; // In first scene, make us the singleton.
			DontDestroyOnLoad(gameObject);
		}
		else if(instance != this) {
			Destroy(gameObject); // On reload, singleton already set, so destroy duplicate.
		}
	}
	
	public static CameraOnAStick GetInstance(){
		return instance;
	}

	private void Start()
	{			
		DontDestroyOnLoad(this.gameObject);
		
		adjustment = new Vector3(0.0f, 0.0f, 0.0f);

    }

	private void FixedUpdate()
	{
		float input = GetInput();
		
		//this.transform.rotation = Quaternion.Euler(new Vector3(fib.GetHeadRotation().eulerAngles.x, fib.GetHeadRotation().eulerAngles.y, 0));
		//this.transform.rotation = fib.GetHeadRotation();
		
		if (Mathf.Abs(input) > float.Epsilon)
		{
			// If the collider doesn't work, a small raycast at a short distance can stop the movement
			
			// Does the ray intersect any objects excluding the player layer
			if (input < 0.0f || !(Physics.Raycast(transform.position, FOVEInterface.transform.TransformDirection(Vector3.forward), adjustment.z))) {
				adjustment.z += 0.045f*input;
				adjustment = Vector3.Max(new Vector3(0.0f, 0.0f, 0.0f), adjustment);
			}
		}

		RaycastHit hit;
		if (Physics.Raycast(transform.position, FOVEInterface.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
		{
			//FOVEInterface.transform.localPosition = Vector3.Min(adjustment, new Vector3(0.0f, 0.0f, Vector3.Distance(transform.position, hit.point) - 0.1f));
			leftEye.localPosition = Vector3.Min(adjustment, new Vector3(0.0f, 0.0f, Vector3.Distance(transform.position, hit.point) - 0.1f));
			rightEye.localPosition = Vector3.Min(adjustment, new Vector3(0.0f, 0.0f, Vector3.Distance(transform.position, hit.point) - 0.1f));
			cCam.localPosition = Vector3.Min(adjustment, new Vector3(0.0f, 0.0f, Vector3.Distance(transform.position, hit.point) - 0.1f));
		}
	}


	private float GetInput()
	{
		return CrossPlatformInputManager.GetAxis("Orthogonal");
	}
}