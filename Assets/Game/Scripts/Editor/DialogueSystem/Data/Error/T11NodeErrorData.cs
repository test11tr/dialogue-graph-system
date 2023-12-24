using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.Data.Error
{
    using Elements;

    public class T11NodeErrorData
    {
        public T11ErrorData ErrorData { get; set; }
        public List<T11Node> Nodes { get; set; }

        public T11NodeErrorData()
        {
            ErrorData = new T11ErrorData();
            Nodes = new List<T11Node>();
        }
    }
}
