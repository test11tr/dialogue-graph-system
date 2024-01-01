using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T11.Data.Error
{
    public class T11ErrorData
    {
        public Color Color { get; set; }

        public T11ErrorData()
        {
            GenerateRandomColor();
        }

        private void GenerateRandomColor()
        {
            Color = new Color32
            (
                (byte) Random.Range(65,256),
                (byte) Random.Range(50,176),
                (byte) Random.Range(50,176),
                255
            );
        }
    }
}
