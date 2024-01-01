using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.Data.Save
{
    public class T11GraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName {  get; set; }
        [field: SerializeField] public List<T11GroupSaveData> Groups { get; set; }
        [field: SerializeField] public List<T11NodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldGroupNames { get; set; }
        [field: SerializeField] public List<string> OldUngrouopedNodeNames { get; set; }
        [field: SerializeField] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            Groups = new List<T11GroupSaveData>();
            Nodes = new List<T11NodeSaveData>();
        }
    }
}
