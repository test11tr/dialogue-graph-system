using System.Collections;
using System.Collections.Generic;
using T11.ScriptableObjects;
using UnityEngine;

namespace T11
{
    public class T11Dialogue : MonoBehaviour
    {
        /* Dialogue Scriptable Objects */
        [SerializeField] private T11DialogueContainerSO dialogueContainer;
        [SerializeField] private T11DialogueGroupSO dialogueGroup;
        [SerializeField] private T11DialogueSO dialogue;

        /*Filters*/
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private bool startingDialoguesOnly;

        /* Indexes */
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
    }
}
