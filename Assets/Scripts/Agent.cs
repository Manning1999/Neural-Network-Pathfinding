﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using B83.Image.BMP;
using UnityEditor;
using UnityEngine;

public class Agent : MonoBehaviour
{


    protected float[] inputs;

    protected NeuralNetwork net = null;
    public NeuralNetwork _net { get { return net; } protected set { net = value; } }


    #region -----Save Options-----

    [SerializeField]
    protected UnityEngine.Object savedNetwork;


    [SerializeField]
    [Tooltip("If true, when a neural network is saved, it will overwrite it's current saved network. If false, it will create a new file")]
    protected bool overwriteSavedNetworkOnSave = false;

    [SerializeField]
    protected string neuralNetworkSaveLocation = "\\Neural Networks\\";

    #endregion


    protected bool initialized = false;


    #region -----Training Options-----
    [SerializeField]
    protected List<Texture2D> trainingImages = new List<Texture2D>();

    public List<Texture2D> _trainingImages { get { return trainingImages; } }

    [SerializeField]
    protected TextAsset trainingAnswers = null;

    public TextAsset _trainingAnswers { get { return trainingAnswers; } }

    [SerializeField]
    [Min(1)]
    protected int trainingIterations = 10;

    [SerializeField]
    protected bool playerControlled = true;

    [SerializeField]
    protected bool recordActions = true;

    protected bool isRecordingActions = false;

    #endregion


    [SerializeField]
    protected float[] results = new float[3];

    [SerializeField]
    protected bool stopped = false;


    [SerializeField]
    protected new Camera camera = null;

    public Camera _camera { get { return camera; } protected set { camera = value; } }


    [SerializeField]
    [Tooltip("This is used to record what buttons the user presses when they are in the process of manually gathering training data")]
    protected float[] userOutput = new float[3];


    [SerializeField]
    [Tooltip("This is where the training images should be stored")]
    protected string trainingImagesFolderPath = "\\Racecar Training Images\\";


    protected StreamWriter writer;







    // Start is called before the first frame update
    protected virtual void Start()
    {
       

        
    }



    //This sets the neural network
    public virtual void Init(NeuralNetwork net)
    {

        //If not network size has been specified then a default one will be made
        if (net == null)
        {
            if (savedNetwork == null)
            {
                net = new NeuralNetwork(PopulationController.Instance._defaultNeuronsPerLayer);
            }
            else
            {
                net = ReadSavedNetwork();
            }
        }

        if (savedNetwork != null)
        {
            net = ReadSavedNetwork();
        }

        this.net = net;
        initialized = true;
    }



    // Update is called once per frame
    protected virtual void Update()
    {
       

    }


   

   

