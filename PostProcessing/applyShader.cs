using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LeftOrRight
	{
		Left,
		Right
	}
public enum TrackMode
	{
		Camera,
		ViewArea
	}

[ExecuteInEditMode]
public class applyShader : MonoBehaviour {
	
	// pix_ prefix for pixel measurements, flt_ for floats
	[Tooltip("Use the shaders")]
	public bool shaders = true;
	[Tooltip("Use the pseudo-Canny edge detection")]
	public bool canny = true;
	
	[Range(0.0f, 90.0f)]
	public float viewingAngle = 46.3f; // in degrees
	public int electrodeDiameter = 80; // in microns
	public int electrodeDistance = 150;
	
	[Range(2u, 16u)]
	public int greyLevels = 2;
	
	[Tooltip("Used with pseudo-Canny edge detection, leads to white on black")]
	public bool invertColors = true;
	
	//public int fps = 60;
	
	[Range(0.0f, 1.0f)]
	[Tooltip("Fraction of broken electrodes")]
	public float brokenChance = 0.1f;
	
	[Range(0.0f, 1.0f)]
	[Tooltip("How much the luminosity can randomly dim")]
	public float luminVariance = 0.5f;
	[Tooltip("Generate a new noise profile every cycle")]
	public bool dynamicDisturbance = true;
	
	[Range(0.0f, 1.0f)]
	[Tooltip("How much the electrode dimension can vary. Fork RADIUS")]
	public float dimensionVariance = 0.3f;
	
	[Range(0.0f, 1.0f)]
	[Tooltip("Fraction of electrodes that activate randomly")]
	public float randomActivation = 0.1f;
	
	[Tooltip("Set to 1 to disable pulsed light")]
	public int pulseCycle = 4;
	
	public FoveInterfaceBase foveInterface;
	public LeftOrRight whichEye;
	
	public TrackMode trackingMode;
	
	//---Privates---//
	private Material mat_main;
	private Material mat_canny;
	
	private float flt_scaleFactor;
	private float viewRadius;
	
	private int fCount;
	
	private bool blacken = false;
	
	private Camera cam;
	
	float ToRad(float deg) {
		return deg/180*3.14159f;
	}
	
	public void SetResolution(Vector2 res) {
		mat_main.SetFloat("_electrode_radius", flt_scaleFactor*(res.x/(viewRadius*2)));
		mat_main.SetFloat("_electrode_distance", flt_scaleFactor*(res.y/viewRadius));
	}
	
	public void SetBlackening(bool b) {
		blacken = b;
	}
	
	public void PrintElectrodeNumber(float hexSide, float eleSide) {
		float N = hexSide/eleSide;
		Debug.Log("The lower bound of electrodes is " + (3*N*N + 3*N + 1).ToString());
	}
	
