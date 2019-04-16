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
	}

	// TODO: utils functions to embeed subassets
}
