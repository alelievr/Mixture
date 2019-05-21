using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable]
public class MixtureGraph : BaseGraph
{
	// Serialized datas for the editor:
	public bool		realtimePreview;

	public MixtureGraph()
	{
		base.onEnabled += Enabled;
	}

	void Enabled()
	{
	}

	// TODO: utils functions to embeed subassets
}