	void Start () {
		
		bool cannyInvert = invertColors;
		// Canny already puts background to back, so there's no need to invert colors.
		if(canny) {
			invertColors = false;
		}
		
		// Screen does not get the FOVE screen, but the editor's one.
		cam = this.GetComponent<Camera>();
		
		fCount = 0;
		if (pulseCycle < 1) {
			pulseCycle = 1;
		}
		
		float tw = 1.0f/2560f; // This should be Screen.width if I could make it get the actual FOVE screen;
		
		// The focal length of the eye in microns.
		viewRadius = 16533.0f;
		
		flt_scaleFactor = 0.5f/(Mathf.Sin(ToRad(cam.fieldOfView/2)));
		
		/*
		float pix_areaRadius;
		float pix_electrodeRadius;
		float pix_electrodeDistance;
		*/
		
		float flt_areaRadius;
		float flt_electrodeRadius;
		float flt_electrodeDistance;
		
		// Radius of the central part over the radius of the viewfield is the sin of the underlying angle.
		/*
		pix_areaRadius = pix_scaleFactor*(areaRadius/viewRadius);
		pix_electrodeRadius = pix_scaleFactor*(electrodeDiameter/(viewRadius*2));
		pix_electrodeDistance = pix_scaleFactor*(electrodeDistance/viewRadius);
		*/
		
		flt_areaRadius = flt_scaleFactor*(Mathf.Sin(ToRad(viewingAngle/2)));
		flt_electrodeRadius = flt_scaleFactor*(electrodeDiameter/(viewRadius*2));
		flt_electrodeDistance = flt_scaleFactor*(electrodeDistance/viewRadius);
		
		//PrintElectrodeNumber(6350, electrodeDistance);
		//PrintElectrodeNumber(flt_areaRadius, flt_electrodeDistance);
		
		mat_main = new Material(Shader.Find("Hidden/init_PP"));
		mat_main.SetFloat("_texelw", tw);
		mat_main.SetFloat("_centerx", 0.5f);
		mat_main.SetFloat("_centery", 0.5f);
		mat_main.SetFloat("_area_radius", flt_areaRadius);
		mat_main.SetFloat("_electrode_radius", flt_electrodeRadius);
		mat_main.SetFloat("_electrode_distance", flt_electrodeDistance);
		mat_main.SetInt("_levels", greyLevels);
		mat_main.SetInt("_invert", invertColors ? 1 : 0);
		
		mat_main.SetFloat("_dimension_variance", dimensionVariance);
		mat_main.SetFloat("_lumin_variance", luminVariance);
		mat_main.SetFloat("_broken_chance", brokenChance);
		mat_main.SetFloat("_random_activation", randomActivation);
		mat_main.SetFloat("_seed_main", Random.value);
		mat_main.SetFloat("_seed_mask", Random.value);
		mat_main.SetInt("_is_black", 0);
		
		mat_canny = new Material(Shader.Find("Hidden/canny"));
		mat_canny.SetFloat("_texelw", tw);
		mat_canny.SetInt("_invert", cannyInvert ? 1 : 0);
	}

	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination){
		
		// If the shaders are deactivated, just show the normal scene
		if (shaders) {
			if(blacken) {
				mat_main.SetInt("_is_black", 1);
				Graphics.Blit (source, destination, mat_main);
			} else {
				// FOVE eye tracking calculations
				FoveInterfaceBase.EyeRays rays = foveInterface.GetGazeRays();
				Ray r = whichEye == LeftOrRight.Left ? rays.left : rays.right;
				
				if(trackingMode == TrackMode.ViewArea) {
					RaycastHit hit;
					Physics.Raycast(r, out hit, Mathf.Infinity);
					Vector2 center = new Vector2(((float)(cam.WorldToScreenPoint(hit.point).x)/(float)cam.pixelWidth),
												((float)(cam.WorldToScreenPoint(hit.point).y)/(float)cam.pixelHeight));
					mat_main.SetFloat("_centerx", center.x);
					mat_main.SetFloat("_centery", center.y);
				} else {
					cam.transform.rotation = Quaternion.LookRotation(r.direction);
				}
				
				// Calculations done by hand for 60fps. It should be 10ms of fire and 40 of silence (1 frame every 5 at 100Hz)
				// right now it's 16.6/50
				fCount = (fCount+1)%pulseCycle;
				if (fCount == 0) {
					mat_main.SetInt("_is_black", 0);
				} else {
					mat_main.SetInt("_is_black", 1);
				}
				
				
				if (dynamicDisturbance) {
					mat_main.SetFloat("_seed_mask", Random.value);
				}
				// I am not 100% sure why, but it works well, without distorting. I assume that the display is virtually split in two square-ish halves
				if (canny) {
					// Fove's resolution
					RenderTexture tmp1 = RenderTexture.GetTemporary(2560, 1440);
					Graphics.Blit (source, tmp1, mat_canny);
					Graphics.Blit (tmp1, destination, mat_main);
					RenderTexture.ReleaseTemporary(tmp1);
				} else {
				Graphics.Blit (source, destination, mat_main);
				}
			}
		} else {
			if (canny) {
				Graphics.Blit(source, destination, mat_canny);
			} else {
			Graphics.Blit(source, destination);
			}
		}
	}
}
