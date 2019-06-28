using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StaticValues {

	public static string timestamp;
	
	public static int runCount = 0;
	
	public static bool initialized = false;

	public static StreamWriter sw;
	
	public static int runsPerRes;
    public static int iterVal;
	
	public static List<Vector2> resolutions;
	
	// These are only used in Test_2_Full
	public static List<int> angles;
	public static List<Vector3> configurations;
	
	public static int lastPosition;
	
	// There can also be a toBeTested if we want to do the same test
	public static List<Transform> tested;
}
