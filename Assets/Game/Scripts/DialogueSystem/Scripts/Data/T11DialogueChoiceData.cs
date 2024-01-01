using System.Collections;
using System.Collections.Generic;
using T11.ScriptableObjects;
using UnityEngine;

namespace T11.Data
{
    using ScriptableObjects;
    using System;

    [Serializable]
    public class T11DialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public T11DialogueSO NextDialogue { get; set; }
    }
}
