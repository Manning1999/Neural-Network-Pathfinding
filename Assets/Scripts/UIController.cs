using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameObject killButton = null;

    [SerializeField]
    private GameObject bestButton = null;

    [SerializeField]
    private GameObject selectedRunnerIcon = null;

    [SerializeField]
    private Runner selectedRunner = null;

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


    public void SelectRunner(Runner runner)
    {
        if (runner == null || selectedRunner != runner)
        {
            selectedRunner = runner;
            killButton.SetActive(true);
            bestButton.SetActive(true);
        }
        else if(selectedRunner == runner)
        {
            selectedRunner = null;
            selectedRunnerIcon.transform.GetComponent<RectTransform>().position = new Vector3(-9999, -9999, -9999);
            killButton.SetActive(false);
            bestButton.SetActive(false);
        }  
    }


    public void KillRunner()
    {
        selectedRunner.KillRunner();
    }

    public void KillAllRunners()
    {
        foreach(Runner runner in FindObjectsOfType<Runner>())
        {
            runner.KillRunner();
        }
    }

    public void MakeRunnerBest()
    {
        PopulationController.Instance.SetBest(selectedRunner._net);
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
        if (selectedRunner != null)
        {
            selectedRunnerIcon.transform.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(selectedRunner.transform.position);
        }
       
    }
}
