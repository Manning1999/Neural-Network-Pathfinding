using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class PopulationController : MonoBehaviour
{


    //create singleton - This means that there can only be one of this in the scene at any given time
    public static PopulationController instance;
    private static PopulationController _instance;

    public static PopulationController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PopulationController>();
            }

            return _instance;
        }
    }

    public enum TrainingMethod { BackPropogation, EvolutionaryAlgorithm }

    [Tooltip("Backpropogation requires training data which will over time adjust the network's weights to get a working model (This is often the fastest method IF you have good training data. \n \nEvolutionary " +
        "Algorithm simulates evolutionary processes by having a group of neural-networks with randomized weights (genetic variation), they each do their thing and then the one's that are most fit for whatever you want " +
        "them to do copy their weights to the next generation of networks (reproduce) and randomize a specified number of weights (mutate). This process is then repeated until you have a workable model. This method is " +
        "often slower but can produce better results that backpropogation")]
    public TrainingMethod trainingmethod = TrainingMethod.EvolutionaryAlgorithm;

    public enum ReproductionMethod { Sexual, Asexual }
    public ReproductionMethod reproductionMethod = ReproductionMethod.Sexual;



    //This is used in sexual-reproduction and asexual-resproduction
    NeuralNetwork bestNet = null;

    //This is only used for sexual-reproduction
    NeuralNetwork secondBestNet = null;

    float averageFitness;
    int numFitnesses;

    //If true, any auto-fitness calculations will be ignored
    bool forcedBest = false;

    //This is a list of all the neural networks in the scene
    List<NeuralNetwork> nets = new List<NeuralNetwork>();

    //This is how many neural networks are still "alive"
    List<NeuralNetwork> netsRemaining = new List<NeuralNetwork>();

    [SerializeField]
    [Tooltip("This is how many weights/genes should be randomized/mutated")]
    private int mutationRate = 100;


    [SerializeField]
    private int[] defaultNeuronsPerLayer = new int[] { 5, 4, 3 };
    public int[] _defaultNeuronsPerLayer { get { return defaultNeuronsPerLayer; } private set { defaultNeuronsPerLayer = value; } }


    public void Start()
    {
        //determine whether this is the runner scene of the car scene
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Runner Scene"))
        {
            foreach (Agent agent in GameObject.FindObjectsOfType<Agent>())
            {
                agent.Init(new NeuralNetwork(defaultNeuronsPerLayer));
                nets.Add(agent._net);
            }
        }
        else
        {
            foreach (Car car in GameObject.FindObjectsOfType<Car>())
            {
                //create a new network with an input layer large enough to store every pixel from the camera + 1 so their is a boolean for whether the car is on the track
                car.Init(new NeuralNetwork(new int[4] { car._camera.pixelWidth * car._camera.pixelHeight, 16, 8, 3 }));
                nets.Add(car._net);
                
            }
        }

        netsRemaining = nets;


    }

    [ContextMenu("Kill All")]
    public void KillAll()
    {
        foreach(Agent agent in FindObjectsOfType<Agent>())
        {
            RunnerKilled(agent._net);
        }
       
    }


    //Kill the runner and remove them from the active pool of runners
    public void RunnerKilled(NeuralNetwork net)
    {
        numFitnesses++;



        if (bestNet == null)
        {
            bestNet = net;
        }
       
        if (net.GetFitness() > bestNet.GetFitness())
        {
            if (forcedBest == false) bestNet = net;
        }
        else
        {
            if(secondBestNet == null)
            {
                secondBestNet = net;
            }
            else if(net.GetFitness() > secondBestNet.GetFitness())
            {
                secondBestNet = net;
            }
        }
           

        netsRemaining.Remove(net);

        //if there are no runners left then reproduce, mutate and restart the scene
        if (netsRemaining.Count == 0)
        {
            forcedBest = false;

            if (reproductionMethod == ReproductionMethod.Asexual)
            {

                ASexuallyReproduce();
            }
            else
            {
                SexuallyReproduce(bestNet, secondBestNet);
            }
        }
    }




    //This forces a specific agent to be considered the best (this is used to manually set a best agent in instances where automated fitness doesn't work or isn't accurate enough
    public void SetBest(NeuralNetwork net)
    {
        bestNet = net;
        forcedBest = true;
    }


    #region -----Reproduction Methods-----

    //This method of reproducing will simply copy the weights of the best one to all the other agents and then slightly mutate them
    private void ASexuallyReproduce()
    {

        //reset the position/rotation of all agents and give them all the same weights as the previous best agent then mutate them all slightly 
        foreach (Agent agent in GameObject.FindObjectsOfType<Agent>())
        {

            netsRemaining.Add(agent._net);
            

            if (agent._net != bestNet)
            {

                agent._net.SetWeights(bestNet.GetWeights());
                agent._net.Mutate(mutationRate);

            }
            agent.Reset();
        }

        bestNet = null;
    }
       
        



   


    //This will mix the weights of the top two most fit parents to simulate heritage
    //Not currently working
    private void SexuallyReproduce(NeuralNetwork parent1, NeuralNetwork parent2)
    {
        float[][][] child1Weights = parent1.GetWeights();
        float[][][] child2Weights = parent2.GetWeights();


        //Switch around the first two rows of weights for the parent agents to create two child agents
        for (int i = 0; i < child1Weights.Length; i++)
        {
            for (int j = 0; j < child1Weights[i].Length; j++)
            {
                child1Weights[i] = child2Weights[i];
            }
        }


        for (int i = 0; i < child2Weights.Length; i++)
        {
            for (int j = 0; j < child2Weights[i].Length; j++)
            {
                for (int k = 0; k < child2Weights[i][j].Length; k++)
                {
                    child2Weights[i][j][k] = child1Weights[i][j][k];
                }
            }
        }


        //Reset the position and rotation of each agent
        //set the weights to that of child 1 and then mutate 20 random genes in each agent
        foreach (Agent agent in GameObject.FindObjectsOfType<Agent>())
        {
            
            netsRemaining.Add(agent._net);

            agent.Reset();
            
            if (agent._net != bestNet)
            {
                int randomNum = UnityEngine.Random.Range(0, 1);

                if (randomNum == 0)
                {
                    agent._net.SetWeights(child1Weights);
                }
                else
                {
                    agent._net.SetWeights(child2Weights);
                }
                agent._net.Mutate(2);

            }
    
        }

    }


    #endregion



     
    //This is a helper function which prints things out to a text document
    public static void WriteString(string stringToWrite)
    {
        string path = "Assets/Resources/Test.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(stringToWrite);
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = (TextAsset)Resources.Load("Test.txt");


    }   
 
}
