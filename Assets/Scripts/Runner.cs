using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : MonoBehaviour
{

    NeuralNetwork net;
    public NeuralNetwork _net { get { return net; } private set { net = value; } }

    bool initialized = false;

    [SerializeField]
    float[] inputs = new float[5];

    [SerializeField]
    float[] results;

    [SerializeField]
    bool stopped = false;

    System.DateTime startTime = System.DateTime.Now;

    [SerializeField]
    private LayerMask layerMask;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized == false)
        {

        }
        else if(stopped == false)
        {

            RaycastHit hit;

            // Forward Raycast
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 5, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.green);
                inputs[0] = hit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.white);
                inputs[0] = 5;
            }

            // Left Raycast
            if (Physics.Raycast(transform.position, -transform.TransformDirection(Vector3.right), out hit, 3, layerMask))
            {
                Debug.DrawRay(transform.position, -transform.TransformDirection(Vector3.right) * hit.distance, Color.green);
                inputs[1] = hit.distance;
            }
            else
            {
               
                Debug.DrawRay(transform.position, -transform.TransformDirection(Vector3.right) * 5, Color.white);
                inputs[1] = 5;
            }

            // Right Raycast
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out hit, 5, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * hit.distance, Color.green);
                inputs[2] = hit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.right) * 5, Color.white);
                inputs[2] = 5;
            }

            // Forward-Right Raycast
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right), out hit, 5, layerMask))
            {
                Debug.DrawRay(transform.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)) * hit.distance, Color.green);
                inputs[3] = hit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.white);
                inputs[3] = 5;
            }

            // Forward-Right Raycast
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward) + -transform.TransformDirection(Vector3.right), out hit, 5, layerMask))
            {
                Debug.DrawRay(transform.position, (transform.TransformDirection(Vector3.forward) + -transform.TransformDirection(Vector3.right)) * hit.distance, Color.green);
                inputs[4] = hit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 5, Color.white);
                inputs[4] = 5;
            }


            transform.position += transform.forward * 1.3f * Time.deltaTime;

            //0 = left, 1 == forward, 2 == right
            results = net.FeedForward(inputs);
            

           
            results = Sigmoid(results);
           



            float rotation = 0;

            if(results[0] > results[1])
            {
                if(results[0] > results[2])
                {
                    //turn left 
                    rotation = -15;
                }
                else
                {
                    //turn right
                    rotation = 15;
                }
            }
            else
            {
                if(results[1] > results[2])
                {
                    //go forward
                    rotation = 0;
                }
                else
                {

                    //turn right
                    rotation = 15;
                }
            }  

           // rotation = results[0] * 360;


            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + rotation, 0);


            net.AddFitness((inputs[0] + inputs[1] + inputs[2] + inputs[3] + inputs[4]) / 5);
        }
    }


    public void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Wall" && stopped == false)
        {

            KillRunner();

        }    
    }


    public void OnCollisionExit(Collision col)
    {
        if (col.gameObject.tag == "Wall")
        {
            transform.GetComponent<Renderer>().material.color = Color.green;
        }
    }



    public void KillRunner()
    {
        stopped = true;

        PopulationController.Instance.RunnerKilled(net);

        transform.GetComponent<Renderer>().material.color = Color.red;
    }


    public void Init(NeuralNetwork net)
    {

        this.net = net;
        initialized = true;
    }


    public void Reset()
    {
        stopped = false;
        transform.GetComponent<Renderer>().material.color = Color.green;
        net.SetFitness(0);
    }



    [ContextMenu("Print Weights")]
    public void PrintWeights()
    {
        PopulationController.WriteString(net.GetWeightsString());

       
    }


    public float[] Sigmoid(float[] x)
    {

        float[] output = new float[x.Length];

        for (int i = 0; i < x.Length; i++) {
           output[i] = 1 / (1 + Mathf.Pow(x[i], -x[i]));
        }
        return output;
    }

    [ContextMenu("Print Sigmoided Inputs")]
    public void PrintInputs()
    {
        float[] newInputs = Sigmoid(inputs);

        foreach (float num in newInputs)
        {
            Debug.Log(num);
        }
    }


    public void OnMouseDown()
    {
        UIController.Instance.SelectRunner(transform.GetComponent<Runner>());
    }


   


}
