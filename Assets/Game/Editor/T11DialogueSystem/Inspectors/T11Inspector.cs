using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace T11.Inspectors
{
    using Utilities;
    
    [CustomEditor(typeof(T11Dialogue))]
    public class T11Inspector : Editor
    {
        /* Dialogue Scriptable Object */
        private SerializedProperty dialogueContainerProperty;
        private SerializedProperty dialogueGroupProperty;
        private SerializedProperty dialogueProperty;

        /*Filters*/
        private SerializedProperty groupedDialoguesProperty;
        private SerializedProperty startingDialoguesOnlyProperty;

        /* Indexes */
        private SerializedProperty selectedDialogueGroupIndexProperty;
        private SerializedProperty selectedDialogueIndexProperty;

        private void OnEnable()
        {
            dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
            dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
            dialogueProperty = serializedObject.FindProperty("dialogue");

            groupedDialoguesProperty = serializedObject.FindProperty("groupedDialogues");
            startingDialoguesOnlyProperty = serializedObject.FindProperty("startingDialoguesOnly");

            selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
            selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDialogueContainerArea();
            if (dialogueContainerProperty.objectReferenceValue == null)
            {
                StopDrawing("Select a Dialogue Container to see the rest of Inspector.");
                return;
            }
            DrawFiltersArea();
            DrawDialogueGroupArea();
            DrawDialogueArea();
            serializedObject.ApplyModifiedProperties();
        }      

        private void DrawDialogueContainerArea()
        {
            T11InspectorUtility.DrawHeader("Dialogue Container");
            dialogueContainerProperty.DrawPropertyField(this);
            T11InspectorUtility.DrawSpace();
        }

        private void DrawFiltersArea()
        {
            T11InspectorUtility.DrawHeader("Filters");
            groupedDialoguesProperty.DrawPropertyField(this);
            startingDialoguesOnlyProperty.DrawPropertyField(this);
            T11InspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea()
        {
            T11InspectorUtility.DrawHeader("Dialogue Group");
            selectedDialogueGroupIndexProperty.intValue = T11InspectorUtility.DrawPopup("Dialogue Group", selectedDialogueGroupIndexProperty, new string[] { });
            dialogueGroupProperty.DrawPropertyField(this);
            T11InspectorUtility.DrawSpace();
        }

        private void DrawDialogueArea()
        {
            T11InspectorUtility.DrawHeader("Dialogue");
            selectedDialogueIndexProperty.intValue = T11InspectorUtility.DrawPopup("Dialogue", selectedDialogueIndexProperty, new string[] { });
            dialogueProperty.DrawPropertyField(this);
        }

        private void StopDrawing(string reason)
        {
            T11InspectorUtility.DrawHelpBox(reason);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
