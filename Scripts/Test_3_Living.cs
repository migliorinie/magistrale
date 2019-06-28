using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

// Hotspot, Node and LargeObject are already in the global project namespace because of Tree_spawning. Test_3FStates because of Test_3_Kitchen.
public enum livingLarges {
	L_BOOKCASE,
	L_DESK,
	L_DOOR,
	L_STUFF,
	L_TABLE_CHAIRS,
	L_TV_AREA
}


public class Test_3_Living : MonoBehaviour {

	// These go on the floor
	public Transform bookcase;
	public Transform desk;
	public Transform stuff;
	public Transform TV_area;
	public Transform table_chairs;
	public Transform door;

	[Space(10)]
	// table-bound small items
	public Transform bottle;
	public Transform cola;
	public Transform fruitbowl;
	public Transform moka;
	public Transform teacup;
	public Transform wine_flask;
	
	[Space(10)]
	// desk-bound
	public Transform laptop;
	public Transform desktop;
	public Transform open_book;
	public Transform closed_book;
	public Transform coffee_mug;
	public Transform fan;
	public Transform scissors;
	public Transform printer;
	
	[Space(10)]
	//stuff-bound
	public Transform ac_guitar;
	public Transform cat;
	public Transform ele_guitar;
	public Transform shoes;
	public Transform working_boots;
	public Transform teddy_bear;
	
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
	
	private Test_3FStates state;
	
	private Vector2 currRes;
	private Transform targ;
	
	private System.DateTime begin;
	private System.DateTime end;
	private string elapsed;
	private bool resUpdated;

