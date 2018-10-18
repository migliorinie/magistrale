using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

// Hotspot, Node and LargeObject are already in the global project namespace because of Tree_spawning.

public enum Test_2FStates {
	PREPARATION,
	SIMULATION,
	EVALUATION
}

public class Test_2_Kitchen : MonoBehaviour {

	public Transform cabinet;
	// These three differ for the stove position
	public Transform mod_kitchen_stoveleft;
	public Transform mod_kitchen_stovemiddle;
	public Transform mod_kitchen_stoveright;
	public Transform refrigerator;
	public Transform table_chairs;
	public Transform kitchen_chair;

	// 23 small items
	public Transform cereal_box;
	public Transform chef_knife;
	public Transform coffee_maker;
	public Transform coffee_mug;
	public Transform cutting_board;
	public Transform dish_1;
	public Transform dish_2;
	public Transform dish_3;
	public Transform dish_soap;
	public Transform knife_block;
	public Transform microwave;
	public Transform pan;
	public Transform pot_small;
	public Transform vase;
	
	public Transform bottle;
	public Transform cola;
	public Transform fish;
	public Transform jug;
	public Transform mushroom;
	public Transform pear;
	public Transform pineapple;
	public Transform teacup;
	public Transform wine_flask;
	
	public Transform window;
	
	[Space(10)]
	public Transform marker;

	public Transform floor;
	
	[Space(10)]
	public Transform VRController;
	public ControlCam controlCam;
	public applyShader leftEye;
	public applyShader rightEye;
	
	[Space(10)]
	public string subject;
	public int runs = 4;
	//public StaticValues sval;
	
	private Test_2FStates state;
	
	private Vector2 currRes;
	private Transform targ;
	
	private System.DateTime begin;
	private System.DateTime end;
	private string elapsed;
	private bool resUpdated;

	void BuildTree(Node root) {
		/* Level 1: big items: fridge, kitchen furniture, table, cabinet
		 * Level 2: top items
		 *
		 * When building big items, I'll check the content of the node
		 * (which maybe should have strings instead of the real objects)
		 * and then create a proper LargeObject. Best practice, do this
		 * by creating a constructor.
		 *
		 * NOTE: This function is working, but is a bad practice for extending.
		 * It would be appropriate, in time, to replace it with something allowing for multiple layers of
		 * large objects and variable numbers of such.
		 */

		// This has to be hardcoded. Only table and chair can go in the middle.
		root.content = null;
		root.largeContent = buildFloor();
		root.parent = null;
		root.children = new List<Node>();
		
		List<int> lv1 = new List<int> {0, 1, 2, 3};
		Shuffle(lv1);
		lv1.Add(4);

		List<Transform> smalls = new List<Transform>();
		smalls.Add(cereal_box);
		smalls.Add(chef_knife);
		smalls.Add(coffee_maker);
		smalls.Add(coffee_mug);
		smalls.Add(cutting_board);
		//smalls.Add(dish_1); // Orange and apple are too similar
		smalls.Add(dish_2);
		smalls.Add(dish_3);
		smalls.Add(dish_soap);
		smalls.Add(knife_block);
		smalls.Add(microwave);
		smalls.Add(pan);
		smalls.Add(pot_small);
		smalls.Add(vase);
		
		smalls.Add(bottle);
		smalls.Add(cola);
		smalls.Add(fish);
		smalls.Add(jug);
		smalls.Add(mushroom);
		smalls.Add(pear);
		smalls.Add(pineapple);
		smalls.Add(teacup);
		smalls.Add(wine_flask);
		// Not adding the window.
	
		Shuffle(smalls);
		
		// 12 objects are instantiated. One [n-0; n-11] is made to be the target.
		do {
			targ = smalls[smalls.Count-(int)Mathf.Floor(Random.value*11.99f)-1];
		} while (StaticValues.tested.Contains(targ));
		StaticValues.tested.Add(targ);
		controlCam.SetTarget(targ);

		// Low-priority but more extendible idea: create a LargeObject list like smalls, then fish from there.
		for (int i = 0; i<lv1.Count; i++) {
			Node child = new Node();
			child.content = null;
			child.parent = root;
			child.children = new List<Node>();
			switch ((larges)lv1[i]) {
				case larges.L_CABINET:
					child.largeContent = buildCabinet();
					break;
				case larges.L_KITCHEN:
					child.largeContent = buildKitchen();
					break;
				case larges.L_REFRIGERATOR:
					child.largeContent = buildFridge();
					break;
				case larges.L_TABLE_CHAIRS:
					if (Random.value > 0.5f || i == 4) {
						child.largeContent = buildTable();
					}
					else {
						Swap<int>(lv1, i, 4);
						child.largeContent = buildChair();
					}
					break;
				case larges.L_CHAIR:
					child.largeContent = buildChair();
					break;
			}
			root.children.Add(child);
			
			// Nidified level2 creation. children[i] is the child we just made
			for (int j = 0; j<root.children[i].largeContent.hotspots.Count; j++) {
				Node grandchild = new Node();
				grandchild.largeContent = null;
				grandchild.parent = root.children[i];
				grandchild.children = new List<Node>();
				grandchild.content = Pop<Transform>(smalls);
				root.children[i].children.Add(grandchild);
				//Debug.Log("Adding " + grandchild.content.name + " to " + child.largeContent.prefab.name);
			}
		}
		
		
	}

