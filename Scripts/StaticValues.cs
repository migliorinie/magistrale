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
	
	public static List<Vector2> resolutions;
	
	// There can also be a toBeTested if we want to do the same test
	public static List<Transform> tested;
}
