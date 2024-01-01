using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.ScriptableObjects
{
    public class T11DialogueContainerSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public SerializableDictionary<T11DialogueGroupSO, List<T11DialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField] public List<T11DialogueSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueGroups = new SerializableDictionary<T11DialogueGroupSO, List<T11DialogueSO>>();
            UngroupedDialogues = new List<T11DialogueSO>();
        }
    }
}
