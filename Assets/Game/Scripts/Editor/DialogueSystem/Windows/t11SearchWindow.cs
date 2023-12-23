using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace T11.Windows
{
    using Elements;
    using Enumerations;

    public class t11SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private T11GraphView graphView;
        private Texture2D indentationIcon;

        public void Initialize(T11GraphView t11GraphView)
        {
            graphView = t11GraphView;
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    userData = T11DialogueType.SingleChoice,
                    level = 2
                },
                new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon))
                {
                    userData = T11DialogueType.MultipleChoice,
                    level = 2
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    userData = new Group(),
                    level = 2
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);

            switch (SearchTreeEntry.userData) {
                case T11DialogueType.SingleChoice:
                {
                    T11SingleChoiceNode singleChoiceNode = (T11SingleChoiceNode)graphView.CreateNode(T11DialogueType.SingleChoice, localMousePosition);
                    graphView.AddElement(singleChoiceNode);
                    return true;
                }
                case T11DialogueType.MultipleChoice:
                {
                    T11MultipleChoiceNode multipleChoiceNode = (T11MultipleChoiceNode)graphView.CreateNode(T11DialogueType.MultipleChoice, localMousePosition);
                    graphView.AddElement(multipleChoiceNode);
                    return true;
                }
                case Group _:
                {
                    Group group = graphView.CreateGroup("Dialogue Group", localMousePosition);
                    graphView.AddElement(group);
                    return true;
                }

                default: return false;
            }
        }
    }
}
