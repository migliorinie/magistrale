using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public enum fullLarges {
	L_BOOKCASE,
	L_DESK,
	L_DOOR,
	L_STUFF,
	L_TABLE_CHAIRS,
	L_TV_AREA,
	L_CABINET,
	L_CHAIR,
	L_MOD_KITCHEN
}

public class Test_3_Full : MonoBehaviour {

	// These go on the floor
	public Transform bookcase;
	public Transform desk;
	public Transform stuff;
	public Transform TV_area;
	public Transform table_chairs;
	public Transform door;
	public Transform mod_kitchen;
	public Transform kitchen_chair;
	public Transform cabinet;

	[Space(10)]
	// table-bound small items
	public Transform cereal_box;
	public Transform cola;
	public Transform dish;
	public Transform fruitbowl;
	public Transform jug;
	public Transform pear;
	public Transform teacup;
	public Transform tin_can;
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
	//kitchen-bound
	public Transform bottle;
	public Transform chef_knife;
	public Transform cutting_board;
	public Transform dish_soap;
	public Transform knife_block;
	public Transform microwave;
	public Transform moka;
	public Transform pan;
	public Transform pineapple;
	public Transform pot_small;
	
	[Space(10)]
	//stuff-bound
	//public Transform ac_guitar;
	public Transform cat;
	public Transform dog;
	//public Transform ele_guitar;
	public Transform pillow;
	public Transform shoes;
	public Transform soccer_ball;
	public Transform teddy_bear;
	public Transform working_boots;
	
	[Space(10)]
	public Transform marker;

	public Transform floor;
	
	[Space(10)]
	public Transform VRController;
	public ControlCam controlCam;
	public applyShader leftEye;
	public applyShader rightEye;
	
	[Space(10)]
	public bool restrictKitchen = false;
	[Space(10)]
	public string subject;
	
	[Tooltip("Each run is composed by 12 tests, looping over 80/120, 60/90 and 40/60 layouts, and 15/25/35/45 degrees of FOV")]
	public int runs = 10;
    [Tooltip("If you make a pause between iterations and quit the test, this will let you assign your value.")]
    public int iterationVal = 1;
	//public StaticValues sval;
	
	private Test_3FStates state;
	
	private Vector2 currRes;
	private Transform targ;
	
	private System.DateTime begin;
	private System.DateTime end;
	private string elapsed;
	private float deltaTime;
	
	private bool resUpdated;
	//private InputField infield;

