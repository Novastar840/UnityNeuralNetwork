using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Linq;

[TestFixture]
public class NeuralNetworkSaveTests
{
	private string testSaveFileName = "TestNeuralNetworkSave.json";
	private string savesFolder;

	[SetUp]
	public void SetUp()
	{
		savesFolder = Path.Combine(Application.dataPath, "Saves");

		// Clean up any existing test save files before each test
		if (Directory.Exists(savesFolder))
		{
			string[] files = Directory.GetFiles(savesFolder, "Test*.json");
			foreach (string file in files)
			{
				File.Delete(file);
			}
		}
	}

	[TearDown]
	public void TearDown()
	{
		// Clean up test save files after each test
		if (Directory.Exists(savesFolder))
		{
			string[] files = Directory.GetFiles(savesFolder, "Test*.json");
			foreach (string file in files)
			{
				File.Delete(file);
				// Also delete the corresponding .meta file if it exists
				string metaFile = file + ".meta";
				if (File.Exists(metaFile))
				{
					File.Delete(metaFile);
				}
			}
		}
	}

	[Test]
	public void CloneViaJson_CreatesIndependentCopy()
	{
		// Arrange
		NeuralNetwork original = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 3;
		initData.OutputCount = 2;
		initData.HiddenLayerSizes = new int[] { 4, 3 };
		original.InitializeNeuralNetwork(initData);

		// Act
		NeuralNetwork clone = NeuralNetworkSave.CloneViaJson(original);

		// Assert - Clone should not be null and should have same structure
		Assert.IsNotNull(clone);
		Assert.AreNotSame(original, clone);

		// Verify the clone has the same layer sizes
		Assert.AreEqual(3, clone.GetInputLayer().GetNeurons().Length);
		Assert.AreEqual(2, clone.GetOutputLayer().GetNeurons().Length);
	}

	[Test]
	public void CloneViaJson_ModifyingCloneDoesNotAffectOriginal()
	{
		// Arrange
		NeuralNetwork original = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 2;
		initData.OutputCount = 1;
		initData.HiddenLayerSizes = new int[] { 3 };
		original.InitializeNeuralNetwork(initData);

		// Get initial output
		float[] inputData = new float[] { 0.5f, 0.8f };
		float[] originalOutput = original.ProcessData(inputData);

		// Act - Clone and mutate the clone
		NeuralNetwork clone = NeuralNetworkSave.CloneViaJson(original);
		clone.Mutate(0.5f);
		float[] clonedOutput = clone.ProcessData(inputData);

		// Assert - Outputs should be different after mutation
		bool outputsAreDifferent = false;
		for (int i = 0; i < originalOutput.Length; i++)
		{
			if (!Mathf.Approximately(originalOutput[i], clonedOutput[i]))
			{
				outputsAreDifferent = true;
				break;
			}
		}
		Assert.IsTrue(outputsAreDifferent, "Mutating clone should produce different output than original");

		// Verify original is unchanged by re-processing
		float[] originalOutputAfterMutation = original.ProcessData(inputData);
		for (int i = 0; i < originalOutput.Length; i++)
		{
			Assert.AreEqual(originalOutput[i], originalOutputAfterMutation[i], 0.0001f,
				"Original network should not be affected by clone mutation");
		}
	}

	[Test]
	public void CloneViaJson_PreservesNetworkStructure()
	{
		// Arrange
		NeuralNetwork original = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 5;
		initData.OutputCount = 3;
		initData.HiddenLayerSizes = new int[] { 10, 8, 6 };
		original.InitializeNeuralNetwork(initData);

		// Act
		NeuralNetwork clone = NeuralNetworkSave.CloneViaJson(original);

		// Assert
		Assert.AreEqual(5, clone.GetInputLayer().GetNeurons().Length);
		Assert.AreEqual(3, clone.GetOutputLayer().GetNeurons().Length);
		// Note: HiddenLayers is private, so we can't directly verify it
	}

