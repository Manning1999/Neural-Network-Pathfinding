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

    public enum ReproductionMethod { Sexual, Asexual }
    public ReproductionMethod reproductionMethod = ReproductionMethod.Sexual;




    NeuralNetwork bestNet = null;
    NeuralNetwork secondBestNet = null;
    float averageFitness;
    int numFitnesses;

    bool forcedBest = false;

    List<NeuralNetwork> nets = new List<NeuralNetwork>();

    List<NeuralNetwork> netsRemaining = new List<NeuralNetwork>();


    public void Start()
    {
        //determine whether this is the runner scene of the car scene
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene"))
        {
            foreach (Runner runner in GameObject.FindObjectsOfType<Runner>())
            {
                runner.Init(new NeuralNetwork(new int[3] { 5, 4, 3 }));
                nets.Add(runner._net);
            }
        }
        else
        {
            foreach (Car car in GameObject.FindObjectsOfType<Car>())
            {
                car.Init(new NeuralNetwork(new int[5] { 3137, 32, 32, 16, 6 }));
                nets.Add(car._net);
            }
        }

        netsRemaining = nets;
        

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
            
            ASexuallyReproduce();
        }
    }




    //This forces a specific agent to be considered the best (this is used to manually set a best agent in instances where automated fitness doesn't work or isn't accurate enough
    public void SetBest(NeuralNetwork net)
    {
        bestNet = net;
        forcedBest = true;
    }




    //This method of reproducing will simply copy the weights of the best one to all the other agents and then slightly mutate them
    private void ASexuallyReproduce()
    {
        //determine whether this is the runner scene of the car scene
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene"))
        {
            //reset the position/rotation of all agents and give them all the same weights as the previous best agent then mutate them all slightly 
            foreach (Runner runner in GameObject.FindObjectsOfType<Runner>())
            {

                runner.transform.position = new Vector3(0, 0, 0);
                runner.transform.rotation = Quaternion.Euler(0, 180, 0);
                netsRemaining.Add(runner._net);
                runner.Reset();

                if (runner._net != bestNet)
                {

                    runner._net.SetWeights(bestNet.GetWeights());
                    runner._net.Mutate(3);

                }
            }
        }
        else
        {
            //reset the position/rotation of all agents and give them all the same weights as the previous best agent then mutate them all slightly 
            foreach (Car car in GameObject.FindObjectsOfType<Car>())
            {
               
                netsRemaining.Add(car._net);
                car.Reset();

                if (car._net != bestNet)
                {

                    car._net.SetWeights(bestNet.GetWeights());
                    car._net.Mutate(30);

                }
            }
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
        foreach (Runner runner in GameObject.FindObjectsOfType<Runner>())
        {
            runner.transform.position = new Vector3(0, 0, 0);
            runner.transform.rotation = Quaternion.Euler(0, 180, 0);
            netsRemaining.Add(runner._net);
            
            runner.Reset();
            
            if (runner._net != bestNet)
            {
                int randomNum = UnityEngine.Random.Range(0, 1);

                if (randomNum == 0)
                {
                    runner._net.SetWeights(child1Weights);
                }
                else
                {
                    runner._net.SetWeights(child2Weights);
                }
                runner._net.Mutate(2);

            }
            else
            {
                Debug.Log(runner.transform.name + " Did the best");
            }
        }

    } 


  




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


    //This will read the text document (Will later be used to save models)
    [MenuItem("Tools/Read file")]
    static void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
