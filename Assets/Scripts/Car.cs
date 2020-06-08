using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{


    NeuralNetwork net;
    public NeuralNetwork _net { get { return net; } private set { net = value; } }

    bool initialized = false;



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

    float[] inputs;

    [SerializeField]
    private float accelerationSpeed = 6;

    [SerializeField]
    private float maxSpeed = 80;

    [SerializeField]
    [Tooltip("This is the maximum for how much the steering wheels can rotate (Inverses for minimum rotation)")]
    private float maxWheelRotation = 40;

    [SerializeField]
    private bool isOnTrack = true;

    [SerializeField]
    private LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {
        inputs = new float[camera.pixelWidth * camera.pixelHeight + 1];
        rb = transform.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        

        int i = 0;
        
        foreach (Color pixel in RTImage().GetPixels())
        {
            //grayscale the pixels to change them from rgb values to a single float
            inputs[i] = pixel.grayscale;
            i++;
        }

        results = net.FeedForward(inputs);

        //results = Sigmoid(results);


        //controls wheel rotation
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
                //go forward
                
            }
            else
            {

                TurnRight();
            }
        }


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
            if(results[3] > results[5])
            {
                Decelerate();
            }
            else{
                Accelerate();
            }
        }


        CheckIfOnTrack();
               
        ApplyLocalPositionToVisuals(FRWheel);
        ApplyLocalPositionToVisuals(FLWheel);
        ApplyLocalPositionToVisuals(BRWheel);
        ApplyLocalPositionToVisuals(FLWheel);

    }


    private void CheckIfOnTrack()
    {
        RaycastHit hit;

        // Forward Raycast
        if (Physics.Raycast(transform.position + transform.up * 3, transform.TransformDirection(Vector3.down), out hit, 5, layerMask))
        {
            if(hit.transform.tag == "Track")
            {
                isOnTrack = true;
                inputs[3136] = 1;
            }
            else
            {
                isOnTrack = false;
            }
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            inputs[3136] = 0;
            //inputs[0] = hit.distance;
        }
        else
        {
            PopulationController.Instance.RunnerKilled(net);
        }
       
    }


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


    private void Accelerate()
    {
        if (FRWheel.motorTorque <= maxSpeed)
        {
            BRWheel.motorTorque += accelerationSpeed;
            BLWheel.motorTorque += accelerationSpeed;
        }
    }
   
    private void Decelerate()
    {
        if (FRWheel.motorTorque >= -maxSpeed / 5)
        {
            BRWheel.motorTorque -= accelerationSpeed;
            BLWheel.motorTorque -= accelerationSpeed;
        }
    }


    private void TurnRight()
    {
        if (FRWheel.steerAngle <= maxWheelRotation)
        {
            FRWheel.steerAngle += 5;
            FLWheel.steerAngle += 5;
        }

        
    }

    private void TurnLeft()
    {
        if (FRWheel.steerAngle >= -maxWheelRotation)
        {
            FRWheel.steerAngle -= 5;
            FLWheel.steerAngle -= 5;
        }
    }


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

    public void Init(NeuralNetwork net)
    {

        this.net = net;
        initialized = true;
    }

    public float[] Sigmoid(float[] x)
    {

        float[] output = new float[x.Length];

        for (int i = 0; i < x.Length; i++)
        {
            output[i] = 1 / (1 + Mathf.Pow(x[i], -x[i]));
        }
        return output;
    }


    public void Reset()
    {
        transform.position = new Vector3(240.71f, 0.5531964f, 216.5f);
        transform.rotation = Quaternion.Euler(0, -174.285f, 0);

        rb.velocity = new Vector3(0, 0, 0);
        BLWheel.motorTorque = 0;
        BRWheel.motorTorque = 0;

        FRWheel.steerAngle = 0;
        FLWheel.steerAngle = 0;
    }


    private void OnCollisionEnter(Collision col)
    {
       if(col.transform.tag == "Wall")
        {
            PopulationController.Instance.RunnerKilled(net);
        } 
    }

}