	// Utility functions for dealing with Vector3s
	public Vector3 ElemwiseProduct(Vector3 first, Vector3 second) {
		return new Vector3(first.x*second.x, first.y*second.y, first.z*second.z);
	}

	public Vector3 Absolute(Vector3 input) {
		return new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
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
		T r = inp[i];
		inp.RemoveAt(i);
		return r;
	}

	Bounds GetBounds(Transform obj) {
		Renderer renderer = obj.GetComponent<Renderer>();
		Bounds combinedBounds;
		if(renderer != null) {
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

	Vector3 RotatedBounds(Vector3 size, float angle) {
		// Get to radians
		angle = angle/180*3.1416f;
		float newX = Mathf.Abs(size.x*Mathf.Cos(angle)) + Mathf.Abs(size.z*Mathf.Sin(angle));
		float newZ = Mathf.Abs(size.x*Mathf.Sin(angle)) + Mathf.Abs(size.z*Mathf.Cos(angle));
		return new Vector3(newX, size.y, newZ);
	}

	// Get the Y angle to assign to objects from a hotspot's direction.
	// For the moment, they are just sticking to the longer side (Z).
	Quaternion TargetAngle(Vector3 rot) {
		return Quaternion.Euler(0, 90*rot.x, 0);
	}

	// Returns the top position of an object
	float YTop (Transform obj) {
		return(GetBounds(obj).center.y + GetBounds(obj).size.y/2);
	}

	// Centers on half the topper's size in the direction of the hotspot.
	// Rotation is possibly inconsequential, provided that the prefabs have
	// been properly rotated to standing position.
	Vector3 SnapToHotspot (LargeObject bot, Transform top, int index, Quaternion rot) {

		Bounds bnds = GetBounds(top);
		bnds.center = rot*bnds.center;
		bnds.size = RotatedBounds(bnds.size, rot.eulerAngles.y);

		Vector3 outvec = GetBounds(bot.instantiated).center + bot.instantiated.rotation*bot.hotspots[index].position +
				 ElemwiseProduct(bnds.size/2, bot.hotspots[index].direction);
		outvec.y = YTop(bot.instantiated) + bnds.size.y/2 + (bot.instantiated.rotation*bot.hotspots[index].position).y;
		outvec = outvec - bnds.center;

		/*Debug.Log("Hotspot position " + bot.hotspots[index].position.ToString() +
		"\nrotated hotspot position " + bot.instantiated.rotation*bot.hotspots[index].position +
		"\nbot position " + bot.instantiated.position.ToString() +
		"\nbot padding " + bot.padding.ToString() +
		"\nbot direction " + bot.direction.ToString() +
		"\nbot " + GetBounds(bot.instantiated).ToString() +
		"\ntop " + GetBounds(top) +
		"\nfinal output " + outvec.ToString());*/
		return outvec;
	/* VERY VERY IMPORTANT
	 * Prefabs must have their rotation on the X and Z axes set to have them stand upright.
	 * Rotation on the Y axis will have to depend on the hotspot's direction.
	 */
	}


	// Overload for instancing Large Objects, which have padding
	Vector3 SnapToHotspot (LargeObject bot, LargeObject top, int index, Quaternion rot) {

		Bounds bnds = GetBounds(top.prefab);
		bnds.center = rot*bnds.center;
		bnds.size = RotatedBounds(bnds.size, rot.eulerAngles.y);
		// Updating padding to correct size in case of direction 0
		top.padding = Absolute(rot*top.padding);

		Vector3 outvec = bot.instantiated.position + bot.hotspots[index].position +
						 ElemwiseProduct(bnds.size/2 + top.padding, bot.hotspots[index].direction);
		outvec.y = YTop(bot.instantiated) + bnds.size.y/2 + bot.hotspots[index].position.y;
		outvec = outvec - bnds.center;

		top.direction = bot.hotspots[index].direction;

		/*Debug.Log("From position " + bot.hotspots[index].position.ToString() +
		" and bounds " + bnds.ToString() + " to " + outvec.ToString());*/
		return outvec;
	}

	LargeObject buildFloor() {
		//The floor is already present, so no prefab.
		LargeObject f = new LargeObject();
		f.hotspots = new List<Hotspot>();
		// Remember not to instantiate it again
		f.prefab = floor;

		// A constructor would be more beautiful. Low priority.

		// Hotspot takes X and Z, Y is used for non-convex surfaces(table with chairs)
		// Also, this assumes that the floor is centered in [0, 0, 0]
		Hotspot northeast = new Hotspot();
		northeast.position.x = GetBounds(f.prefab).size.x/2;
		northeast.position.z = GetBounds(f.prefab).size.z/2;
		// It gets the maximum x and z, so they can only decrease.
		northeast.direction = new Vector3(-1, 0, -1);
		northeast.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(northeast);

		Hotspot northwest = new Hotspot();
		northwest.position.x = (-1)*GetBounds(f.prefab).size.x/2;
		northwest.position.z = GetBounds(f.prefab).size.z/2;
		// Increasing x, decreasing z
		northwest.direction = new Vector3(1, 0, -1);
		northwest.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(northwest);

		Hotspot southeast = new Hotspot();
		southeast.position.x = GetBounds(f.prefab).size.x/2;
		southeast.position.z = (-1)*GetBounds(f.prefab).size.z/2;
		// Increasing z, decreasing x
		southeast.direction = new Vector3(-1, 0, 1);
		southeast.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(southeast);

		Hotspot southwest = new Hotspot();
		southwest.position.x = (-1)*GetBounds(f.prefab).size.x/2;
		southwest.position.z = (-1)*GetBounds(f.prefab).size.z/2;
		// Both decreasing
		southwest.direction = new Vector3(1, 0, 1);
		southwest.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(southwest);

		Hotspot center = new Hotspot();
		center.position = new Vector3(0, 0, 0);
		center.direction = new Vector3(0, 0, 0);
		center.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(center);

		// The floor is already instantiated
		f.instantiated = floor;

		// Probably hotspot lists should be saved in enums.
		return f;
	}

	LargeObject buildFridge() {
		LargeObject f = new LargeObject();
		f.hotspots = new List<Hotspot>(); //Nothing on top yet
		f.prefab = refrigerator;
		f.padding = Absolute(f.prefab.rotation*new Vector3(0.15f, 0.0f, 0.0f));
		return f;
	}

	LargeObject buildCabinet() {
		LargeObject c = new LargeObject();
		c.hotspots = new List<Hotspot>(); //Nothing on top yet
		c.prefab = cabinet;
		c.padding = Absolute(c.prefab.rotation*new Vector3(0.2f, 0.0f, 0.0f));
		return c;
	}

	LargeObject buildKitchen() {
		LargeObject k = new LargeObject();
		k.hotspots = new List<Hotspot>();
		// The hardcoding hurts. As a low priority, a builder to
		// return a new GameObject can be created.
		switch ((int)Mathf.Floor(Random.value*3))
		{
			case 0:
				k.prefab = mod_kitchen_stoveleft;
				break;
			case 1:
				k.prefab = mod_kitchen_stovemiddle;
				break;
			case 2:
				k.prefab = mod_kitchen_stoveright;
				break;
		}
		// I'm only writing hotspots for this one.
		// Can be changed if one places the hotspot generation inside the switch.
		k.prefab = mod_kitchen_stovemiddle;
		k.padding = Absolute(k.prefab.rotation*new Vector3(0.0f, 0.0f, 0.0f));
		
		Hotspot cooker = new Hotspot();
		// Positioned determined by hand
		cooker.position = new Vector3(-0.0315f, -0.221f, 0.3166f);
		cooker.direction = new Vector3(0, 0, 0);
		cooker.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(cooker);
		
		Hotspot whiteleft = new Hotspot();
		// Positioned determined by hand
		whiteleft.position = new Vector3(-0.06f, -0.221f, 1.896f);
		whiteleft.direction = new Vector3(0, 0, 0);
		whiteleft.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(whiteleft);
		
		Hotspot drawerleft = new Hotspot();
		// Positioned determined by hand
		drawerleft.position = new Vector3(0.196f, -0.221f, 1.337f);
		drawerleft.direction = new Vector3(0, 0, 0);
		drawerleft.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(drawerleft);
		
		Hotspot drawerright = new Hotspot();
		// Positioned determined by hand
		drawerright.position = new Vector3(-0.055f, -0.221f, 0.829f);
		drawerright.direction = new Vector3(0, 0, 0);
		drawerright.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(drawerright);
		
		Hotspot washer = new Hotspot();
		// Positioned determined by hand
		washer.position = new Vector3(0.0f, -0.221f, -0.847f);
		washer.direction = new Vector3(0, 0, 0);
		washer.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(washer);
		
		Hotspot rightmost_l = new Hotspot();
		// Positioned determined by hand
		rightmost_l.position = new Vector3(0.0f, -0.221f, -1.289f);
		rightmost_l.direction = new Vector3(0, 0, 0);
		rightmost_l.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(rightmost_l);
		
		Hotspot rightmost_r = new Hotspot();
		// Positioned determined by hand
		rightmost_r.position = new Vector3(0.20f, -0.221f, -1.846f);
		rightmost_r.direction = new Vector3(0, 0, 0);
		rightmost_r.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(rightmost_r);
		
		return k;
	}

	LargeObject buildTable() {
		// IMPORTANT: We can easily add some more randomness by rotating the table 180 degrees
		LargeObject t = new LargeObject();
		t.hotspots = new List<Hotspot>();

		// 6 hours looking from chairless side
		Hotspot hs6h = new Hotspot();
		// Positioned determined by hand
		hs6h.position = new Vector3(0.084f, -0.2395f, -0.478f);
		hs6h.direction = new Vector3(0, 0, 0);
		hs6h.angle = Quaternion.Euler(0, 0, 0);
		t.hotspots.Add(hs6h);
		
		Hotspot hs9h = new Hotspot();
		// Positioned determined by hand
		hs9h.position = new Vector3(-0.228f, -0.2395f, 0.046f);
		hs9h.direction = new Vector3(0, -0, 0);
		hs9h.angle = Quaternion.Euler(0, -90, 0);
		t.hotspots.Add(hs9h);
		
		Hotspot hs12h = new Hotspot();
		// Positioned determined by hand
		hs12h.position = new Vector3(0.0965f, -0.2395f, 0.493f);
		hs12h.direction = new Vector3(0, 0, 0);
		hs12h.angle = Quaternion.Euler(0, 0, 0);
		t.hotspots.Add(hs12h);
		
		Hotspot hs3h = new Hotspot();
		// Positioned determined by hand
		hs3h.position = new Vector3(0.37f, -0.2395f, 0.046f);
		hs3h.direction = new Vector3(0, 0, 0);
		hs3h.angle = Quaternion.Euler(0, 90, 0);
		t.hotspots.Add(hs3h);

		t.prefab = table_chairs;
		t.padding = Absolute(t.prefab.rotation*new Vector3(0.7f, 0.0f, 0.1f));
		return t;
	}

	LargeObject buildChair() {
		LargeObject c = new LargeObject();
		c.hotspots = new List<Hotspot>(); //Nothing on top yet

		Hotspot hsCenter = new Hotspot();
		// Positioned determined by hand. Remember that it's aligned to the original orientation.
		hsCenter.position = new Vector3(0.0f, 0.0f, -0.5842f);
		hsCenter.direction = new Vector3(0, 0, 0);
		hsCenter.angle = Quaternion.Euler(0, 180, 0);
		c.hotspots.Add(hsCenter);

		c.prefab = kitchen_chair;
		c.padding = Absolute(c.prefab.rotation*new Vector3(0.3f, 0.0f, 0.0f));
		return c;
	}
	
	/*HOTSPOTS
	 * Chair - 1
	 * Table - 4
	 * Kitchen - 7
	 * TOTAL - 12 for 23 items
	 */
	 
	void InstTree(Node n) {
		if(n.parent != null) {
			int index = 0;
			for(int i = 0; i<n.parent.children.Count; i++) {
				if (n.parent.children[i] == n)
					index = i;
			}
			if(n.largeContent != null && n.content == null) {
				n.largeContent.instantiated = Instantiate(n.largeContent.prefab,
					SnapToHotspot(n.parent.largeContent, n.largeContent, index, n.parent.largeContent.hotspots[index].angle*
						TargetAngle(n.parent.largeContent.hotspots[index].direction)),
					n.parent.largeContent.hotspots[index].angle*
						TargetAngle(n.parent.largeContent.hotspots[index].direction)*n.largeContent.prefab.rotation);
				// Large objects have children
				for (int i = 0; i<n.children.Count; i++) {
					InstTree(n.children[i]);
				}
			} else if (n.content != null && n.largeContent == null) {
				//Instantiate(marker,
				Instantiate(n.content,
					SnapToHotspot(n.parent.largeContent, n.content, index, n.parent.largeContent.hotspots[index].angle*
						TargetAngle(n.parent.largeContent.hotspots[index].direction)),
					Quaternion.Euler(0, n.parent.largeContent.instantiated.rotation.eulerAngles.y, 0)*
						n.parent.largeContent.hotspots[index].angle*
						TargetAngle(n.parent.largeContent.hotspots[index].direction)*n.content.rotation);
			}
		} else { //If it's the root
			for (int i = 0; i<n.children.Count; i++) {
				InstTree(n.children[i]);
			}
		}
	}

	void Awake() {
		if (!StaticValues.initialized) {
			StaticValues.timestamp = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm");
			StaticValues.runCount = 0;
			StaticValues.sw = new StreamWriter(Application.dataPath + "/../Logs/Test_2_Kitchen/Test_2_Kitchen_" + StaticValues.timestamp + ".json");
			StaticValues.resolutions = new List<Vector2> {new Vector2(40, 60), new Vector2(60, 90), new Vector2(80, 120), new Vector2(120, 150)};
			StaticValues.runsPerRes = runs/StaticValues.resolutions.Count;
			StaticValues.tested = new List<Transform>();
			
			begin = System.DateTime.Now;
			
			// C#'s JSON serializer is just unpleasant. I'll analyze this data with Python anyways
			StaticValues.sw.WriteLine("{");
			StaticValues.sw.WriteLine("\t\"test\": {");
			StaticValues.sw.WriteLine("\t\t\"subject\" : \"" + subject + "\",");
			StaticValues.sw.WriteLine("\t\t\"date\" : \"" + begin.ToString("yyyy-MM-dd") + "\",");
			StaticValues.sw.WriteLine("\t\t\"time\" : \"" + begin.ToString("HH:mm:ss") + "\",");
			StaticValues.sw.WriteLine("\t\t\"canny\" : " + ((leftEye.canny && rightEye.canny)? "1" : "0" + ","));
			StaticValues.sw.WriteLine("\t\t\"experiments\" : [");
		
			StaticValues.initialized = true;
		}
		resUpdated = false;
	}
	// Use this for initialization
	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		
		state = Test_2FStates.PREPARATION;
		Node root = new Node();
		BuildTree(root);
		InstTree(root);
		
		VRController = RigidbodyFirstPersonController.GetInstance().transform;
		leftEye = VRController.Find("Fove Interface").Find("FOVE Eye (Left)").GetComponent<applyShader>();
		rightEye = VRController.Find("Fove Interface").Find("FOVE Eye (Right)").GetComponent<applyShader>();
		VRController.position = new Vector3(-0.04f, -1.04f, -1.88f);
		
		// I can maybe add a second FOVE interface with a "Please Wait" screen
		leftEye.SetBlackening(true);
		rightEye.SetBlackening(true);
				
		// It would be better to have an input scheme but this is effective until we get a controller
		controlCam.transform.parent.GetComponent<Pivot_rotate>().active = true;
		VRController.GetComponent<RigidbodyFirstPersonController>().enabled = false;
	}

	// Update is called once per frame
	void Update () {
		switch (state) {
			case Test_2FStates.PREPARATION:
				// Possibly allow to rotate the object or rotate it automatically
				
				/*
				 *** TODO *** TODO *** TODO ***
				 * PROBLEM WITH THE CONTROL SCHEME:
				 * The user is using a keyboard/joystick to move
				 * The controller needs a keyboard to evaluate the answers.
				 * Without a joystick, there's a conflict.
				 * With a joystick, I need to remember to disable the controller during PREPARATION
				 *
				 * Alternatively, making a separate control scheme: the controller has WASD to rotate the preview and the
				 * spacebar to activate the test, the subject has the mouse to give an answer and the arrows to move.
				 */
				if ((StaticValues.runCount)%StaticValues.runsPerRes == 0 && StaticValues.resolutions.Count > 0 && !resUpdated) {
					currRes = Pop(StaticValues.resolutions);
					if (leftEye != null && rightEye != null) {
						leftEye.SetResolution(currRes);
						rightEye.SetResolution(currRes);
					}
					if (StaticValues.runCount > 0) {
						// runs
						StaticValues.sw.WriteLine("\t\t\t\t]");
						// set
						StaticValues.sw.WriteLine("\t\t\t},");
					}
					StaticValues.sw.WriteLine("\t\t\t{");
					StaticValues.sw.WriteLine("\t\t\t\t\"resolution\" : \"" + currRes.x.ToString() + "/" + currRes.y.ToString() + "\",");
					StaticValues.sw.WriteLine("\t\t\t\t\"runs\" : [");
					resUpdated = true;
				}
				
				if (Input.GetKeyDown(KeyCode.Space)) {
					state = Test_2FStates.SIMULATION;
					begin = System.DateTime.Now;
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "";
					controlCam.transform.parent.GetComponent<Pivot_rotate>().active = false;
					VRController.GetComponent<RigidbodyFirstPersonController>().enabled = true;
					leftEye.SetBlackening(false);
					rightEye.SetBlackening(false);
				}
			break;
			
			case Test_2FStates.SIMULATION:
				if(Input.GetAxis("Confirm") != 0) {
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Y if CORRECT,\nN if WRONG";
					end = System.DateTime.Now;
					elapsed = (end - begin).ToString();
					string[] tmp = elapsed.Split(':');
					elapsed = tmp[1] + ":" + tmp[2].Substring(0, 6);
					
					leftEye.SetBlackening(true);
					rightEye.SetBlackening(true);
					state = Test_2FStates.EVALUATION;
				}
			break;
			
			case Test_2FStates.EVALUATION:
				if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.N)) {
					state = Test_2FStates.PREPARATION;
					
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Space to start";
					
					// item
					StaticValues.sw.WriteLine("\t\t\t\t\t{");
					// Child 2 is the instantiated clone
					StaticValues.sw.WriteLine("\t\t\t\t\t\t\"name\" : \"" + targ.name.Split('(')[0] + "\",");
					StaticValues.sw.WriteLine("\t\t\t\t\t\t\"result\" : \"" + (Input.GetKeyDown(KeyCode.Y) ? "CORRECT" : "WRONG") + "\",");
					StaticValues.sw.WriteLine("\t\t\t\t\t\t\"elapsed\" : \"" + elapsed + "\"");
					if ((StaticValues.runCount)%StaticValues.runsPerRes != 0) {
						StaticValues.sw.WriteLine("\t\t\t\t\t},");
					} else {
						StaticValues.sw.WriteLine("\t\t\t\t\t}");
					}
					elapsed = "";
					Quit();
				}
			break;
		}
	}
	
	void Quit() {
		
		if (StaticValues.runCount == runs-1) {
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		} else {
			StaticValues.runCount++;
			resUpdated = false;
			SceneManager.LoadScene("Test_2_Kitchen");
		}
	}
	
	// In case of accidental exit
	void OnApplicationQuit() {
		// runs
		StaticValues.sw.WriteLine("\t\t\t\t]");
		// set
		StaticValues.sw.WriteLine("\t\t\t}");
		// experiments
		StaticValues.sw.WriteLine("\t\t]");
		// test
		StaticValues.sw.WriteLine("\t}");
		// main object
		StaticValues.sw.WriteLine("}");
		StaticValues.sw.Close();
		System.Diagnostics.Process.Start(Application.dataPath + "/../Logs/Test_2_Kitchen/Test_2_Kitchen_" + StaticValues.timestamp + ".json");
	}
}