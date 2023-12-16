using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using T11.Enumerations;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace T11.Elements
{
    public class T11MultipleChoiceNode : T11Node
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            DialogueType = T11DialogueType.MultipleChoice;
            Choices.Add("New Choice");
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN Container */

            Button addChoiceButton = new Button()
            {
                text = "Add Choice"
            };
            addChoiceButton.AddToClassList("t11-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT Container */

            foreach (var choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                choicePort.portName = "";

                Button deleteChoiceButton = new Button()
                {
                    text = "X"
                };
                deleteChoiceButton.AddToClassList("t11-node__button");

                TextField choiceTextField = new TextField()
                {
                    value = choice
                };
                choiceTextField.AddToClassList("t11-node__textfield");
                choiceTextField.AddToClassList("t11-node__choice-textfield");
                choiceTextField.AddToClassList("t11-node__textfield_hidden");
                choiceTextField.style.flexDirection = FlexDirection.Column;

                choicePort.Add(choiceTextField);
                choicePort.Add(deleteChoiceButton);
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }
    }
}