	void BuildTree(Node root) {

		root.content = null;
		root.largeContent = buildFloor();
		root.parent = null;
		root.children = new List<Node>();
		
		List<int> lv1 = new List<int> {1, 8, 2, 0, 6, 5, 7, 4, 3};
		//Shuffle(lv1);

		List<Transform> table_obj = new List<Transform>();
		table_obj.Add(bottle);
		table_obj.Add(cola);
		table_obj.Add(fruitbowl);
		table_obj.Add(dish);
		table_obj.Add(jug);
		table_obj.Add(tin_can);
		table_obj.Add(wine_flask);
		
		List<Transform> kitchen_obj = new List<Transform>();
		kitchen_obj.Add(cutting_board);
		kitchen_obj.Add(dish_soap);
		kitchen_obj.Add(knife_block);
		kitchen_obj.Add(microwave);
		kitchen_obj.Add(moka);
		kitchen_obj.Add(pineapple);
		
		List<Transform> desk_obj = new List<Transform>();
		// The first is a small one, the second the accessory, the last the main
		desk_obj.Add(scissors);
		
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
		//stuff_obj.Add(ac_guitar);
		stuff_obj.Add(cat);
		stuff_obj.Add(dog);
		//stuff_obj.Add(ele_guitar);
		stuff_obj.Add(pillow);
		stuff_obj.Add(shoes);
		stuff_obj.Add(soccer_ball);
		stuff_obj.Add(teddy_bear);
		stuff_obj.Add(working_boots);
	
		Shuffle(table_obj);
        do {
            Shuffle(kitchen_obj);
        } while (kitchen_obj[5] == microwave);
		do {
            Shuffle(stuff_obj);
        } while (stuff_obj[stuff_obj.Count-1] == dog);
		
		if (Random.value > 0.5) {
			kitchen_obj.Add(pan);
		} else {
			kitchen_obj.Add(pot_small);
		}
		
		targ = coffee_mug;
		
		// Here I swap the coffee mug for one random item anywhere.
		// 4 objects on the table, 4 on the ground, 3 on the desk, 7 on the kitchen, total 18
		// At everz iteration it will be in a different place
		int firstRoll;
		do {
			firstRoll = (int)Mathf.Floor(Random.value*3.99f);
		} while ((firstRoll) == StaticValues.lastPosition);
		StaticValues.lastPosition = firstRoll;
		
		switch (firstRoll) {
			case 0: {
				desk_obj[desk_obj.Count -1 -(int)Mathf.Floor(Random.value*2.99f)] = coffee_mug;
				break;
			}
			case 1: {
				table_obj[table_obj.Count -1 -(int)Mathf.Floor(Random.value*3.99f)] = coffee_mug;
				break;
			}
			case 2: {
				stuff_obj[stuff_obj.Count -1 -(int)Mathf.Floor(Random.value*3.99f)] = coffee_mug;
				break;
			}
			case 3: {
				kitchen_obj[kitchen_obj.Count -1 -(int)Mathf.Floor(Random.value*6.99f)] = coffee_mug;
				break;
			}
		}

        /*
        desk_obj = new List<Transform>{scissors, fan, closed_book};
        table_obj = new List<Transform>{wine_flask, bottle, fruitbowl, dish};
        stuff_obj = new List<Transform>{teddy_bear, soccer_ball, shoes, cat};
        kitchen_obj = new List<Transform>{cutting_board, microwave, pineapple, moka, coffee_mug, knife_block, pan};
		*/
        
		controlCam.SetTarget(targ);

		// Low-priority but more extendible idea: create a LargeObject list like smalls, then fish from there.
		for (int i = 0; i<lv1.Count; i++) {
			Node child = new Node();
			child.content = null;
			child.parent = root;
			child.children = new List<Node>();
			switch ((fullLarges)lv1[i]) {
				case fullLarges.L_BOOKCASE:
					child.largeContent = buildBookcase();
					break;
				case fullLarges.L_DESK:
					child.largeContent = buildDesk();
					break;
				case fullLarges.L_DOOR:
					child.largeContent = buildDoor();
					break;
				case fullLarges.L_STUFF:
					child.largeContent = buildStuff();
					break;
				case fullLarges.L_TABLE_CHAIRS:
					child.largeContent = buildTable();
					break;
				case fullLarges.L_TV_AREA:
					child.largeContent = buildTVArea();
					break;
				case fullLarges.L_CABINET:
					child.largeContent = buildCabinet();
					break;
				case fullLarges.L_CHAIR:
					child.largeContent = buildChair();
					break;
				case fullLarges.L_MOD_KITCHEN:
					child.largeContent = buildKitchen();
					break;
				default:
					child.largeContent = new LargeObject(marker);
					break;
			}
			root.children.Add(child);
			
			// Nidified level2 creation. children[i] is the child we just made
			for (int j = 0; j<root.children[i].largeContent.hotspots.Count; j++) {
				Node grandchild = new Node();
				grandchild.largeContent = null;
				grandchild.parent = root.children[i];
				grandchild.children = new List<Node>();
				if((fullLarges)lv1[i] == fullLarges.L_DESK) {
					grandchild.content = Pop<Transform>(desk_obj);
				}
				else if((fullLarges)lv1[i] == fullLarges.L_TABLE_CHAIRS) {
					grandchild.content = Pop<Transform>(table_obj);
				}
				else if((fullLarges)lv1[i] == fullLarges.L_MOD_KITCHEN) {
					grandchild.content = Pop<Transform>(kitchen_obj);
				}
				else if((fullLarges)lv1[i] == fullLarges.L_STUFF) {
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
	
	// Cartesian product of a Vector2 and an int to give a Vector3
	public List<Vector3> Cartesian(List<Vector2> inp1, List<int> inp2) {
		List<Vector3> output = new List<Vector3>();
		for (int i = 0; i<inp1.Count; i++) {
			for (int j = 0; j<inp2.Count; j++) {
				output.Add(new Vector3(inp1[i].x, inp1[i].y, inp2[j]));
			}
		}
		return output;
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

		Hotspot north = new Hotspot();
		north.position.x = 0;
		north.position.z = GetBounds(f.prefab).size.z/2;
		north.direction = new Vector3(0, 0, -1);
		north.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(north);
		
		Hotspot east = new Hotspot();
		east.position.x = GetBounds(f.prefab).size.x/2;
		// Slightly off center as massive things are on the northern wall
		east.position.z = 0;
		east.direction = new Vector3(-1, 0, 0);
		east.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(east);
		
		Hotspot south = new Hotspot();
		south.position.x = 0;
		south.position.z = (-1)*GetBounds(f.prefab).size.z/2;
		// Both decreasing
		south.direction = new Vector3(0, 0, 1);
		south.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(south);
		
		Hotspot west = new Hotspot();
		west.position.x = (-1)*GetBounds(f.prefab).size.x/2;
		// Slightly off center as massive things are on the northern wall
		west.position.z = (-1)*GetBounds(f.prefab).size.z/6;
		// Both decreasing
		west.direction = new Vector3(1, 0, 0);
		west.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(west);
		
		Hotspot center = new Hotspot();
		center.position.x = -0.6f;
		center.position.z = 0.2f;
		// Both decreasing
		center.direction = new Vector3(0, 0, 0);
		center.angle = Quaternion.Euler(0, 0, 0);
		f.hotspots.Add(center);

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
	
	LargeObject buildCabinet() {
		LargeObject c = new LargeObject();
		c.hotspots = new List<Hotspot>(); //Nothing on top yet
		c.prefab = cabinet;
		c.padding = Absolute(c.prefab.rotation*new Vector3(0.2f, 0.0f, 0.0f));
		return c;
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
		accessory.position = new Vector3(0.06f, -0.1872f, 0.55f);
		accessory.direction = new Vector3(0, 0, 0);
		accessory.angle = Quaternion.Euler(0, 0, 0);
		d.hotspots.Add(accessory);
		
		Hotspot small = new Hotspot();
		// Positioned determined by hand
		small.position = new Vector3(0.06f, -0.1872f, -0.7f);
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
	
	LargeObject buildKitchen() {
		LargeObject k = new LargeObject();
		k.hotspots = new List<Hotspot>();
		// I'm only writing hotspots for this one.
		// Can be changed if one places the hotspot generation inside the switch.
		k.prefab = mod_kitchen;
		k.padding = Absolute(k.prefab.rotation*new Vector3(0.0f, 0.0f, 0.0f));
		
		Hotspot cooker = new Hotspot();
		// Positioned determined by hand
		cooker.position = new Vector3(-1.721f, -0.211f, 0.484f);
		cooker.direction = new Vector3(0, 0, 0);
		cooker.angle = Quaternion.Euler(0, 0, 0);
		k.hotspots.Add(cooker);
		
		Hotspot drawerright = new Hotspot();
		// Positioned determined by hand
		drawerright.position = new Vector3(-1.721f, -0.211f, -0.5f);
		drawerright.direction = new Vector3(0, 0, 0);
		drawerright.angle = Quaternion.Euler(0, 0, 0);
		k.hotspots.Add(drawerright);
		
		Hotspot drawerleft = new Hotspot();
		// Positioned determined by hand
		drawerleft.position = new Vector3(-1.721f, -0.211f, 0.0f);
		drawerleft.direction = new Vector3(0, 0, 0);
		drawerleft.angle = Quaternion.Euler(0, 0, 0);
		k.hotspots.Add(drawerleft);
		
		Hotspot whitecenter = new Hotspot();
		// Positioned determined by hand
		whitecenter.position = new Vector3(-1.833f, -0.211f, -1.0f);
		whitecenter.direction = new Vector3(0, 0, 0);
		whitecenter.angle = Quaternion.Euler(0, -45, 0);
		k.hotspots.Add(whitecenter);
		
		Hotspot storage_r = new Hotspot();
		// Positioned determined by hand
		storage_r.position = new Vector3(-1.264f, -0.211f, -1.0f);
		storage_r.direction = new Vector3(0, 0, 0);
		storage_r.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(storage_r);
		
		Hotspot storage_l = new Hotspot();
		// Positioned determined by hand
		storage_l.position = new Vector3(-0.794f, -0.211f, -1.032f);
		storage_l.direction = new Vector3(0, 0, 0);
		storage_l.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(storage_l);
		
		Hotspot washer = new Hotspot();
		// Positioned determined by hand
		washer.position = new Vector3(-0.226f, -0.211f, -1.0f);
		washer.direction = new Vector3(0, 0, 0);
		washer.angle = Quaternion.Euler(0, -90, 0);
		k.hotspots.Add(washer);
		
		return k;
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
		hs6h.position = new Vector3(0.084f, -0.1395f, -0.478f);
		hs6h.direction = new Vector3(0, 0, 0);
		hs6h.angle = Quaternion.Euler(0, 0, 0);
		t.hotspots.Add(hs6h);
		
		Hotspot hs9h = new Hotspot();
		// Positioned determined by hand
		hs9h.position = new Vector3(-0.228f, -0.1395f, 0.046f);
		hs9h.direction = new Vector3(0, -0, 0);
		hs9h.angle = Quaternion.Euler(0, -90, 0);
		t.hotspots.Add(hs9h);
		
		Hotspot hs12h = new Hotspot();
		// Positioned determined by hand
		hs12h.position = new Vector3(0.0965f, -0.1395f, 0.493f);
		hs12h.direction = new Vector3(0, 0, 0);
		hs12h.angle = Quaternion.Euler(0, 0, 0);
		t.hotspots.Add(hs12h);
		
		Hotspot hs3h = new Hotspot();
		// Positioned determined by hand
		hs3h.position = new Vector3(0.37f, -0.1395f, 0.046f);
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
				//Debug.Log(n.parent.largeContent.prefab.name + " " + index.ToString() + " - " + n.content.name);
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
			StaticValues.sw = new StreamWriter(Application.dataPath + "/../Logs/Test_3_Full/Test_3_FullRoom_" + StaticValues.timestamp + ".json");
			StaticValues.resolutions = new List<Vector2> {new Vector2(40, 60), new Vector2(60, 90), new Vector2(80, 120)};
			StaticValues.angles = new List<int> {45, 35, 25, 15};
			StaticValues.configurations = Cartesian(StaticValues.resolutions, StaticValues.angles);
			StaticValues.lastPosition = 999;
            StaticValues.iterVal = iterationVal;
			
			begin = System.DateTime.Now;
			
			// C#'s JSON serializer is just unpleasant. I'll analyze this data with Python anyways
			StaticValues.sw.WriteLine("{");
			StaticValues.sw.WriteLine("\t\"test\": {");
			StaticValues.sw.WriteLine("\t\t\"subject\" : \"" + subject + "\",");
			StaticValues.sw.WriteLine("\t\t\"date\" : \"" + begin.ToString("yyyy-MM-dd") + "\",");
			StaticValues.sw.WriteLine("\t\t\"time\" : \"" + begin.ToString("HH:mm:ss") + "\",");
			StaticValues.sw.WriteLine("\t\t\"canny\" : " + ((leftEye.canny && rightEye.canny)? "1" : "0") + ",");
            StaticValues.sw.WriteLine("\t\t\"noisy\" : " + ((leftEye.brokenChance > float.Epsilon && rightEye.brokenChance > float.Epsilon)? "1" : "0") + ",");
			StaticValues.sw.WriteLine("\t\t\"experiments\" : [");
		
			StaticValues.initialized = true;
		}
		resUpdated = false;
	}
	// Use this for initialization
	void Start () {
		//Cursor.lockState = CursorLockMode.Locked;
		
		state = Test_3FStates.PREPARATION;
		Node root = new Node();
		BuildTree(root);
		InstTree(root);
        
		VRController = CameraOnAStick.GetInstance().transform;
		VRController.position = new Vector3(3.0f, -0.5f, -2.5f);
		VRController.GetComponent<CameraOnAStick>().adjustment = new Vector3(0, 0, 0);
		
		// It would be better to have an input scheme but this is effective until we get a controller
		controlCam.transform.parent.GetComponent<Pivot_rotate>().active = true;
		VRController.GetComponent<CameraOnAStick>().enabled = false;
	}

	// Update is called once per frame
	void Update () {
		
		if (Time.timeSinceLevelLoad < 0.1f){
			// Doing it on start has weird effects.
			leftEye = VRController.Find("Fove Interface").Find("FOVE Eye (Left)").GetComponent<applyShader>();
			rightEye = VRController.Find("Fove Interface").Find("FOVE Eye (Right)").GetComponent<applyShader>();
			leftEye.SetBlackening(true);
			rightEye.SetBlackening(true);
		}
		switch (state) {
			case Test_3FStates.PREPARATION:
				// Possibly allow to rotate the object or rotate it automatically
				
				if (StaticValues.configurations.Count == StaticValues.resolutions.Count * StaticValues.angles.Count) {
					StaticValues.sw.WriteLine("\t\t\t{");
                    // This is for doing multiple iterations WITHOUT a pause in the middle
					//StaticValues.sw.WriteLine("\t\t\t\t\"iteration\" : \"" + (StaticValues.runCount+1).ToString() + "\",");
                    // This is with a pause in the middle, adding the iteration by hand.
                    StaticValues.sw.WriteLine("\t\t\t\t\"iteration\" : \"" + StaticValues.iterVal + "\",");
                    StaticValues.iterVal++;
					StaticValues.sw.WriteLine("\t\t\t\t\"runs\" : [");
				}
				
				if (StaticValues.configurations.Count > 0 && !resUpdated) {
					Vector3 tmp = Pop(StaticValues.configurations);
					currRes = new Vector2(tmp.x, tmp.y);
					
					if (leftEye != null && rightEye != null) {
						leftEye.SetResolution(currRes);
						leftEye.SetViewingAngle(tmp.z);
						rightEye.SetResolution(currRes);
						rightEye.SetViewingAngle(tmp.z);
					}
					StaticValues.sw.WriteLine("\t\t\t\t\t{");
					StaticValues.sw.WriteLine("\t\t\t\t\t\t\"resolution\" : \"" + currRes.x.ToString() + "/" + currRes.y.ToString() + "\",");
					StaticValues.sw.WriteLine("\t\t\t\t\t\t\"angle\" : \"" + tmp.z.ToString() + "\",");
					resUpdated = true;
					
					Debug.Log((StaticValues.runCount+1).ToString() + " - " + (12-StaticValues.configurations.Count).ToString());
				}
				
				if (Input.GetKeyDown(KeyCode.Space)) {
					state = Test_3FStates.SIMULATION;
					begin = System.DateTime.Now;
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "";
					controlCam.transform.parent.GetComponent<Pivot_rotate>().active = false;
					VRController.GetComponent<CameraOnAStick>().enabled = true;
					leftEye.SetBlackening(false);
					rightEye.SetBlackening(false);
				}
			break;
			
			case Test_3FStates.SIMULATION:
				// The examinator takes the time, confirming the answer once the correct one is reached.
				//deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
				//float fps = 1.0f / deltaTime;
				//Debug.Log(fps);
				
				if (Input.GetKeyDown(KeyCode.Space)) {
					controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Enter the accuracy";
					/*
					infield = controlCam.transform.Find("Canvas").Find("accuracy").GetComponent<InputField>();
					infield.gameObject.SetActive(true);
					infield.ActivateInputField();
					infield.Select();
					*/

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
				controlCam.transform.Find("Canvas").Find("lower").GetComponent<Text>().text = "Press Space to start";
				
				// Child 2 is the instantiated clone
				StaticValues.sw.WriteLine("\t\t\t\t\t\t\"name\" : \"" + targ.name.Split('(')[0] + "\",");
				//StaticValues.sw.WriteLine("\t\t\t\t\"result\" : \"" + (infield.GetComponent<InputEditor>().written) + "\",");
				StaticValues.sw.WriteLine("\t\t\t\t\t\t\"elapsed\" : \"" + elapsed + "\"");
				if(StaticValues.configurations.Count == 0) {
					StaticValues.sw.WriteLine("\t\t\t\t\t}");
					StaticValues.sw.WriteLine("\t\t\t\t]"); //sets
					if(StaticValues.runCount == runs-1) {
						StaticValues.sw.WriteLine("\t\t\t}"); //experiments
					} else {
						StaticValues.sw.WriteLine("\t\t\t},"); //experiments
					}
				} else {
				StaticValues.sw.WriteLine("\t\t\t\t\t},");
				}
				elapsed = "";
				Quit();
			break;
		}
	}
	
	void Quit() {
		if (StaticValues.runCount == runs-1 && StaticValues.configurations.Count == 0) {
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		} else {
			if (StaticValues.configurations.Count == 0) {
				StaticValues.configurations = Cartesian(StaticValues.resolutions, StaticValues.angles);
				StaticValues.runCount++;
			}
			SceneManager.LoadScene("Test_3_Full");
		}
	}
	
	// In case of accidental exit
	void OnApplicationQuit() {
		// experiments
		StaticValues.sw.WriteLine("\t\t]");
		// test
		StaticValues.sw.WriteLine("\t}");
		// main object
		StaticValues.sw.WriteLine("}");
		StaticValues.sw.Close();
		//System.Diagnostics.Process.Start(Application.dataPath + "/../Logs/Test_3_Full/Test_3_FullRoom_" + StaticValues.timestamp + ".json");
	}
}