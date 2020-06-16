using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NeuralNetwork
{

    private int[] layers;
    private float[][] neurons;
    private float[][][] weights;

    float fitness;
    int numFitnesses = 0;


   

    public NeuralNetwork(int[] layers)
    {
        //deep copy of layers of this network 
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }


        //generate matrix
        InitNeurons();
        InitWeights();
    }

    /// <summary>
    /// This will adds a fitness score which will later be used to find the average fitness of the network
    /// </summary>
    /// <param name="fit">Fitness score to add</param>
    public void AddFitness(float fit)
    {
        fitness += fit;
        numFitnesses++;
    }

    /// <summary>
    /// Sets the fitness to the specified number
    /// </summary>
    /// <param name="fit">This is the number which fitness will be set to</param>
    public void SetFitness(float fit)
    {
        fitness = fit;
    }

    /// <summary>
    /// This will return the fitness of the neural network. If findAverage is true, it will return the average fitness by dividing the total fitness by the number of times it has been given a fitness score
    /// </summary>
    /// <param name="findAverage"></param>
    /// <returns></returns>
    public float GetFitness(bool findAverage = true)
    {
        if (findAverage == false) return fitness;

        fitness = fitness / numFitnesses;
        return fitness;
    }



    /// <summary>
    /// This initializes the network
    /// </summary>
    /// <param name="copyNetwork"></param>
    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        InitNeurons();

        InitWeights();
    }



    

    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();

        //iterate through all layers and add neurons to each layer
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);

        }

        //convert list to jagged array
        neurons = neuronsList.ToArray();
    }


    /// <summary>
    /// This initializes all the weights for the connections between neurons by giving them randomized weights
    /// </summary>
    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();



        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();


            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {

                float[] neuronweights = new float[neuronsInPreviousLayer];

                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronweights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);

                }


                layerWeightsList.Add(neuronweights);
            }


            weightsList.Add(layerWeightsList.ToArray());
        }

        weights = weightsList.ToArray();


    }

    /// <summary>
    /// This will return all the weights in string form so they can be printed out to a text file
    /// </summary>
    /// <returns></returns>
    public string GetWeightsString()
    {
        string weightsString = "";

        for (int i = 0; i < weights.Length; i++)
        {
            weightsString += "\n Layer: " + i + "";
            
            for (int j = 0; j < weights[i].Length; j++)
            {
                weightsString += "\n Layer: [" + i + "] : Neuron [" + j + "]    ";
                for (int k = 0; k < weights[i][j].Length; k++)
                {

                    weightsString += weights[i][j][k] + ", ";

                }
            }
        }

        return weightsString;
    }


    /// <summary>
    /// returns the weights as a three-dimensional jagged array
    /// </summary>
    /// <returns>returns a jagged array which contains all the weights</returns>
    public float[][][] GetWeights()
    {
        return weights;
    }

    /// <summary>
    /// Returns the number of layers
    /// </summary>
    /// <returns></returns>
    public int[] GetLayers(){
        return layers;
    }

    /// <summary>
    /// Returns the neurons
    /// </summary>
    /// <returns></returns>
    public float[][] GetNeurons()
    {
        return neurons;
    }
    

    /// <summary>
    /// Will take the inputs and feed them through the network using it's current weights and then returns the outputs in the form of a float[]
    /// </summary>
    /// <param name="inputs">The inputs are an array of floats. The inputs will be sigmoided in this functions so pre-sigmoiding them is not neccesary</param>
    public float[] FeedForward(float[] inputs)
    {

        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = Sigmoid(inputs[i]);
        }



        //iterate through all the layers
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0.25f;

                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                neurons[i][j] = (float)Math.Tanh(value);

            }
        }

        return neurons[neurons.Length - 1]; //return output layer
    }






    /// <summary>
    /// Manually sets the weights
    /// </summary>
    /// <param name="newWeights">The new value of all the weights</param>
    public void SetWeights(float[][][] newWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {

                    weights[i][j][k] = newWeights[i][j][k];
                }
            }
        }
    }



    /// <summary>
    /// This will completely randomize all of a networks weights
    /// </summary>
    public void CompletelyMutate()
    {
        for(int i = 0; i < weights.Length; i++)
        {
            for(int j = 0; j < weights[i].Length; j++)
            {
                for(int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    float randomNumber = UnityEngine.Random.Range(0, 5);

                    if(randomNumber <= 2)
                    {
                        weight *= -1f;

                    }
                    else if(randomNumber <= 4)
                    {
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if( randomNumber <= 6)
                    {
                        float factor = UnityEngine.Random.Range(0, 1) + 1;
                        weight *= factor;
                    }
                    else if(randomNumber <= 8)
                    {
                        float factor = UnityEngine.Random.Range(0, 1) + 1;
                        weight *= factor;
                    }

                    weights[i][j][k] = weight;
                }
            }
        }
    }

   
    /// <summary>
    /// Mutates the specified number of weights/genes
    /// </summary>
    /// <param name="weightsToMutate"></param>
    //This will choose x number of weights/genes and randomize them
    public void Mutate(int weightsToMutate)
    {

        
        List<Vector3> modifiedGenes = new List<Vector3>();
        for (int x = 0; x < weightsToMutate; x++)
        {
            //pick a random gene
            int i = UnityEngine.Random.Range(0, weights.Length);
            int j = UnityEngine.Random.Range(0, weights[i].Length);
            int k = UnityEngine.Random.Range(0, weights[i][j].Length);
         
            int q = 0;
            foreach(Vector3 vec in modifiedGenes)
            {
                
                if (vec.x == i && vec.y == j && vec.z == k)
                {
                    x--;
                    break;
                }
                
                q++;
            }

            if (q == modifiedGenes.Count)
            {
                modifiedGenes.Add(new Vector3(i, j, k));
                float newWeight = UnityEngine.Random.Range(-1f, 1f);
                //PopulationController.WriteString("Modified gene number: " + i + "," + j + "," + k + "  From " + weights[i][j][k].ToString() + " To " + newWeight);

                weights[i][j][k] = newWeight;
            }
        }


    }

    /// <summary>
    /// /// <summary>
    /// Picks the specified number of weights to mutate within the range and randomizes them (This requires some knowledge of the network's structure. Going out of bounds will make it default to the min/max)
    /// <param name="minLayer">Minimum layer for the range within which to mutate weights</param>
    /// <param name="maxLayer">Maximum layer for the range within which to mutate weights</param>
    /// <param name="minNeuron">Minimum neuron number for the range within which to mutate weights</param>
    /// <param name="maxNeuron">Maximum neuron number for the range within which to mutate weights</param>
    /// <param name="minConnection">Minimum connection number for the range within which to mutate weights</param>
    /// <param name="maxConnection">Maximum connection number for the range within which to mutate weights</param>
    /// <param name="numberOfWeightsToMutate">This is how many weights to mutate within the range</param>
    public void CustomMutate(int minLayer, int maxLayer, int minNeuron, int maxNeuron, int minConnection, int maxConnection, int numberOfWeightsToMutate)
    {
        List<Vector3> modifiedGenes = new List<Vector3>();
        for (int x = 0; x < numberOfWeightsToMutate; x++)
        {


            //pick a random gene
            if (minLayer <= -1) minLayer = 0;
            if (maxLayer > weights.Length) maxLayer = weights.Length;

            int i = UnityEngine.Random.Range(minLayer, maxLayer);

            if (minNeuron <= -1) minNeuron = 0;
            if (maxNeuron > weights[i].Length) maxNeuron = weights[i].Length;

            int j = UnityEngine.Random.Range(minNeuron, maxNeuron);

            if (minConnection <= -1) minConnection = 0;
            if (maxConnection > weights[i][j].Length) maxConnection = weights[i][j].Length;

            int k = UnityEngine.Random.Range(minConnection, maxConnection);

            int q = 0;


            foreach (Vector3 vec in modifiedGenes)
            {

                if (vec.x == i && vec.y == j && vec.z == k)
                {
                    x--;
                    break;
                }

                q++;
            }

            if (q == modifiedGenes.Count)
            {
                modifiedGenes.Add(new Vector3(i, j, k));
                float newWeight = UnityEngine.Random.Range(-1f, 1f);
                //PopulationController.WriteString("Modified gene number: " + i + "," + j + "," + k + "  From " + weights[i][j][k].ToString() + " To " + newWeight);

                weights[i][j][k] = newWeight;
            }

        }
    }


    /// <summary>
    /// Adjusts all the weights by the margin of error
    /// </summary>
    /// <param name="difference">Difference</param>
    public void AdjustWeights(float difference)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] += difference;
                }
            }
        }
    }


    /// <summary>
    /// Flattens a number to be between 0 and 1
    /// </summary>
    /// <param name="x">Number to flatten</param>
    public float Sigmoid(float x)
    {

        float output = 1 / (1 + Mathf.Pow(x, -x));
        return output;
    }


}