	[Test]
	public void Save_CreatesSaveFile()
	{
		// Arrange
		NeuralNetworkSave saveScriptableObject = ScriptableObject.CreateInstance<NeuralNetworkSave>();

		// Use reflection or direct field access to set the filename for testing
		var field = typeof(NeuralNetworkSave).GetField("SaveFileName",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(saveScriptableObject, testSaveFileName);

		NeuralNetwork neuralNetwork = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 2;
		initData.OutputCount = 1;
		initData.HiddenLayerSizes = new int[] { 4 };
		neuralNetwork.InitializeNeuralNetwork(initData);

		// Act
		saveScriptableObject.Save(neuralNetwork);

		// Assert
		string expectedFilePath = Path.Combine(savesFolder, testSaveFileName);
		Assert.IsTrue(File.Exists(expectedFilePath), "Save file should be created");

		// Verify file contains valid JSON
		string jsonContent = File.ReadAllText(expectedFilePath);
		Assert.IsNotNull(jsonContent);
		Assert.IsNotEmpty(jsonContent);
		Assert.IsTrue(jsonContent.Contains("InputLayer"), "JSON should contain InputLayer data");
	}

	[Test]
	public void Load_ReturnsNullWhenNoSaveFileExists()
	{
		// Arrange
		NeuralNetworkSave saveScriptableObject = ScriptableObject.CreateInstance<NeuralNetworkSave>();

		var field = typeof(NeuralNetworkSave).GetField("SaveFileName",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(saveScriptableObject, "NonExistentFile.json");

		// Act
		NeuralNetwork loadedNetwork = saveScriptableObject.Load();

		// Assert
		Assert.IsNull(loadedNetwork, "Load should return null when no save file exists");
	}

	[Test]
	public void SaveAndLoad_RoundTripPreservesData()
	{
		// Arrange
		NeuralNetworkSave saveScriptableObject = ScriptableObject.CreateInstance<NeuralNetworkSave>();

		var field = typeof(NeuralNetworkSave).GetField("SaveFileName",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(saveScriptableObject, testSaveFileName);

		NeuralNetwork originalNetwork = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 3;
		initData.OutputCount = 2;
		initData.HiddenLayerSizes = new int[] { 5 };
		originalNetwork.InitializeNeuralNetwork(initData);

		// Process some data to establish baseline
		float[] inputData = new float[] { 0.1f, 0.5f, 0.9f };
		float[] originalOutput = originalNetwork.ProcessData(inputData);

		// Act - Save and load
		saveScriptableObject.Save(originalNetwork);
		NeuralNetwork loadedNetwork = saveScriptableObject.Load();

		// Assert
		Assert.IsNotNull(loadedNetwork, "Loaded network should not be null");

		// Process the same input on loaded network
		float[] loadedOutput = loadedNetwork.ProcessData(inputData);

		// Outputs should match (allowing for small floating point differences)
		Assert.AreEqual(originalOutput.Length, loadedOutput.Length);
		for (int i = 0; i < originalOutput.Length; i++)
		{
			Assert.AreEqual(originalOutput[i], loadedOutput[i], 0.0001f,
				$"Output at index {i} should match after save/load round trip");
		}
	}

	[Test]
	public void Save_OverwritesExistingFile()
	{
		// Arrange
		NeuralNetworkSave saveScriptableObject = ScriptableObject.CreateInstance<NeuralNetworkSave>();

		var field = typeof(NeuralNetworkSave).GetField("SaveFileName",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		field.SetValue(saveScriptableObject, testSaveFileName);

		// Create and save first network
		NeuralNetwork network1 = new NeuralNetwork();
		NeuralNetworkInitData initData1 = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData1.InputCount = 2;
		initData1.OutputCount = 1;
		initData1.HiddenLayerSizes = new int[] { 3 };
		network1.InitializeNeuralNetwork(initData1);
		saveScriptableObject.Save(network1);

		string expectedFilePath = Path.Combine(savesFolder, testSaveFileName);
		FileInfo fileInfo1 = new FileInfo(expectedFilePath);
		long fileSize1 = fileInfo1.Length;

		// Act - Save different network to same file
		NeuralNetwork network2 = new NeuralNetwork();
		NeuralNetworkInitData initData2 = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData2.InputCount = 10;
		initData2.OutputCount = 5;
		initData2.HiddenLayerSizes = new int[] { 20, 15 };
		network2.InitializeNeuralNetwork(initData2);
		saveScriptableObject.Save(network2);

		// Assert
		FileInfo fileInfo2 = new FileInfo(expectedFilePath);
		long fileSize2 = fileInfo2.Length;

		// File size should change due to different network structure
		Assert.AreNotEqual(fileSize1, fileSize2, "File should be overwritten with new content");
	}

	[Test]
	public void CloneViaJson_WithEmptyHiddenLayers()
	{
		// Arrange
		NeuralNetwork original = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 4;
		initData.OutputCount = 2;
		initData.HiddenLayerSizes = new int[0];
		original.InitializeNeuralNetwork(initData);

		// Act
		NeuralNetwork clone = NeuralNetworkSave.CloneViaJson(original);

		// Assert
		Assert.IsNotNull(clone);
		Assert.AreEqual(4, clone.GetInputLayer().GetNeurons().Length);
		Assert.AreEqual(2, clone.GetOutputLayer().GetNeurons().Length);
	}

	[Test]
	public void CloneViaJson_WithLargeNetwork()
	{
		// Arrange
		NeuralNetwork original = new NeuralNetwork();
		NeuralNetworkInitData initData = ScriptableObject.CreateInstance<NeuralNetworkInitData>();
		initData.InputCount = 100;
		initData.OutputCount = 50;
		initData.HiddenLayerSizes = new int[] { 200, 150, 100 };
		original.InitializeNeuralNetwork(initData);

		// Act
		NeuralNetwork clone = NeuralNetworkSave.CloneViaJson(original);

		// Assert
		Assert.IsNotNull(clone);
		Assert.AreEqual(100, clone.GetInputLayer().GetNeurons().Length);
		Assert.AreEqual(50, clone.GetOutputLayer().GetNeurons().Length);
	}
}