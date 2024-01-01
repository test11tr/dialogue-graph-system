using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.Data.Save
{
    [Serializable]
    public class T11ChoiceSaveData
    {
        [field: SerializeField] public string Text { get ; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
}
