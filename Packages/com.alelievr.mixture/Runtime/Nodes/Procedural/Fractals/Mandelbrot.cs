using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

namespace Mixture
{
	[System.Serializable, NodeMenuItem("Procedural/Fractal/Mandelbrot")]
	public class Mandelbrot : FractalNode 
	{
		public override string name => "Mandelbrot";

		public override string shaderName => "Hidden/Mixture/Mandelbrot";

		// Enumerate the list of material properties that you don't want to be turned into a connectable port.
		protected override IEnumerable<string> filteredOutProperties => new string[]{};
	}
}