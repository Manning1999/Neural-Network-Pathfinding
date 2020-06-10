using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using B83.Image.BMP;
using UnityEditor;
using UnityEngine;

public class Car : MonoBehaviour
{

    [SerializeField]
    private int trainingIterations = 10;

    NeuralNetwork net;
    public NeuralNetwork _net { get { return net; } private set { net = value; } }

    bool initialized = false;

    [SerializeField]
    List<Texture2D> trainingImages = new List<Texture2D>();

    [SerializeField]
    private TextAsset trainingAnswers = null;

    [SerializeField]
    float[] results = new float[6];

    [SerializeField]
    bool stopped = false;

    [SerializeField]
    WheelCollider FRWheel = null;

    [SerializeField]
    WheelCollider FLWheel = null;

    [SerializeField]
    WheelCollider BRWheel = null;

    [SerializeField]
    WheelCollider BLWheel = null;

    Rigidbody rb = null;

    [SerializeField]
    new Camera camera = null;

    public Camera _camera { get { return camera; } private set { camera = value; } }

    float[] inputs;

    [SerializeField]
    private float accelerationSpeed = 500;

    [SerializeField]
    private float maxSpeed = 50000;

    [SerializeField]
    [Tooltip("This is the maximum for how much the steering wheels can rotate (Inverses for minimum rotation)")]
    private float maxWheelRotation = 40;

    [SerializeField]
    private bool isOnTrack = true;

    [SerializeField]
    private LayerMask layerMask;


    [SerializeField]
    private float[] userOutput = new float[6];

    [SerializeField]
    bool playerControlled = true;

    [SerializeField]
    private bool recordActions = true;

    bool isRecordingActions = false;

    StreamWriter writer;

    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        inputs = new float[camera.pixelWidth * camera.pixelHeight];
        rb = transform.GetComponent<Rigidbody>();
        startPosition = transform.position;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (net != null && playerControlled == false && stopped == false)
        {
            

            Accelerate();

            int i = 0;

            foreach (Color pixel in RTImage().GetPixels())
            {
                //grayscale the pixels to change them from rgb values to a single float
                //each pixel is an input for the neural network
                inputs[i] = pixel.grayscale;
                i++;
            }

            //feed the inputs into the neural network
            results = net.FeedForward(inputs);

            results = Sigmoid(results);


            //controls wheel rotation based on which output was highest
            if (results[0] > results[1])
            {
                if (results[0] > results[2])
                {
                    TurnLeft();
                }
                else
                {
                    TurnRight();
                }
            }
            else
            {
                if (results[1] > results[2])
                {
                    StraightenOut();

                }
                else
                {

                    TurnRight();
                }  
            }

           

            /*
           //controls acceleration
           //3 is decelerate, 4 is maintain speed, 5 is accelerate
           if (results[3] > results[4])
           {
               if (results[4] > results[5])
               {
                   //Maintain speed
               }
               else
               {
                   Accelerate();
               }

           }
           else
           {
               if (results[3] > results[5])
               {
                   Decelerate();
               }
               else
               {
                   Accelerate();
               }
           } */
        }

        if (playerControlled == true)
        {
            GetPlayerInput();

            //This records the players actions every x seconds
            if (isRecordingActions == false && recordActions == true)
            {
                isRecordingActions = true;
                camera.transform.GetComponent<ScreenRecorder>().enabled = true;
                StartCoroutine(RecordActions());
            }
        }


        //CheckIfOnTrack();


