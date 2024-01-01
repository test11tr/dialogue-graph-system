using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.ScriptableObjects
{
    using Enumerations;
    using Data;

    public class T11DialogueSO : ScriptableObject
    {
        [field: SerializeField] public string DialogueName { get; set; }
        [field: SerializeField] [field: TextArea()] public string Text { get; set; }
        [field: SerializeField] public List<T11DialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public T11DialogueType DialogueType { get; set; }
        [field: SerializeField] public bool IsStartingDialogue { get; set; }

        public void Initialize(string dialogueName, string text, List<T11DialogueChoiceData> choices, T11DialogueType dialogueType, bool isStartingDialogue)
        {
            DialogueName = dialogueName;
            Text = text;
            Choices = choices;
            DialogueType = dialogueType;
            IsStartingDialogue = isStartingDialogue;
        }
    }
}
