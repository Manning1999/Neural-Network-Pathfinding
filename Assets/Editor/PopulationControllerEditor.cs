using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PopulationController))]
public class PopulationControllerEditor : Editor
{

    SerializedProperty mutationRate;

    SerializedProperty neuronsPerLayer;

    SerializedProperty minLayer, maxLayer, minNeuron, maxNeuron, minConnection, maxConnection;

    public void OnEnable()
    {
        mutationRate = serializedObject.FindProperty("mutationRate");

        neuronsPerLayer = serializedObject.FindProperty("defaultNeuronsPerLayer");

        minLayer = serializedObject.FindProperty("minLayer");
        maxLayer = serializedObject.FindProperty("maxLayer");
        minNeuron = serializedObject.FindProperty("minNeuron");
        maxNeuron = serializedObject.FindProperty("maxNeuron");
        minConnection = serializedObject.FindProperty("minConnection");
        maxConnection = serializedObject.FindProperty("maxConnection");
    }


    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        serializedObject.Update();

        PopulationController popController = (PopulationController)target;



        if (GUILayout.Button("Kill All Agents"))
        {
            popController.KillAll();
        }
        GUILayout.Space(20);




        popController.reproductionMethod = (PopulationController.ReproductionMethod)EditorGUILayout.EnumPopup(new GUIContent("Reproduction Method","This is the method used when transitioning to the next generation of agents.\nSexual: The top two most fit agents will have their weights mixed in various ways in order to create the next generation of agents.\nASexual: The weights from the best performing agent will be copied to all other agents and then they will all be mutated by the specified amount to get genetic diversity"), popController.reproductionMethod);

        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(neuronsPerLayer, true);

        GUILayout.Space(20);

        popController.mutationType = (PopulationController.MutationType)EditorGUILayout.EnumPopup(new GUIContent("Mutation Type: ","Entire network means that the specified number of weights can be mutated from anywhere in the network.\nCustom range means that only weights within the specified range will be mutated"), popController.mutationType);
        if (popController.mutationType == PopulationController.MutationType.CustomRange)
        {
            //Custom mutate options
            EditorGUILayout.LabelField("Custom Mutation Range");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(minLayer, true);
            EditorGUILayout.PropertyField(maxLayer, true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(minNeuron, true);
            EditorGUILayout.PropertyField(maxNeuron, true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(minConnection, true);
            EditorGUILayout.PropertyField(maxConnection, true);
            EditorGUILayout.EndHorizontal();
        }


        GUILayout.Label(new GUIContent("Mutation  Rate:", "This is how many genes should be mutated each time the agents reproduce"));
        mutationRate.intValue = EditorGUILayout.IntSlider(mutationRate.intValue, 1, 999);

        //Applies all changes to variables
        serializedObject.ApplyModifiedProperties();
    }



}
