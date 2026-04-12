using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handles multithreaded neural network processing for multiple ragdolls.
/// This allows utilizing all CPU cores instead of being limited to a single thread.
/// </summary>
public static class NeuralNetworkParallelProcessor
{
	/// <summary>
	/// Represents a pending neural network computation result
	/// </summary>
	public struct NetworkProcessingResult
	{
		public NeuralNetwork NeuralNetwork;
		public float[] Inputs;
		public float[] Outputs;
		public int RagdollID;
	}

	/// <summary>
	/// Process multiple neural networks in parallel using thread pool.
	/// Returns immediately with a task that completes when all networks are processed.
	/// </summary>
	public static Task<NetworkProcessingResult[]> ProcessNetworksInParallel(NetworkProcessingResult[] networksToProcess)
	{
		return Task.Run(() =>
		{
			Parallel.ForEach(networksToProcess, network =>
			{
				network.Outputs = network.NeuralNetwork.ProcessData(network.Inputs);
			});

			return networksToProcess;
		});
	}
}
