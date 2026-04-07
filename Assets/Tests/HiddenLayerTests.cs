using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class HiddenLayerTests
{
	[Test]
	public void HiddenLayer_InitializeLayer_CreatesCorrectNumberOfNeurons()
	{
		// Arrange
		HiddenLayer layer = new HiddenLayer();
		int size = 4;
		float[] previousValues = new float[] { 1f, 2f, 3f };

		// Act
		layer.InitializeLayer(size, previousValues);

		// Assert
		Assert.AreEqual(size, layer.GetNeurons().Length);
	}

	[Test]
	public void HiddenLayer_GetAllNeuronValues_ReturnsNonNegativeValues()
	{
		// Arrange
		HiddenLayer layer = new HiddenLayer();
		int size = 3;
		float[] previousValues = new float[] { 0.5f, 0.5f, 0.5f };

		// Act
		layer.InitializeLayer(size, previousValues);
		float[] values = layer.GetAllNeuronValues();

		// Assert - Hidden neurons use ReLU, so values should be >= 0
		Assert.AreEqual(size, values.Length);
		foreach (float value in values)
		{
			Assert.GreaterOrEqual(value, 0f);
		}
	}

	[Test]
	public void HiddenLayer_GetNeurons_ReturnsHiddenNeuronArray()
	{
		// Arrange
		HiddenLayer layer = new HiddenLayer();
		int size = 2;
		float[] previousValues = new float[] { 1f };

		// Act
		layer.InitializeLayer(size, previousValues);
		HiddenNeuron[] neurons = layer.GetNeurons();

		// Assert
		Assert.IsNotNull(neurons);
		Assert.AreEqual(size, neurons.Length);
	}

	[Test]
	public void HiddenLayer_InitializeLayer_SetsPreviousLayerValues()
	{
		// Arrange
		HiddenLayer layer = new HiddenLayer();
		int size = 2;
		float[] previousValues = new float[] { 1f, 2f, 3f, 4f };

		// Act
		layer.InitializeLayer(size, previousValues);

		// Assert - Neurons should be initialized and have calculated values
		HiddenNeuron[] neurons = layer.GetNeurons();
		foreach (HiddenNeuron neuron in neurons)
		{
			Assert.GreaterOrEqual(neuron.GetNeuronValue(), 0f);
		}
	}
}