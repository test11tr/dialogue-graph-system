using System.Collections;
using System.Collections.Generic;
using T11.Elements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace T11.Data.Error
{
    public class T11GroupErrorData
    {
        public T11ErrorData ErrorData { get; set; }
        public List<T11Group> Groups { get; set; }

        public T11GroupErrorData() 
        {
            ErrorData = new T11ErrorData();
            Groups = new List<T11Group>();
        }
    }
}

