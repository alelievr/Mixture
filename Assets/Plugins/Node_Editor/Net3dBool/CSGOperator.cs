using UnityEngine;
using System.Collections;

using Net3dBool;

public class CSGOperator : MonoBehaviour {

	public enum OperatorType {
		Union,
		Intersection,
		Difference
	};

	public OperatorType		operation;
	private OperatorType 	lastop;
	public GameObject 		objectA;
	public GameObject 		objectB;
	public GameObject 		output;

	public Material 		ObjMaterial;

	public bool 			enableOperator;
	private bool			lasten;

	// Keep a temporary to hold the result - allow for redo
	private Solid 			temp;

	private Solid 			solidA;
	private Solid			solidB;

	private BooleanModeller	modeller;

	// Use this for initialization
	void Start () {

		// Prepare for an operation on solids - grab them.
		solidA = objectA.GetComponent<CSGObject> ().GetSolid ();
		if (solidA == null) {
			Debug.LogError("ObjectA is not a CSGObject.");
			return;
		}
		solidB = objectB.GetComponent<CSGObject> ().GetSolid ();
		if (solidB == null) {
			Debug.LogError("ObjectB is not a CSGObject.");
			return;
		}

		modeller = new Net3dBool.BooleanModeller(solidA, solidB);
		// Do check...

		lastop = (OperatorType)(1 - (int)operation);
		lasten = !enableOperator;
	}

	public void ApplyCSG()
	{
		switch (operation) {
		case OperatorType.Union:
			temp = modeller.getUnion ();
			break;
		case OperatorType.Intersection:
			temp = modeller.getIntersection ();
			break;
		case OperatorType.Difference:
			temp = modeller.getDifference ();
			break;
		}

		// Apply to output game object
		CSGGameObject.GenerateMesh(output, ObjMaterial, temp);
		// Make sure the output object has its 'solid' updated
		output.GetComponent<CSGObject> ().GenerateSolid ();

		// Hide the original objects
		objectA.SetActive (false);
		objectB.SetActive (false);

		lasten = enableOperator;
		lastop = operation;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// If enable or operation has changed then redo csg
		if ((lasten != enableOperator) || (lastop != operation))
			ApplyCSG ();
	}
}
