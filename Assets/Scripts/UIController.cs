using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameObject killButton = null;

    [SerializeField]
    private GameObject makeBestButton = null;

    [SerializeField]
    private GameObject selectedAgentIcon = null;

    [SerializeField]
    private Agent selectedAgent = null;
    

    private bool paused = false;

    //create singleton - This means that there can only be one of this in the scene at any given time
    public static UIController instance;
    private static UIController _instance;

    public static UIController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<UIController>();
            }

            return _instance;
        }
    }


    public void SelectAgent(Agent agent)
    {
        if (agent == null || selectedAgent != agent)
        {
            selectedAgent = agent;
            killButton.SetActive(true);
            makeBestButton.SetActive(true);
        }
        else if(selectedAgent == agent)
        {
            selectedAgent = null;
            selectedAgentIcon.transform.GetComponent<RectTransform>().position = new Vector3(-9999, -9999, -9999);
            killButton.SetActive(false);
            makeBestButton.SetActive(false);
        }  
    }


   

    public void KillAgent()
    {
        if(selectedAgent != null) selectedAgent.KillAgent();
        
    }


    public void KillAllAgents()
    {
        foreach(Runner runner in FindObjectsOfType<Runner>())
        {
            runner.KillAgent();
        }
    }


    public void MakeAgentBest()
    {
        PopulationController.Instance.SetBest(selectedAgent._net);
    }

        
    public void PauseTime()
    {
        paused = !paused;

        if (paused == true)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

    }


    public void Update()
    {
        if (selectedAgent != null)
        {
            selectedAgent.transform.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(selectedAgent.transform.position);
        } 
    }
}