    [ContextMenu("Save network")]
    public void SaveNetwork()
    {
        //Create a directory in which to store any saved neural networks
        if (!System.IO.Directory.Exists(Application.dataPath + neuralNetworkSaveLocation))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + neuralNetworkSaveLocation);
        }

        //This is the default file name for new saved neural networks
        string fileName = transform.name + " network" + "--" + inputs.Length + "--" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;

        BinaryFormatter bf = new BinaryFormatter();


        //either overwrite the saved file or create a new one
        if (overwriteSavedNetworkOnSave == false)
        {
            var file = File.Open(Application.dataPath + "\\Resources\\" + neuralNetworkSaveLocation + fileName + ".nne", FileMode.Create);
            
            //serialize the neural network so it can be stored more easily 
            bf.Serialize(file, net);
            savedNetwork = Resources.Load<UnityEngine.Object>(Application.dataPath + "\\Resources\\" + neuralNetworkSaveLocation + fileName + ".nne");
            file.Close();

        }
        else
        {
            Debug.Log(savedNetwork);
            Debug.Log(AssetDatabase.GetAssetPath(savedNetwork));
            var file = File.Open(AssetDatabase.GetAssetPath(savedNetwork), FileMode.Append);
            bf.Serialize(file, net);
            file.Close();

        }

        

    }

    //This will return the neural network stored in the .nne file
    protected NeuralNetwork ReadSavedNetwork()
    {

        //If the file is invalid, return null
        if (!AssetDatabase.GetAssetPath(savedNetwork).ToString().Contains(".nne"))
        {
            Debug.LogWarning("Invalid file attached to: " + transform.name + ". Please ensure that it is a .nne file that was generated by this program");
            savedNetwork = null;
            return null;
        }

        BinaryFormatter bf = new BinaryFormatter();
        NeuralNetwork newNet = null;
        
        Debug.Log(AssetDatabase.GetAssetPath(savedNetwork));
        var file = File.Open(AssetDatabase.GetAssetPath(savedNetwork), FileMode.Open);

        newNet = bf.Deserialize(file) as NeuralNetwork;
        Debug.Log("read saved file");
        file.Close();
        return newNet;
    }




    public static byte[] ConvertToByteArray(string str, Encoding encoding)
    {
        return encoding.GetBytes(str);
    }

    public static String ToBinary(Byte[] data)
    {
        return string.Join(" ", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
    }

  
    public virtual void Reset()
    {
        stopped = false;
        net.SetFitness(0);
    }


    public virtual void KillAgent()
    {
        Debug.Log("killed");
        stopped = true;
        
        PopulationController.Instance.RunnerKilled(net);
    }


    [ContextMenu("Make best")]
    public void MakeBest()
    {
        PopulationController.Instance.SetBest(net);
    }


    //flattening function for a single float
    public virtual float Sigmoid(float x)
    {
        //Round the sigmoid to 6 decimals
        float output = (float)Math.Round(1 / (1 + Mathf.Pow(x, -x)), 6);
        return output;
    }


    //Flattening function for an array of floats
    public virtual float[] Sigmoid(float[] x)
    {

        float[] output = new float[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            //Round the sigmoid to 6 decimals
            output[i] = (float)Math.Round(1 / (1 + Mathf.Pow(x[i], -x[i])), 6);
        }
        return output;
    }



    //Flattens a number to be between 0 and 1
    protected virtual float SigmoidDerivative(float x)
    {
        return (float)Math.Round(x * (1 - x), 6);
    }


    //flattens all the numbers in an array to be between 0 and 1
    protected float[] SigmoidDerivative(float[] x)
    {
        float[] output = new float[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            output[i] = (float)Math.Round(x[i] * (1 - x[i]), 6);
        }
        return output;
    }



    //This just prints to the console the size of the neural network
    [ContextMenu("Print network details")]
    public void PrintDetails()
    {
        Debug.Log("Neurons: " + net.GetNeurons().Length);
        Debug.Log(net.GetWeights()[1][1][1]);

    }


    // Take a "screenshot" of a camera's Render Texture.
    protected Texture2D RTImage()
    {
        camera.targetTexture = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;

        camera.targetTexture = null;
        return image;
    }



    #region --------Training Tools---------

    [ContextMenu("Train Model")]
    public void Train()
    {

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(trainingAnswers));
        var allLines = File.ReadAllText(AssetDatabase.GetAssetPath(trainingAnswers), Encoding.Default).Split(" \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(c => c.Length == 0);
        Init(new NeuralNetwork(new int[4] { camera.pixelWidth * camera.pixelHeight + 1, 64, 32, 6 }));

        float[,] trainingOutputs = new float[trainingImages.Count, 6];
        trainingOutputs = ReadAnswersFromFile();

        //load all of the training images into a list
        foreach (string file in System.IO.Directory.GetFiles(Application.dataPath + "/Racecar Training Images/"))
        {

            if (!file.Contains(".meta") && file.Contains(".bmp"))
            {
                trainingImages.Add(LoadTexture(file));

            }
        }

        for (int i = 0; i < trainingIterations; i++)
        {




            foreach (Texture2D trainingImage in trainingImages)
            {


                //inputs = new float[trainingImage.height * trainingImage.width + 1];
                int z = 0;
                foreach (Color pixel in trainingImage.GetPixels())
                {
                    //grayscale the pixels to change them from rgb values to a single float
                    inputs[z] = pixel.grayscale;
                    z++;
                }
                float[] outputs = new float[6];
                float[] errors = null;

                float[] errorTimesOutputs = null;

                //Run through the network with the current weights
                outputs = net.FeedForward(Sigmoid(inputs));

                errors = new float[6];



                //get the margin of error for each output and adjust the weights accordingly
                for (int x = 0; x < outputs.Length; x++)
                {

                    try
                    {
                        errors[x] = outputs[x] - trainingOutputs[trainingImages.IndexOf(trainingImage), x];
                        Debug.Log(errors[x]);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Something went wrong: " + e);
                    }
                }

                errorTimesOutputs = new float[6];
                for (int y = 0; y < errorTimesOutputs.Length; y++)
                {
                    errorTimesOutputs[y] = errors[y] * SigmoidDerivative(outputs[y]);
                }


                try
                {
                    //Get dot product of the inputs and outputs
                    float adjustments = inputs.Zip(errorTimesOutputs, (d1, d2) => d1 * d2).Sum();

                    Debug.Log(adjustments);

                    //Tell the neural network to adjust all the weights
                    net.AdjustWeights(adjustments);

                }
                catch (Exception e)
                {
                    Debug.Log("Something went wrong down here: " + e);

                }
            }
        }
    }





    [ContextMenu("Read answers")]
    public virtual float[,] ReadAnswersFromFile()
    {
        string text = trainingAnswers.ToString(); // Converts to string
        string[] lines = text.Split('\n'); // Splits per newline
        float[,] trainingOutputs = new float[lines.Length, 6];

        int lastCheckedPos = 0;
        for (int j = 0; j < lines.Length - 1; j++)
        {

            for (int k = 0; k < 6; k++)
            {

                try
                {
                    if (k == 0)
                    {

                        trainingOutputs[j, k] = float.Parse(lines[j].Substring(0, 1));
                        lastCheckedPos = lines[j].IndexOf(",");
                    }
                    else if (k == 5)
                    {

                        trainingOutputs[j, k] = float.Parse(lines[j].Substring(lastCheckedPos + 1, 1));
                        lastCheckedPos = 0;
                    }
                    else
                    {

                        trainingOutputs[j, k] = float.Parse(lines[j].Substring(lastCheckedPos + 1, 1));
                        lastCheckedPos = lines[j].IndexOf(",", lastCheckedPos + 1);
                    }

                }
                catch (Exception e)
                {
                    Debug.Log("Error at: " + k + " : " + j + ":" + e);
                }


            }
        }

        return trainingOutputs;
    }



    public static Texture2D LoadTexture(string filePath)
    {

        Texture2D tex = null;
        byte[] fileData;

        Debug.Log(filePath);

        try
        {
            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);

                BMPLoader bmpLoader = new BMPLoader();
                //bmpLoader.ForceAlphaReadWhenPossible = true; //Uncomment to read alpha too

                //Load the BMP data
                BMPImage bmpImg = bmpLoader.LoadBMP(fileData);

                //Convert the Color32 array into a Texture2D
                tex = bmpImg.ToTexture2D();

            }
            else
            {
                Debug.Log("File doesn't exist");
            }
        }
        catch (Exception e)
        {
            Debug.Log("Couldn't convert image: " + e);
        }
        return tex;
    }



    //This is can be used to gather the training images when you only need a single image
    [ContextMenu("Save Screenshot")]
    public virtual void SaveImage()
    {
        string path = Application.dataPath + "\\" + trainingImagesFolderPath + "\\";

        Texture2D _texture = RTImage();

        //_texture = ConvertToGrayscale(_texture);

        byte[] _bytes = _texture.EncodeToPNG();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        System.IO.File.WriteAllBytes(path + "Image-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".png", _bytes);
        Debug.Log("image was saved to was saved as: " + path);
    }





    //This records the users actions every x seconds
    protected virtual IEnumerator RecordActions()
    {
        yield return new WaitForSeconds(0.1f);





        string userOutputString = "";
        for (int i = 0; i < 6; i++)
        {

            if (i != 5) userOutputString += userOutput[i] + ",";
            if (i == 5) userOutputString += userOutput[i] + "/";
        }
        writer.WriteLine(userOutputString);


        //Restart the record actions method
        if (playerControlled == true && recordActions == true)
        {
            StartCoroutine(RecordActions());
            yield return null;
        }


        //removes empty lines from the training answers file
        var allLines = File.ReadAllText(AssetDatabase.GetAssetPath(trainingAnswers), Encoding.Default).Split(" \r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(c => c.Length == 0);
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(trainingAnswers));
        camera.transform.GetComponent<ScreenRecorder>().enabled = true;
        writer.Close();
        Debug.Log("Added lines to file");
    }




    //This records the user's actions when called
    protected virtual void RecordAction()
    {

        writer = new StreamWriter(AssetDatabase.GetAssetPath(trainingAnswers), true);

        string userOutputString = "";
        for (int i = 0; i < 6; i++)
        {

            if (i != 5) userOutputString += userOutput[i] + ",";
            if (i == 5) userOutputString += userOutput[i] + "/";

        }
        writer.WriteLine(userOutputString);
        writer.Close();

        SaveImage();

    }
    #endregion



}
