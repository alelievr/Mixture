using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Rendering;
using System;

namespace Mixture
{
	[System.Serializable]
	// [NodeMenuItem("For End")]
	public class ForEnd : MixtureNode, ILoopEnd
	{
		public enum AggregationMode
		{
			FeedbackToStartNode = 0,
			// MergeResult = 1,
			None = 2,
		}

		[Input("Input")]
		public object input;

        [Output("Output")]
        public object output;

		public override string	name => "For End";

		public override bool    hasPreview => false;
		public override bool    hasSettings => false;
		public override bool    showDefaultInspector => true;

		public AggregationMode mode;

		[System.NonSerialized]
		ForStart loopStart;

		[SerializeField, HideInInspector]
		string loopStartGUID;

		protected override void Enable()
		{
			onAfterEdgeConnected += EdgeConnectionCallback;
			onAfterEdgeDisconnected += EdgeConnectionCallback;
			RegisterLoopStart();
		}

		void RegisterLoopStart()
		{
			if (loopStart == null && !String.IsNullOrEmpty(loopStartGUID) && graph.nodesPerGUID.ContainsKey(loopStartGUID))
				loopStart = graph.nodesPerGUID[loopStartGUID] as ForStart;
			if (loopStart != null)
			{
				loopStart.onAfterEdgeConnected += EdgeConnectionCallback;
				loopStart.onAfterEdgeDisconnected += EdgeConnectionCallback;
			}
		}

		void UnregisterLoopStart()
		{
			if (loopStart != null)
			{
				loopStart.onAfterEdgeConnected -= EdgeConnectionCallback;
				loopStart.onAfterEdgeDisconnected -= EdgeConnectionCallback;
			}
		}

		void EdgeConnectionCallback(SerializableEdge edge)
		{
			if (edge.inputPort == inputPorts[0])
			{
				// Update the loop start:
				var newLoopStart = FindInDependencies(n => n is ForStart) as ForStart;
				if (newLoopStart != loopStart)
				{
					UnregisterLoopStart();
					loopStart = newLoopStart;
					RegisterLoopStart();
				}
				else
				{
					loopStart = newLoopStart;
				}
				if (loopStart != null)
					loopStartGUID = loopStart.GUID;
				UpdateAllPorts();
			}
			else if (loopStart != null && edge.inputPort == loopStart.inputPorts[0])
			{
				UpdateAllPorts();
			}
		}

		// Functions with Attributes must be either protected or public otherwise they can't be accessed by the reflection code
		[CustomPortBehavior(nameof(input))]
		public IEnumerable< PortData > InputPortType(List< SerializableEdge > edges)
		{
			var inputType = loopStart?.inputType?.type ?? typeof(object);

            yield return new PortData
            {
                identifier = nameof(input),
                displayName = "Input",
                acceptMultipleEdges = false,
                displayType = inputType,
            };
		}

		[CustomPortBehavior(nameof(output))]
		public IEnumerable< PortData > OutputPortType(List< SerializableEdge > edges)
		{
			var inputType = loopStart?.inputType?.type ?? typeof(object);

            yield return new PortData
            {
                identifier = nameof(output),
                displayName = "Output",
                acceptMultipleEdges = true,
                displayType = inputType,
            };
		}

		protected override bool ProcessNode(CommandBuffer cmd)
		{
			if (loopStart == null)
			{
				Debug.Log("For End node is not connected to a start");
				return false;
			}

			if (mode == AggregationMode.FeedbackToStartNode)
				FeedbackInputToStartNode(cmd);

			// So right now we need to flush the command buffer because we are changing
			// the inputs to some potential CRT nodes in the loop, which result in a change 
			// of material value and because this change is not registered within the command
			// buffer, it would cause issues to just change values without flushing the cmd.
			// We need to consider either material property blocks or a material pool in the loop
			// for these nodes.
			MixtureGraphProcessor.AddGPUAndCPUBarrier(cmd);

			return true;
		}

		RenderTexture doubleBufferForEnd;

		void FeedbackInputToStartNode(CommandBuffer cmd)
		{
			var feedback = input;

			// For render texture we double buffer the input just in case there is only one
			// node in the for loop which will result in the output and input being the same
			// for this node. Which could break most of the nodes that uses render textures.
			if (feedback is RenderTexture rt)
			{
				if (doubleBufferForEnd == null)
					doubleBufferForEnd = new RenderTexture(1, 1, 0);
				
				if (doubleBufferForEnd.width != rt.width
					|| doubleBufferForEnd.height != rt.height
					|| doubleBufferForEnd.graphicsFormat != rt.graphicsFormat
					|| doubleBufferForEnd.useMipMap != rt.useMipMap
					|| doubleBufferForEnd.filterMode != rt.filterMode
					|| doubleBufferForEnd.wrapMode != rt.wrapMode)
				{
					doubleBufferForEnd.Release();
					doubleBufferForEnd.height = rt.height;
					doubleBufferForEnd.width = rt.width;
					doubleBufferForEnd.graphicsFormat = rt.graphicsFormat;
					doubleBufferForEnd.useMipMap = rt.useMipMap;
					doubleBufferForEnd.wrapMode = rt.wrapMode;
					doubleBufferForEnd.filterMode = rt.filterMode;
					doubleBufferForEnd.autoGenerateMips = false;
					doubleBufferForEnd.name = "Double Buffer For End";
					doubleBufferForEnd.Create();
				}

				cmd.CopyTexture(rt, doubleBufferForEnd);

				feedback = doubleBufferForEnd;
			}

			// Feedback the input texture as start output for next iteration
			loopStart.output = feedback;
		}

		public void PrepareNewIteration(BaseNode startNode)
		{
			output = new MixtureMesh();
			inputMeshes.Clear();
		}

        public void PrepareLoopEnd(ILoopStart loopStartNode)
        {
			if (loopStartNode == null)
			{
				Debug.LogError("For End connected to a non For start loop");
				return;
			}
        }

		List<MixtureMesh> inputMeshes = new List<MixtureMesh>();
		public void FinalIteration()
		{
			output = input;
			// TODO
			// if (mode == AggregationMode.MergeResult)
			{
			}
		}

		protected override void Disable()
		{
			if (doubleBufferForEnd != null)
				doubleBufferForEnd.Release();
		}
    }
}