        //Makes the wheels rotate so that they look like the wheel colliders
        ApplyLocalPositionToVisuals(FRWheel);
        ApplyLocalPositionToVisuals(FLWheel);
        ApplyLocalPositionToVisuals(BRWheel);
        ApplyLocalPositionToVisuals(BLWheel);
            

    }


    private void CheckIfOnTrack()
    {
        RaycastHit hit;

        // Forward Raycast
        if (Physics.Raycast(transform.position + transform.up * 3, transform.TransformDirection(Vector3.down), out hit, 5, layerMask))
        {
            if (hit.transform.tag == "Track")
            {
                isOnTrack = true;
              //  inputs[camera.pixelWidth * camera.pixelHeight] = 1;
            }
            else
            {
                isOnTrack = false;
            }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            //inputs[camera.pixelWidth * camera.pixelHeight] = 0;

        }
        else
        {
            if (net != null)
            {
                PopulationController.Instance.RunnerKilled(net);
            }
        }

    }

    //This updates the wheels to look like the wheel colliders
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }




    #region -------Car controls-------
    private void Accelerate()
    {
        if (FRWheel.motorTorque <= maxSpeed)
        {
            BRWheel.motorTorque += accelerationSpeed;
            BLWheel.motorTorque += accelerationSpeed;

            BRWheel.brakeTorque = 0;
            BLWheel.brakeTorque = 0;
            FRWheel.brakeTorque = 0;
            FLWheel.brakeTorque = 0;
        }
        Debug.Log("Accelerating now");
    }

    private void Decelerate()
    {

        BRWheel.brakeTorque += accelerationSpeed * 2;
        BLWheel.brakeTorque += accelerationSpeed * 2;
        FRWheel.brakeTorque += accelerationSpeed * 2;
        FLWheel.brakeTorque += accelerationSpeed * 2;

        //Slowly remove the motor torque
        BRWheel.motorTorque = Mathf.Lerp(BRWheel.motorTorque, 0, 3f);
        BLWheel.motorTorque = Mathf.Lerp(BLWheel.motorTorque, 0, 3f);
        Debug.Log("Decelerating Now");

    }


    private void TurnRight()
    {
        FRWheel.steerAngle = Mathf.Lerp(FRWheel.steerAngle, maxWheelRotation, Time.deltaTime * 1);
        FLWheel.steerAngle = Mathf.Lerp(FLWheel.steerAngle, maxWheelRotation, Time.deltaTime * 1);
    }

    private void TurnLeft()
    {

        FRWheel.steerAngle = Mathf.Lerp(FRWheel.steerAngle, -maxWheelRotation, Time.deltaTime * 1);
        FLWheel.steerAngle = Mathf.Lerp(FLWheel.steerAngle, -maxWheelRotation, Time.deltaTime * 1);

    }


    private void StraightenOut()
    {
        FRWheel.steerAngle = Mathf.Lerp(FRWheel.steerAngle, 0, 0.5f);
        FLWheel.steerAngle = Mathf.Lerp(FLWheel.steerAngle, 0, 0.5f);
    }
    #endregion


    // Take a "screenshot" of a camera's Render Texture.
    Texture2D RTImage()
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


    //This sets the neural network
    public void Init(NeuralNetwork net)
    {
        //If not network size has been specified then a default one nwill be made
        if(net == null)
        {
            net = new NeuralNetwork(new int[4] { camera.pixelWidth * camera.pixelHeight, 16, 16, 3 });
        }
        this.net = net;
        initialized = true;
    }


   
    [ContextMenu("FeedForward")]
    public void ManuallyFeedForward()
    {
        int i = 0;
        foreach (Color pixel in RTImage().GetPixels())
        {
            //grayscale the pixels to change them from rgb values to a single float
            inputs[i] = pixel.grayscale;
            i++;
        }

        results = net.FeedForward(inputs);

        results = Sigmoid(results);
    }



    public void Reset()
    {
        transform.position = new Vector3(240.71f, 0.5531964f, 216.5f);
        transform.rotation = Quaternion.Euler(0, -174.285f, 0);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        BLWheel.motorTorque = 0;
        BRWheel.motorTorque = 0;

        BRWheel.brakeTorque = 0;
        FRWheel.brakeTorque = 0;
        BLWheel.brakeTorque = 0;
        FLWheel.brakeTorque = 0;

        FRWheel.steerAngle = 0;
        FLWheel.steerAngle = 0;
        stopped = false;
    }




    private void OnCollisionEnter(Collision col)
    {
        if (col.transform.tag == "Wall" && net != null)
        {
            KillCar();
        }
    }


    #region -----Genetic Algorithm tools-----
    [ContextMenu("Kill Car")]
    public void KillCar()
    {
        Debug.Log("Killed " + transform.name);
        stopped = true;
        net.SetFitness(Vector3.Distance(startPosition, transform.position));
        PopulationController.Instance.RunnerKilled(net);

    }

    [ContextMenu("Make best")]
    private void MakeBest()
    {
        PopulationController.Instance.SetBest(net);
    }

    #endregion

    





    //This is just used for debugging purposes to see how training images look in grayscale
    Texture2D ConvertToGrayscale(Texture2D tex)
    {
        Color32[] pixels = tex.GetPixels32();
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                Color32 pixel = pixels[x + y * tex.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l, l, l, 1);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply(false);
        return tex;
    }






    public float[] Sigmoid(float[] x)
    {

        float[] output = new float[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            output[i] = (float)Math.Round(1 / (1 + Mathf.Pow(x[i], -x[i])), 6);
        }
        return output;
    }



    //Flattens a number to be between 0 and 1
    private float SigmoidDerivative(float x)
    {
        return (float)Math.Round(x * (1 - x), 6);
    }

    //flattens all the numbers in an array to be between 0 and 1
    private float[] SigmoidDerivative(float[] x)
    {
        float[] output = new float[x.Length];

        for(int i = 0; i < x.Length; i++)
        {
            output[i] = (float)Math.Round(x[i] * (1 - x[i]), 6);
        }
        return output;
    }


   



    [ContextMenu("Print network details")]
    public void PrintDetails()
    {
        Debug.Log("Neurons: " + net.GetNeurons().Length);
        Debug.Log(net.GetWeights()[1][1][1]);
        
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
    public float[,] ReadAnswersFromFile()
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





    //This is used to get the player's input as well as gather training data
    private void GetPlayerInput()
    {
        //These are used to record the conditions when a button should be pressed down
        if (Input.GetKeyDown("w"))
        {
            RecordAction();
        }
        if (Input.GetKeyDown("s"))
        {
            RecordAction();
        }
        if (Input.GetKeyDown("a"))
        {
            RecordAction();
        }
        if (Input.GetKeyDown("d"))
        {
            RecordAction();
        }



        //These are used to manually control the car
        if (Input.GetKey("w"))
        {
            Accelerate();
            userOutput[5] = 1;
        }
        if (Input.GetKey("s"))
        {
            Decelerate();
            userOutput[3] = 1;
        }
        if (Input.GetKey("a"))
        {
            TurnLeft();
            userOutput[0] = 1;

        }
        if (Input.GetKey("d"))
        {
            TurnRight();
            userOutput[2] = 1;
        }



        //These are used to both control the car as well as record when a button should be released
        if (Input.GetKeyUp("w"))
        {
            userOutput[5] = 0;
            RecordAction();
        }
        if (Input.GetKeyUp("s"))
        {

            userOutput[3] = 0;
            RecordAction();
        }
        if (Input.GetKeyUp("a"))
        {

            userOutput[0] = 0;
            RecordAction();
        }
        if (Input.GetKeyUp("d"))
        {

            userOutput[2] = 0;
            RecordAction();
        }


        if (!Input.GetKey("w") && !Input.GetKey("s"))
        {
            userOutput[4] = 1;
        }
        else
        {
            userOutput[4] = 0;
        }
        if (!Input.GetKey("a") && !Input.GetKey("d"))
        {
            
            StraightenOut();
            userOutput[1] = 1;
        }
        else
        {
            userOutput[1] = 0;
            
        }

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



    //This is used to gather the training images
    [ContextMenu("Save Screenshot")]
    public void SaveImage()
    {
        string path = Application.dataPath + "\\Racecar Training Images\\";

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
    private IEnumerator RecordActions()
    {
        yield return new WaitForSeconds(0.1f);



       

        string userOutputString = "";
        for(int i = 0; i < 6; i++)
        {

            if(i != 5) userOutputString += userOutput[i] + ",";
            if (i == 5) userOutputString += userOutput[i] + "/";
        }
        writer.WriteLine(userOutputString);
        

        


       // SaveImage();

        //Restart the record actions method
        if(playerControlled == true && recordActions == true)
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
    private void RecordAction()
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
