using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using T11.Enumerations;
using T11.Utilities;
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

            Button addChoiceButton = t11ElementUtility.CreateButton("Add Choice", () =>
            {
                Port choicePort = CreateChoicePort("New Choice");
                Choices.Add("New Choice");
                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("t11-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT Container */

            foreach (var choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();

            Button deleteChoiceButton = t11ElementUtility.CreateButton("X");

            deleteChoiceButton.AddToClassList("t11-node__button");

            TextField choiceTextField = t11ElementUtility.CreateTextField(choice);

            choiceTextField.AddClasses(
                "t11-node__textfield",
                "t11-node__choice-textfield",
                "t11-node__textfield_hidden"
            );

            choiceTextField.style.flexDirection = FlexDirection.Column;

            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        }
    }
}
