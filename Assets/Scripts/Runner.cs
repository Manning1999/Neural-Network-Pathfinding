using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runner : Agent
{



    System.DateTime startTime = System.DateTime.Now;

    [SerializeField]
    private LayerMask layerMask;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        inputs = new float[5];    
    }

    // Update is called once per frame
    protected override void Update()
    {
        if(initialized == false)
        {

        }
        else if(stopped == false)
        {


            //These raycasts are used to get information about the agent's surroundings and are fed into the neural network
            #region -----Raycasts-----
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


            #endregion

            //Constantly move the agent forward
            transform.position += transform.forward * 1.3f * Time.deltaTime;

            //The highest output from the network is used to determine which way to go
            //0 = left, 1 == forward, 2 == right
            results = net.FeedForward(inputs);
            

           
            results = Sigmoid(results);


            #region -----Output Rotation-----
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



            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + rotation, 0);


            net.AddFitness((inputs[0] + inputs[1] + inputs[2] + inputs[3] + inputs[4]) / 5);
            #endregion
        }
    }


    public void OnCollisionEnter(Collision col)
    {

        //if the agent hits a wall then kill it
        if(col.gameObject.tag == "Wall" && stopped == false)
        {

            KillAgent();

        }    
    }


   

    public override void KillAgent()
    {
        base.KillAgent();
        stopped = true;
        transform.GetComponent<Renderer>().material.color = Color.red;
    }


   

    public override void Reset()
    {
        base.Reset();
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);
        transform.GetComponent<Renderer>().material.color = Color.green;
        
    }


    //If the agent is clicked, tell the UI controller that this is the currently selected agent. The UI will then put a marker over it and the UI 
    //buttons pressed will affect this agent
    public void OnMouseDown()
    {
        UIController.Instance.SelectAgent(transform.GetComponent<Runner>());
    }


   


}