	void BuildTree(Node root) {
		/* Level 1: big items
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
		
		List<int> lv1 = new List<int> {0, 1, 2, 3, 4, 5};
		Shuffle(lv1);

		List<Transform> table_obj = new List<Transform>();
		table_obj.Add(bottle);
		table_obj.Add(cola);
		table_obj.Add(fruitbowl);
		table_obj.Add(moka);
		table_obj.Add(teacup);
		table_obj.Add(wine_flask);
		
		List<Transform> desk_obj = new List<Transform>();
		// The first is a small one, the second the accessory, the last the main
		switch ((int)Mathf.Floor(Random.value*1.99f)) {
			case 0: {
				desk_obj.Add(coffee_mug);
			}
			break;
			case 1: {
				desk_obj.Add(scissors);
			}
			break;
		}
		switch ((int)Mathf.Floor(Random.value*1.99f)) {
			case 0: {
				desk_obj.Add(fan);
			}
			break;
			case 1: {
				desk_obj.Add(printer);
			}
			break;
		}
		switch ((int)Mathf.Floor(Random.value*3.99f)) {
			case 0: {
				desk_obj.Add(laptop);
			}
			break;
			case 1: {
				desk_obj.Add(open_book);
			}
			break;
			case 2: {
				desk_obj.Add(desktop);
			}
			break;
			case 3: {
				desk_obj.Add(closed_book);
			}
			break;
		}
		
		List<Transform> stuff_obj = new List<Transform>();
		stuff_obj.Add(ac_guitar);
		stuff_obj.Add(cat);
		stuff_obj.Add(ele_guitar);
		stuff_obj.Add(shoes);
		stuff_obj.Add(working_boots);
		stuff_obj.Add(teddy_bear);
	
		Shuffle(table_obj);
		Shuffle(stuff_obj);
		
		do {
			// Pick a target from one of the lists.
			switch ((int)Mathf.Floor(Random.value*2.99f)) {
				case 0: {
					// 4 objects instantiated
					if(table_obj.Count <= 0) {
						break;
					}
					targ = table_obj[table_obj.Count-(int)Mathf.Floor(Random.value*3.99f)-1];
				}
				break;
				case 1: {
					// 3 objects instantiated
					if(desk_obj.Count <= 0) {
						break;
					}
					targ = desk_obj[desk_obj.Count-(int)Mathf.Floor(Random.value*2.99f)-1];
				}
				break;
				case 2: {
					// 4 objects instantiated
					if(stuff_obj.Count <= 0) {
						break;
					}
					targ = stuff_obj[stuff_obj.Count-(int)Mathf.Floor(Random.value*3.99f)-1];
				}
				break;
			}
		} while (StaticValues.tested.Contains(targ));
		StaticValues.tested.Add(targ);
		
		// DEBUG
		//targ = fan;
		
		controlCam.SetTarget(targ);

		// Low-priority but more extendible idea: create a LargeObject list like smalls, then fish from there.
		for (int i = 0; i<lv1.Count; i++) {
			Node child = new Node();
			child.content = null;
			child.parent = root;
			child.children = new List<Node>();
			switch ((livingLarges)lv1[i]) {
				case livingLarges.L_BOOKCASE:
					child.largeContent = buildBookcase();
					break;
				case livingLarges.L_DESK:
					child.largeContent = buildDesk();
					break;
				case livingLarges.L_DOOR:
					child.largeContent = buildDoor();
					break;
				case livingLarges.L_STUFF:
					child.largeContent = buildStuff();
					break;
				case livingLarges.L_TABLE_CHAIRS:
					child.largeContent = buildTable();
					break;
				case livingLarges.L_TV_AREA:
					child.largeContent = buildTVArea();
					break;
			}
			root.children.Add(child);
			
			// Nidified level2 creation. children[i] is the child we just made
			for (int j = 0; j<root.children[i].largeContent.hotspots.Count; j++) {
				Node grandchild = new Node();
				grandchild.largeContent = null;
				grandchild.parent = root.children[i];
				grandchild.children = new List<Node>();
				if((livingLarges)lv1[i] == livingLarges.L_DESK) {
					grandchild.content = Pop<Transform>(desk_obj);
				}
				else if((livingLarges)lv1[i] == livingLarges.L_TABLE_CHAIRS) {
					grandchild.content = Pop<Transform>(table_obj);
				}
				else if((livingLarges)lv1[i] == livingLarges.L_STUFF) {
					grandchild.content = Pop<Transform>(stuff_obj);
				}
				
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
	// Right now, pretty ugly patchwork
	Quaternion TargetAngle(Vector3 rot) {
		if (rot.z != 0) {
			return Quaternion.Euler(0, -90*rot.z, 0);
		} else {
			return Quaternion.Euler(0, 90 - 90*rot.x, 0);
		}
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

		Hotspot east = new Hotspot();
		east.position.x = GetBounds(f.prefab).size.x/2;
		east.position.z = 0;
		east.direction = new Vector3(-1, 0, 0);
		east.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(east);
		
		Hotspot west = new Hotspot();
		west.position.x = (-1)*GetBounds(f.prefab).size.x/2;
		west.position.z = 0;
		// Both decreasing
		west.direction = new Vector3(1, 0, 0);
		west.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(west);

		// The floor is already instantiated
		f.instantiated = floor;

		// Probably hotspot lists should be saved in enums.
		return f;
	}
	
	LargeObject buildBookcase() {
		LargeObject b = new LargeObject();
		b.hotspots = new List<Hotspot>(); //Nothing on top yet
		b.prefab = bookcase;
		b.padding = Absolute(b.prefab.rotation*new Vector3(0.0f, 0.0f, 0.0f));
		
		return b;
	}

	LargeObject buildDesk() {
		LargeObject d = new LargeObject();
		d.hotspots = new List<Hotspot>(); //Nothing on top yet
		d.prefab = desk;
		d.padding = Absolute(d.prefab.rotation*new Vector3(0.15f, 0.0f, 0.0f));
		
		Hotspot main = new Hotspot();
		// Positioned determined by hand
		main.position = new Vector3(0.11f, -0.1872f, -0.1f);
		main.direction = new Vector3(0, 0, 0);
		main.angle = Quaternion.Euler(0, 180, 0);
		d.hotspots.Add(main);
		
		Hotspot accessory = new Hotspot();
		// Positioned determined by hand
		accessory.position = new Vector3(0.06f, -0.1872f, 0.45f);
		accessory.direction = new Vector3(0, 0, 0);
		accessory.angle = Quaternion.Euler(0, 0, 0);
		d.hotspots.Add(accessory);
		
		Hotspot small = new Hotspot();
		// Positioned determined by hand
		small.position = new Vector3(0.06f, -0.1872f, -0.52f);
		small.direction = new Vector3(0, 0, 0);
		small.angle = Quaternion.Euler(0, 00, 0);
		d.hotspots.Add(small);
		
		return d;
	}
	
	LargeObject buildDoor() {
		LargeObject d = new LargeObject();
		d.hotspots = new List<Hotspot>(); //Nothing on top yet
		d.prefab = door;
		d.padding = Absolute(d.prefab.rotation*new Vector3(0.15f, 0.0f, 0.0f));
		
		return d;
	}
	
	LargeObject buildStuff() {
		LargeObject s = new LargeObject();
		s.hotspots = new List<Hotspot>(); //Nothing on top yet
		s.prefab = stuff;
		s.padding = Absolute(s.prefab.rotation*new Vector3(0.1f, 0.0f, 0.1f));
		
		Hotspot hs6h = new Hotspot();
		// Positioned determined by hand
		hs6h.position = new Vector3(0.0f, 0.7f, 0.0f);
		hs6h.direction = new Vector3(0, 0, 0);
		hs6h.angle = Quaternion.Euler(0, -90, 0);
		s.hotspots.Add(hs6h);
		
		Hotspot hs9h = new Hotspot();
		// Positioned determined by hand
		hs9h.position = new Vector3(0.7f, 0.0f, 0.0f);
		hs9h.direction = new Vector3(0, -0, 0);
		hs9h.angle = Quaternion.Euler(0, 0, 0);
		s.hotspots.Add(hs9h);
		
		Hotspot hs12h = new Hotspot();
		// Positioned determined by hand
		hs12h.position = new Vector3(0.0f, -0.7f, 0.0f);
		hs12h.direction = new Vector3(0, 0, 0);
		hs12h.angle = Quaternion.Euler(0, 90, 0);
		s.hotspots.Add(hs12h);
		
		Hotspot hs3h = new Hotspot();
		// Positioned determined by hand
		hs3h.position = new Vector3(-0.7f, 0.0f, 0.0f);
		hs3h.direction = new Vector3(0, 0, 0);
		hs3h.angle = Quaternion.Euler(0, 180, 0);
		s.hotspots.Add(hs3h);
		
		return s;
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
		t.padding = Absolute(t.prefab.rotation*new Vector3(0.4f, 0.0f, 0.1f));
		return t;
	}
	
	LargeObject buildTVArea() {
		LargeObject tv = new LargeObject();
		tv.hotspots = new List<Hotspot>(); //Nothing on top yet
		tv.prefab = TV_area;
		tv.padding = Absolute(tv.prefab.rotation*new Vector3(0.15f, 0.0f, 0.0f));
		
		return tv;
	}
	
	/*HOTSPOTS
	 * Desk - 2
	 * Table - 4
	 * Stuff - 2
	 * TOTAL - 8 for 19 items (considering doubles, 16 without)
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
						
				//Debug.Log(n.parent.largeContent.hotspots[index].position.ToString() + " - " + n.largeContent.instantiated.transform.name);
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
			StaticValues.sw = new StreamWriter(Application.dataPath + "/../Logs/Test_3_Living/Test_3_LivingRoom_" + StaticValues.timestamp + ".json");
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
		
		state = Test_3FStates.PREPARATION;
		Node root = new Node();
		BuildTree(root);
		InstTree(root);
		
		VRController = RigidbodyFirstPersonController.GetInstance().transform;
		leftEye = VRController.Find("Fove Interface").Find("FOVE Eye (Left)").GetComponent<applyShader>();
		rightEye = VRController.Find("Fove Interface").Find("FOVE Eye (Right)").GetComponent<applyShader>();
		VRController.position = new Vector3(-0.894f, -1.102f, -0.711f);
		
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
			case Test_3FStates.PREPARATION:
				// Possibly allow to rotate the object or rotate it automatically
				
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
					state = Test_3FStates.SIMULATION;
					begin = System.DateTime.Now;
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "";
					controlCam.transform.parent.GetComponent<Pivot_rotate>().active = false;
					VRController.GetComponent<RigidbodyFirstPersonController>().enabled = true;
					leftEye.SetBlackening(false);
					rightEye.SetBlackening(false);
				}
			break;
			
			case Test_3FStates.SIMULATION:
				if(Input.GetAxis("Confirm") != 0) {
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Y if CORRECT,\nN if WRONG";
					end = System.DateTime.Now;
					elapsed = (end - begin).ToString();
					string[] tmp = elapsed.Split(':');
					elapsed = tmp[1] + ":" + tmp[2].Substring(0, 6);
					
					leftEye.SetBlackening(true);
					rightEye.SetBlackening(true);
					state = Test_3FStates.EVALUATION;
				}
			break;
			
			case Test_3FStates.EVALUATION:
				if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.N)) {
					state = Test_3FStates.PREPARATION;
					
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Space to start";
					
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
			SceneManager.LoadScene("Test_3_Living");
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
		System.Diagnostics.Process.Start(Application.dataPath + "/../Logs/Test_3_Living/Test_3_LivingRoom_" + StaticValues.timestamp + ".json");
	}
}