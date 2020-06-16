using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using B83.Image.BMP;
using UnityEditor;
using UnityEngine;

public class Car : Agent
{


    
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
    private float accelerationSpeed = 500;

    [SerializeField]
    private float maxSpeed = 5000;

    [SerializeField]
    [Tooltip("This is the maximum for how much the steering wheels can rotate (Inverses for minimum rotation)")]
    private float maxWheelRotation = 40;

 
    [SerializeField]
    [Tooltip("This bool simply controls whether or not the car will control it's speed. I made this because I am working on two versions of the car AI. One basic AI that maintains a constant speed while driving and another that will intelligently accelerate and decelerate when going around corners")]
    private bool controlsSpeed = false;


   

   
    //This is the location where the car's will reset to when they all die
    Vector3 startPosition;


    protected override void Start()
    {
        base.Start();
        inputs = new float[camera.pixelWidth * camera.pixelHeight];
        rb = transform.GetComponent<Rigidbody>();
        startPosition = transform.position;

        if(controlsSpeed == false)
        {
            maxSpeed = 70;
            accelerationSpeed = 20;
        }
        
    }


    public override void Init(NeuralNetwork net)
    {
        //If not network size has been specified then a default one will be made
        if (net == null)
        {
            if (savedNetwork == null)
            {
                net = new NeuralNetwork(new int[4] { camera.pixelWidth * camera.pixelHeight, 16, 8, 3 });
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


    protected override void Update()
    {
        base.Update();

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (net != null && playerControlled == false && stopped == false)
        {


            if (controlsSpeed == false)
            {
                Accelerate();
            }

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

           // results = Sigmoid(results);


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


            if (controlsSpeed == true)
            {
                //3 decelerate, 4 maintain speed, 5 accelerate
                if (results[3] > results[4])
                {
                    if (results[3] > results[5])
                    {
                        Decelerate();
                    }
                    else
                    {
                        Accelerate();
                    }
                }
                else
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
            }



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
        

        //Makes the wheels rotate so that they look like the wheel colliders
        ApplyLocalPositionToVisuals(FRWheel);
        ApplyLocalPositionToVisuals(FLWheel);
        ApplyLocalPositionToVisuals(BRWheel);
        ApplyLocalPositionToVisuals(BLWheel);
            

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




    #region -------Car controls--------
    private void Accelerate()
    {
        if (FRWheel.rpm <= maxSpeed)
        {
            BRWheel.motorTorque += accelerationSpeed;
            BLWheel.motorTorque += accelerationSpeed;

            BRWheel.brakeTorque = 0;
            BLWheel.brakeTorque = 0;
            FRWheel.brakeTorque = 0;
            FLWheel.brakeTorque = 0;
        }

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

    }


    private void TurnRight()
    {
        FRWheel.steerAngle = Mathf.Lerp(FRWheel.steerAngle, maxWheelRotation, Time.deltaTime * 1f);
        FLWheel.steerAngle = Mathf.Lerp(FLWheel.steerAngle, maxWheelRotation, Time.deltaTime * 1f);
    }

    private void TurnLeft()
    {

        FRWheel.steerAngle = Mathf.Lerp(FRWheel.steerAngle, -maxWheelRotation, Time.deltaTime * 1f);
        FLWheel.steerAngle = Mathf.Lerp(FLWheel.steerAngle, -maxWheelRotation, Time.deltaTime * 1f);

    }


    private void StraightenOut()
    {
        FRWheel.steerAngle = Mathf.Lerp(FRWheel.steerAngle, 0, 0.5f);
        FLWheel.steerAngle = Mathf.Lerp(FLWheel.steerAngle, 0, 0.5f);
    }
    #endregion


   
    public override void Reset()
    {
        
        transform.position = new Vector3(240.71f, 0.5531964f, 216.5f);
        transform.rotation = Quaternion.Euler(0, -174.285f, 0);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.ResetInertiaTensor();


        BLWheel.motorTorque = 0;
        BRWheel.motorTorque = 0;

        BRWheel.brakeTorque = 0;
        FRWheel.brakeTorque = 0;
        BLWheel.brakeTorque = 0;
        FLWheel.brakeTorque = 0;

        FRWheel.steerAngle = 0;
        FLWheel.steerAngle = 0;
        base.Reset();
    }




    private void OnCollisionEnter(Collision col)
    {
        if (col.transform.tag == "Wall" && net != null && stopped == false)
        {
            Debug.Log("crashed into a wall");
            KillAgent();
        }
    }


   

    





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


    

}
