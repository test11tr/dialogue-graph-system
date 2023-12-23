using System.Collections;
using System.Collections.Generic;
using T11.Enumerations;
using T11.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace T11.Elements
{
    public class T11SingleChoiceNode : T11Node
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            DialogueType = T11DialogueType.SingleChoice;
            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT Container */

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice);
                choicePort.portName = choice;
                outputContainer.Add(choicePort);
            }
            RefreshExpandedState();
        }

    }
}
