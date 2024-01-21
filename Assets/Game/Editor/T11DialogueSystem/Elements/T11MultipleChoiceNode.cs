using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using T11.Enumerations;
using T11.Utilities;
using T11.Windows;
using T11.Data.Save;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace T11.Elements
{
    public class T11MultipleChoiceNode : T11Node
    {
        public override void Initialize(string nodeName, T11GraphView t11GraphView, Vector2 position)
        {
            base.Initialize(nodeName, t11GraphView, position);
            DialogueType = T11DialogueType.MultipleChoice;
            T11ChoiceSaveData choiceData = new T11ChoiceSaveData() 
            {
                Text = "New Choice"
            };
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* MAIN Container */

            Button addChoiceButton = T11ElementUtility.CreateButton("Add Choice", () =>
            {
                T11ChoiceSaveData choiceData = new T11ChoiceSaveData()
                {
                    Text = "New Choice"
                };
                Choices.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddToClassList("t11-node__button");

            mainContainer.Insert(1, addChoiceButton);

            /* OUTPUT Container */

            foreach (T11ChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            T11ChoiceSaveData choiceData = (T11ChoiceSaveData)userData;

            Button deleteChoiceButton = T11ElementUtility.CreateButton("X", () =>
            {
                if(Choices.Count == 1)
                {
                    return;
                }

                if(choicePort.connected) 
                {
                    graphView.DeleteElements(choicePort.connections);
                }

                Choices.Remove(choiceData);
                graphView.RemoveElement(choicePort);
            });

            deleteChoiceButton.AddToClassList("t11-node__button");

            TextField choiceTextField = T11ElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

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
