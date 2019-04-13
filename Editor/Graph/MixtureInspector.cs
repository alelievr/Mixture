using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using UnityEditor;

[CustomEditor(typeof(MixtureGraph), false)]
public class MixtureInspector : GraphInspector
{
	protected override void CreateInspector()
	{
		base.CreateInspector();
	}
}
