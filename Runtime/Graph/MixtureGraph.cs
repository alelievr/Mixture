using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable]
public class MixtureGraph : BaseGraph
{
	public MixtureGraph()
	{
		base.onEnabled += Enabled;
	}

	void Enabled()
	{
		// Create an output node if it does not exists
		if (!nodes.Any(n => n is OutputNode))
			AddNode(new OutputNode());
	}

	// TODO: utils functions to embeed subassets
}
