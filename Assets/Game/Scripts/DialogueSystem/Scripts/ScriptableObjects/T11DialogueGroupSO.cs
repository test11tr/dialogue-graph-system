using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.ScriptableObjects
{
    public class T11DialogueGroupSO : ScriptableObject
    {
        [field: SerializeField] public string GroupName { get; set; }  

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }
    }
}
