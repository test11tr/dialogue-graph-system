using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace T11.Elements
{
    using Enumerations;
    using UnityEngine.UIElements;

    public class T11Node : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public T11DialogueType DialogueType { get; set; }

        public virtual void Initialize(Vector2 position)
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text.";

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("t11-node__main-container");
            extensionContainer.AddToClassList("t11-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE Container */
            TextField DialogueNameTextField = new TextField()
            {
                value = DialogueName,
            };
            DialogueNameTextField.AddToClassList("t11-node__textfield");
            DialogueNameTextField.AddToClassList("t11-node__filename-textfield");
            DialogueNameTextField.AddToClassList("t11-node__textfield_hidden");
            titleContainer.Insert(0, DialogueNameTextField);

            /* INPUT Container */
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "Dialogue Connection";
            inputContainer.Add(inputPort);

            /* EXTENSION Container */
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("t11-node__custom-data-container");

            Foldout textFoldout = new Foldout() 
            {
                text = "Dialogue Text"
            };

            TextField textTextField = new TextField()
            {
                value = Text
            };
            textTextField.AddToClassList("t11-node__textfield");
            textTextField.AddToClassList("t11-node__quote-textfield");
            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }
    }
}
