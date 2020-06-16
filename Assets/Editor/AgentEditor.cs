using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Agent), true)]
public class AgentEditor : Editor
{
    public override void OnInspectorGUI()
    {


        serializedObject.Update();
        Agent agent = (Agent)target;

        if (GUILayout.Button("Kill Agent"))
        {
            agent.KillAgent();
        }

        if (GUILayout.Button("Make best of generation"))
        {
            agent.MakeBest();
        }

        if (GUILayout.Button("Save Network"))
        {
            agent.SaveNetwork();
        }
        GUILayout.Space(20);

        if (agent._trainingImages.Count >= 1)
        {
            if (agent._trainingAnswers != null)
            {
                if (GUILayout.Button("Train Agent"))
                {
                    agent.Train();
                }
            }
            else
            {
                GUILayout.Label("Attach an answers file to train the model using back-propogation");
            }
        }
        else
        {
            GUILayout.Label("Attach at least one training image to train the model using back-propogation");
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Take Screenshot"))
        {
            agent.SaveImage();
        }

        base.OnInspectorGUI();

       

        





        serializedObject.ApplyModifiedProperties();
    }
}
