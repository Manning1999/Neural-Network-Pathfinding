using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NeuralNetwork
{

    private int[] layers;
    private float[][] neurons;
    public float[][][] weights;

    float fitness;
    int numFitnesses = 0;


    public NeuralNetwork(int[] newLayers, float[][] newNeurons, float[][][] newWeights, bool mutateNetwork = false, int genesToMutate = 0)
    {
        
        layers = newLayers;
        neurons = newNeurons;
        weights = newWeights;

        if (mutateNetwork == true) Mutate(genesToMutate);
    }


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


    public void AddFitness(float fit)
    {
        fitness += fit;
        numFitnesses++;
    }

    public void SetFitness(float fit)
    {
        fitness = fit;
    }

    //This will return the fitness of the neural network. If findAverage is true, it will return the average fitness by 
    //dividing the total fitness by the number of times it has been given a fitness score
    public float GetFitness(bool findAverage = true)
    {
        if (findAverage == false) return fitness;

        fitness = fitness / numFitnesses;
        return fitness;
    }



    //This initializes the network
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


    //This initializes all the weights for the connections between neurons by giving them randomized weights
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

    //This will return all the weights in string form so they can be printed out to a text file
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

   
    //returns the weights as a three-dimensional jagged array
    public float[][][] GetWeights()
    {
        return weights;
    }

    //returns the format of the layers
    public int[] GetLayers(){
        return layers;
    }

    //returns the neurons
    public float[][] GetNeurons()
    {
        return neurons;
    }


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




    //manually sets the weights
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



    //This will completely re-randomize the weights for a complete mutation
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
                float newWeight = UnityEngine.Random.Range(-0.5f, 0.5f);
                //PopulationController.WriteString("Modified gene number: " + i + "," + j + "," + k + "  From " + weights[i][j][k].ToString() + " To " + newWeight);

                weights[i][j][k] = newWeight;
            }
        }


    }



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


    //Sigmoid function that helps flatten a number to be between 0 and 1
    public float Sigmoid(float x)
    {

        float output = 1 / (1 + Mathf.Pow(x, -x));
        return output;
    }


}

