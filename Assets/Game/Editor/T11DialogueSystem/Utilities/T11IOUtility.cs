using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace T11.Utilities
{
    using Data;
    using Data.Save;
    using Elements;
    using ScriptableObjects;
    using System.Drawing.Printing;
    using System.IO;
    using T11.Data;
    using UnityEditor.Experimental.GraphView;
    using Windows;

    public static class T11IOUtility
    {
        private static T11GraphView graphView;
        private static string graphFileName;
        private static string containerFolderPath;

        private static List<T11Group> groups;
        private static List<T11Node> nodes;

        private static Dictionary<string, T11DialogueGroupSO> createdDialogueGroups;
        private static Dictionary<string, T11DialogueSO> createdDialogues;

        private static Dictionary<string, T11Group> loadedGroups;
        private static Dictionary<string, T11Node> loadedNodes;

        public static void Initialize(T11GraphView t11GraphView, string graphName)
        {
            graphView = t11GraphView;
            graphFileName = graphName;
            containerFolderPath = $"Assets/Game/T11DialogueSystem/Dialogues/{graphName}";
            groups = new List<T11Group>();
            nodes = new List<T11Node>();
            createdDialogueGroups = new Dictionary<string, T11DialogueGroupSO>();
            createdDialogues = new Dictionary<string, T11DialogueSO>();
            loadedGroups = new Dictionary<string, T11Group>();
            loadedNodes = new Dictionary<string, T11Node>();
        }

        public static void Remove()
        {
            GetElementsFromGraphView();
            RemoveAsset("Assets/Game/Editor/T11DialogueSystem/Graphs", $"{graphFileName}Graph");
            RemoveFolder($"Assets/Game/T11DialogueSystem/Dialogues/{graphFileName}");
            AssetDatabase.Refresh();
        }

        public static void Save()
        {
            CreateStaticFolders();
            GetElementsFromGraphView();
            T11GraphSaveDataSO graphData = CreateAsset<T11GraphSaveDataSO>("Assets/Game/Editor/T11DialogueSystem/Graphs", $"{graphFileName}Graph");
            graphData.Initialize(graphFileName);
            T11DialogueContainerSO dialogueContainer = CreateAsset<T11DialogueContainerSO>(containerFolderPath, graphFileName);
            dialogueContainer.Initialize(graphFileName);
            SaveGroups(graphData, dialogueContainer);
            SaveNodes(graphData, dialogueContainer);
            SaveAsset(graphData);
            SaveAsset(dialogueContainer);
        }

        private static void SaveNodes(T11GraphSaveDataSO graphData, T11DialogueContainerSO dialogueContainer)
        {
            SerializableDictionary<string, List<string>> groupedNodeNames = new SerializableDictionary<string, List<string>>();
            List<string> ungroupedNodeNames = new List<string>();

            foreach (T11Node node in nodes)
            {
                SaveNodeToGraph(node, graphData);
                SaveNodeToScriptableObject(node, dialogueContainer);

                if(node.Group != null)
                {
                    groupedNodeNames.AddItem(node.Group.title, node.DialogueName);
                    continue;
                }

                ungroupedNodeNames.Add(node.DialogueName); 
            }

            UpdateDialoguesChoicesConnections();
            UpdateOldGroupedNodes(groupedNodeNames, graphData);
            UpdateOldUngroupedNodes(ungroupedNodeNames, graphData);
        }

        private static void SaveNodeToGraph(T11Node node, T11GraphSaveDataSO graphData)
        {
            List<T11ChoiceSaveData> choices = CloneNodeChoices(node.Choices);

            T11NodeSaveData nodeData = new T11NodeSaveData()
            {
                ID = node.ID,
                Name = node.DialogueName,
                Choices = choices,
                Text = node.Text,
                GroupID = node.Group?.ID,
                DialogueType = node.DialogueType,
                Position = node.GetPosition().position
            };

            graphData.Nodes.Add(nodeData);
        }

        private static List<T11ChoiceSaveData> CloneNodeChoices(List<T11ChoiceSaveData> nodeChoices)
        {
            List<T11ChoiceSaveData> choices = new List<T11ChoiceSaveData>();

            foreach (T11ChoiceSaveData choice in nodeChoices)
            {
                T11ChoiceSaveData choiceData = new T11ChoiceSaveData()
                {
                    Text = choice.Text,
                    NodeID = choice.NodeID,
                };

                choices.Add(choiceData);
            }

            return choices;
        }

        private static void SaveNodeToScriptableObject(T11Node node, T11DialogueContainerSO dialogueContainer)
        {
            T11DialogueSO dialogue;

            if(node.Group != null)
            {
                dialogue = CreateAsset<T11DialogueSO>($"{containerFolderPath}/Groups/{node.Group.title}/Dialogues", node.DialogueName);
                dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.Group.ID], dialogue);
            }
            else
            {
                dialogue = CreateAsset<T11DialogueSO>($"{containerFolderPath}/Global/Dialogues", node.DialogueName);
                dialogueContainer.UngroupedDialogues.Add(dialogue);
            }

            dialogue.Initialize(
                node.DialogueName,
                node.Text,
                ConvertNodeChoicesToDialogueChoices(node.Choices),
                node.DialogueType,
                node.IsStartingNode()
            );

            createdDialogues.Add(node.ID, dialogue);

            SaveAsset( dialogue );
        }

        private static void SaveGroups(T11GraphSaveDataSO graphData, T11DialogueContainerSO dialogueContainer)
        {
            List<string> groupNames = new List<string>();

            foreach (T11Group group in groups)
            {
                SaveGroupToGraph(group, graphData);
                SaveGroupToScriptableObject(group, dialogueContainer);
                groupNames.Add(group.title);
            }

            UpdateOldGroups(groupNames, graphData);
        }

        private static void SaveGroupToGraph(T11Group group, T11GraphSaveDataSO graphData)
        {
            T11GroupSaveData groupdata = new T11GroupSaveData()
            {
                ID = group.ID,
                Name = group.title,
                Position = group.GetPosition().position
            };

            graphData.Groups.Add(groupdata);
        }

        private static void SaveGroupToScriptableObject(T11Group group, T11DialogueContainerSO dialogueContainer)
        {
            string groupName = group.title;
            CreateFolder($"{containerFolderPath}/Groups", groupName);
            CreateFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

            T11DialogueGroupSO dialogueGroup = CreateAsset<T11DialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

            dialogueGroup.Initialize(groupName);
            createdDialogueGroups.Add(group.ID, dialogueGroup);
            dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<T11DialogueSO>());
            SaveAsset(dialogueGroup);

        }

        private static void UpdateOldGroups(List<string> currentGroupnames, T11GraphSaveDataSO graphData)
        {
            if(graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
            {
                List<string> GroupsToRemove = graphData.OldGroupNames.Except(currentGroupnames).ToList();

                foreach (string groupToRemove in GroupsToRemove)
                {
                    RemoveFolder($"{containerFolderPath}/Groups/{groupToRemove}");
                }
            }

            graphData.OldGroupNames = new List<string>(currentGroupnames);
        }

        private static List<T11DialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<T11ChoiceSaveData> nodeChoices)
        {
            List<T11DialogueChoiceData> dialogueChoices = new List<T11DialogueChoiceData>();
            foreach(T11ChoiceSaveData nodeChoice in nodeChoices)
            {
                T11DialogueChoiceData choiceData = new T11DialogueChoiceData() 
                {
                    Text = nodeChoice.Text,
                };

                dialogueChoices.Add(choiceData);
            }

            return dialogueChoices;
        }

        private static void UpdateDialoguesChoicesConnections()
        {
            foreach (T11Node node in nodes)
            {
                T11DialogueSO dialogue = createdDialogues[node.ID];

                for(int choiceIndex = 0; choiceIndex < node.Choices.Count; choiceIndex++)
                {
                    T11ChoiceSaveData nodeChoice = node.Choices[choiceIndex];

                    if(string.IsNullOrEmpty(nodeChoice.NodeID)) 
                    {
                        continue;
                    }

                    dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.NodeID];
                    SaveAsset(dialogue);
                }
            }
        }

        private static void UpdateOldUngroupedNodes(List<string> currentUngroupedNodeNames, T11GraphSaveDataSO graphData)
        {
            if(graphData.OldUngrouopedNodeNames != null && graphData.OldUngrouopedNodeNames.Count != 0) 
            {
                List<string> nodesToRemove = graphData.OldUngrouopedNodeNames.Except(currentUngroupedNodeNames).ToList();

                foreach(string nodeToRemove in nodesToRemove)
                {
                    RemoveAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
                }
            }

            graphData.OldUngrouopedNodeNames = new List<string>(currentUngroupedNodeNames);
        }

        private static void UpdateOldGroupedNodes(SerializableDictionary<string, List<string>> currentGroupedNodeNames, T11GraphSaveDataSO graphData)
        {
            if(graphData.OldGroupedNodeNames != null && graphData.OldGroupedNodeNames.Count != 0)
            {
                foreach(KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames)
                {
                    List<string> nodesToRemove = new List<string>();

                    if (currentGroupedNodeNames.ContainsKey(oldGroupedNode.Key))
                    {
                        nodesToRemove = oldGroupedNode.Value.Except(currentGroupedNodeNames[oldGroupedNode.Key]).ToList();
                    }

                    foreach(string nodeToRemove in nodesToRemove)
                    {
                        RemoveAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                    }
                }
            }

            graphData.OldGroupedNodeNames = new SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
        }

        public static void Load()
        {
            T11GraphSaveDataSO graphData = LoadAsset<T11GraphSaveDataSO>("Assets/Game/Editor/T11DialogueSystem/Graphs", graphFileName);

            if(graphData == null)
            {
                EditorUtility.DisplayDialog(
                    "Couldn't load the file!",
                    "The file at the following path could not be found: \n\n" +
                    $"Assets/Game/Editor/T11DialogueSystem/Graphs/{graphFileName}\n\n" +
                    "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                    "Okey."
                );

                return;
            }
            T11EditorWindow.UpdateFileName(graphData.FileName);
            LoadGroups(graphData.Groups);
            LoadNodes(graphData.Nodes);
            LoadNodesConnections();
        }

        private static void LoadGroups(List<T11GroupSaveData> groups)
        {
            foreach(T11GroupSaveData groupData in groups)
            {
                T11Group group = graphView.CreateGroup(groupData.Name, groupData.Position);
                group.ID  = groupData.ID;
                loadedGroups.Add(group.ID, group);
            }
        }

        private static void LoadNodes(List<T11NodeSaveData> nodes)
        {
            foreach(T11NodeSaveData nodeData in nodes)
            {
                List<T11ChoiceSaveData> choices = CloneNodeChoices(nodeData.Choices);
                T11Node node = graphView.CreateNode(nodeData.Name, nodeData.DialogueType, nodeData.Position, false);
                node.ID = nodeData.ID;
                node.Choices = choices;
                node.Text = nodeData.Text;
                node.Draw();
                graphView.AddElement(node);
                loadedNodes.Add(node.ID, node);
                if (string.IsNullOrEmpty(nodeData.GroupID))
                {
                    continue;
                }
                T11Group group = loadedGroups[nodeData.GroupID];
                node.Group = group;
                group.AddElement(node);
            }
        }

        private static void LoadNodesConnections()
        {
            foreach(KeyValuePair<string, T11Node> loadedNode in loadedNodes)
            {
                foreach(Port choicePort in loadedNode.Value.outputContainer.Children())
                {
                    T11ChoiceSaveData choiceData = (T11ChoiceSaveData)choicePort.userData;
                    if(string.IsNullOrEmpty(choiceData.NodeID))
                    {
                        continue;
                    }

                    T11Node nextNode = loadedNodes[choiceData.NodeID];
                    Port nextNodeInputPort = (Port)nextNode.inputContainer.Children().First();
                    Edge edge = choicePort.ConnectTo(nextNodeInputPort);
                    graphView.AddElement(edge);
                    loadedNode.Value.RefreshPorts();
                }
            }
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
        
        private static void CreateFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder($"{path}/{folderName}"))
            {
                return;
            }

            AssetDatabase.CreateFolder(path, folderName);
        }

        private static void RemoveFolder(string fullPath)
        {
            Debug.Log(fullPath);

            FileUtil.DeleteFileOrDirectory($"{fullPath}.meta");
            FileUtil.DeleteFileOrDirectory($"{fullPath}/");
        }

        private static T CreateAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            T asset = LoadAsset<T>(path, assetName);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, fullPath);
            }

            return asset;
        }

        private static T LoadAsset<T>(string path, string assetName) where T : ScriptableObject
        {
            string fullPath = $"{path}/{assetName}.asset";
            return AssetDatabase.LoadAssetAtPath<T>(fullPath);
        }

        private static void RemoveAsset(string path, string assetName)
        {
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
            AssetDatabase.DeleteAsset($"{path}/{assetName}.asset.meta");
        }

        private static void SaveAsset(UnityEngine.Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
