using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private GameObject RightLegHigh;
    [SerializeField] private GameObject LeftLegHigh;
    [SerializeField] private GameObject RightLegLow;
    [SerializeField] private GameObject LeftLegLow;
    [SerializeField] private GameObject RightFoot;
    [SerializeField] private GameObject LeftFoot;
    [SerializeField] private GameObject Body;

    [SerializeField] private NeuralNetworkInitData InitData;
    [SerializeField] private NeuralNetworkSave SaveFile;

    private Vector3 RightLegHighPosition;
    private Vector3 LeftLegHighPosition;
    private Vector3 RightLegLowPosition;
    private Vector3 LeftLegLowPosition;
    private Vector3 RightFootPosition;
    private Vector3 LeftFootPosition;
    private Vector3 BodyPosition;

    private NeuralNetwork NeuralNetwork;

    private void Awake()
    {
    }

    private void Start()
    {
        NeuralNetwork = SaveFile.Load();
        if (NeuralNetwork == null)
        {
            NeuralNetwork = new NeuralNetwork();
            NeuralNetwork.InitializeNeuralNetwork(InitData);
            NeuralNetwork.Save(SaveFile);
        }
    }

    private void UpdateBodyPositions()
    {
        RightLegHighPosition = RightLegHigh.transform.position;
        LeftLegHighPosition = LeftLegHigh.transform.position;
        BodyPosition = Body.transform.position;
        RightLegLowPosition = RightLegLow.transform.position;
        LeftLegLowPosition = LeftLegLow.transform.position;
    }
}
