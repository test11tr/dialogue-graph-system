using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace T11.Elements
{
    public class T11Group : Group
    {
        public string oldTitle;
        private Color defaultBorderColor;
        private float defaultBorderWidth;

        public T11Group(string groupTitle, Vector2 position)
        {
            title = groupTitle;
            oldTitle = groupTitle;
            SetPosition(new Rect(position, Vector2.zero));
            defaultBorderColor = contentContainer.style.borderBottomColor.value;
            defaultBorderWidth = contentContainer.style.borderBottomWidth.value;
        }

        public void SetErrorStyle(Color color) 
        {
            contentContainer.style.borderBottomColor = color;
            contentContainer.style.borderBottomWidth = 2f;
        }

        public void ResetStyle()
        {
            contentContainer.style.borderBottomColor = defaultBorderColor;
            contentContainer.style.borderBottomWidth = defaultBorderWidth;
        }
    }
}
