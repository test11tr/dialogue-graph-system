using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace T11.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;

    public class T11Node : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public T11DialogueType DialogueType { get; set; }
        private T11GraphView graphView;
        private Color defaultBackgroundColor;

        public virtual void Initialize(T11GraphView t11GraphView, Vector2 position)
        {
            DialogueName = "Dialogue Name";
            Choices = new List<string>();
            Text = "Dialogue Text.";

            graphView = t11GraphView;
            defaultBackgroundColor = new Color(29f / 255, 29f / 255, 30f / 255);
            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("t11-node__main-container");
            extensionContainer.AddToClassList("t11-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE Container */
            TextField DialogueNameTextField = t11ElementUtility.CreateTextField(DialogueName, callback =>
            {
                graphView.RemoveUngroupedNode(this);
                DialogueName = callback.newValue;
                graphView.AddUngroupedNode(this);
            });

            DialogueNameTextField.AddClasses(
                "t11-node__textfield",
                "t11-node__filename-textfield",
                "t11-node__textfield_hidden"
            );

            titleContainer.Insert(0, DialogueNameTextField);

            /* INPUT Container */
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            /* EXTENSION Container */
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("t11-node__custom-data-container");

            Foldout textFoldout = t11ElementUtility.CreateFoldout("Dialogue Details");

            TextField textTextField = t11ElementUtility.CreateTextArea(Text);

            textTextField.AddClasses(
                "t11-node__textfield",
                "t11-node__quote-textfield"
            );

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}
