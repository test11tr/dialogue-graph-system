using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace T11.Utilities
{
    using Elements;
    using Windows;

    public static class T11IOUtility
    {
        private static T11GraphView graphView;
        private static string graphFileName;
        private static string containerFolderPath;

        private static List<T11Group> groups;
        private static List<T11Node> nodes;

        public static void Initialize(T11GraphView t11GraphView, string graphName)
        {
            graphView = t11GraphView;
            graphFileName = graphName;
            containerFolderPath = $"Assets/Game/T11DialogueSystem/Dialogues/{graphFileName}";
            groups = new List<T11Group>();
            nodes = new List<T11Node>();
        }

        public static void Save()
        {
            CreateStaticFolders();
            GetElementsFromGraphView();
        }

        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }

        private static void CreateStaticFolders()
        {
            CreateFolder("Assets/Game/Editor/T11DialogueSystem", "Graphs");

            CreateFolder("Assets/Game", "T11DialogueSystem");
            CreateFolder("Assets/Game/T11DialogueSystem", "Dialogues");
            CreateFolder("Assets/Game/T11DialogueSystem/Dialogues", graphFileName);
            CreateFolder(containerFolderPath, "Global");
            CreateFolder(containerFolderPath, "Groups");
            CreateFolder($"{containerFolderPath}/Global", "Dialogues");
        }

        private static void GetElementsFromGraphView()
        {
            graphView.graphElements.ForEach(graphElement =>
            {
                Type groupType = typeof(T11Group);
                
                if(graphElement is T11Node node)
                {
                    nodes.Add(node);
                    return;
                }

                if(graphElement.GetType() == groupType)
                {
                    T11Group group = (T11Group)graphElement;
                    groups.Add(group);
                    return;
                }
            });
        }        
    }
}
