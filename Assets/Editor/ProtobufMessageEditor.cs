using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor
{


    public class ProtobufMessageEditor : EditorWindow
    {
        private const string EditorTitle = "Protobuf Message Editor";
        private const string EditorMenu = "Tool/Protobuf Message Editor";
        private const string ProtoPath = "Assets\\Plugins\\Protobuf\\MessagePrototype";
        private List<string> m_MessageFileNames;
        private List<string> m_MessageFilePaths;
        private int m_SelectedFile = -1;
        private List<MessageItem> m_MessageItems;
        private ProtobufMessage m_Message;
        private ProtobufMessage m_OriginMessage;
        private Vector2 m_V1 = new Vector2(0, 0);
        private Vector2 m_V2 = new Vector2(0, 0);
        private string[] m_SyntaxList = {
            "proto2",
            "proto3",
        };
        private string[] m_TypeList;

        public class Styles
        {
            public GUIStyle ListItem = new GUIStyle("MenuItem");
            public GUIStyle BoldStyle = new GUIStyle();
            public GUIStyle TitleStyle = new GUIStyle();

            public Styles()
            {
                ListItem.fixedWidth = Screen.width * 0.29f;
                TitleStyle.fontSize = 13;
                TitleStyle.fontStyle = FontStyle.Bold;
                BoldStyle.fontStyle = FontStyle.Bold;
            }
        }
        private Styles m_Styles;

        public ProtobufMessageEditor()
        {
            titleContent = new GUIContent(EditorTitle);
        }
        
        [MenuItem(EditorMenu)]
        public static void OpenEditor()
        {
            GetWindow(typeof(ProtobufMessageEditor));
        }

        private void OnGUI()
        {
            if (m_Styles == null)
            {
                m_Styles = new Styles();
            }

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            //Left
            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.3f));
            m_V1 = GUILayout.BeginScrollView(m_V1, false, false, GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(Screen.height));
            {
                var old = m_SelectedFile;
                m_SelectedFile = GUILayout.SelectionGrid(m_SelectedFile, m_MessageFileNames.ToArray(), 1, m_Styles.ListItem);
                if (old != m_SelectedFile)
                {
                    LoadAndDeserializeFile();
                }
                if (GUILayout.Button("Add new message"))
                {
                    // Add message item
                    CreateMessageFile();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.Space(5);

            //Right
            m_V2 = GUILayout.BeginScrollView(m_V2, false, false, GUILayout.Width(Screen.width * 0.69f), GUILayout.Height(Screen.height * 0.95f));
            GUILayout.BeginVertical();
            

            if (m_Message != null)
            {
                GUILayout.Label(" Base Info", m_Styles.TitleStyle);
                GUILayout.Space(5);
                m_Message.syntax = (Syntax)EditorGUILayout.Popup("syntax", (int)m_Message.syntax, m_SyntaxList);
                m_Message.package = EditorGUILayout.TextField("package", m_Message.package);
                m_Message.name = EditorGUILayout.TextField("message name", m_Message.name);
                GUILayout.Space(5);
                GUILayout.Label(" Message Items", m_Styles.TitleStyle);
                GUILayout.BeginVertical();
                for (var i = 0; i < m_MessageItems.Count; i++)
                {
                    GUILayout.Space(5);
                    var messageItem = m_MessageItems[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label(" Message Item " + (i+1), m_Styles.BoldStyle);
                    messageItem.name = EditorGUILayout.TextField("name", messageItem.name);
                    messageItem.type = (MessageType) EditorGUILayout.Popup("type", (int) messageItem.type, m_TypeList);
                    messageItem.repeated = EditorGUILayout.Toggle("repeated", messageItem.repeated);
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(GUILayout.MaxWidth(100));
                    GUILayout.Space(14);
                    if (GUILayout.Button("Delete", GUILayout.Height(54)))
                    {
                        // Delete message item
                        DeleteMessageItem(i);
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }
                GUILayout.EndVertical();
                
                if (GUILayout.Button("Add a message item"))
                {
                    // Add message item
                    AddMessageItem();
                }
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save message"))
                {
                    // Save message
                    SaveMessage();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("Delete message"))
                {
                    // Delete message
                    DeleteMessage();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        private void SaveMessage()
        {
            string massageStr = m_Message.ToString();
            string newName = ProtoPath + "\\" + m_Message.name + ".proto";
            if (!string.IsNullOrEmpty(newName) && newName != m_MessageFilePaths[m_SelectedFile])
            {
                FileInfo fileInfo = new FileInfo(m_MessageFilePaths[m_SelectedFile]);
                fileInfo.MoveTo(newName);
            }
            File.WriteAllText(newName, massageStr);
            ReadMessageFileLists();
            AssetDatabase.Refresh();
            OnGUI();
        }

        private void DeleteMessage()
        {
            File.Delete(m_MessageFilePaths[m_SelectedFile]);
            File.Delete(m_MessageFilePaths[m_SelectedFile] + ".meta");
            m_Message = null;
            m_MessageItems = null;
            m_SelectedFile = -1;
            ReadMessageFileLists();
            AssetDatabase.Refresh();
            OnGUI();
        }

        private void DeleteMessageItem(int index)
        {
            m_Message.messageItems.RemoveAt(index);
        }

        private void AddMessageItem()
        {
            m_Message.messageItems.Add(new MessageItem());
        }

        private void CreateMessageFile()
        {
            var index = 1;
            if (!File.Exists(ProtoPath + "\\NewMessage.proto"))
            {
                var stream = File.Create(ProtoPath + "\\NewMessage.proto");
                Debug.Log(ProtoPath + "\\NewMessage.proto");
                stream.Close();
            }
            else
            {
                while (File.Exists(ProtoPath + "\\NewMessage" + index + ".proto"))
                {
                    index++;
                }
                var stream = File.Create(ProtoPath + "\\NewMessage" + index + ".proto");
                Debug.Log(ProtoPath + "\\NewMessage" + index + ".proto");
                stream.Close();
            }
            ReadMessageFileLists();
            AssetDatabase.Refresh();
            OnGUI();
        }

        private void LoadAndDeserializeFile()
        {
            List<string> lines = new List<string>();
            m_Message = new ProtobufMessage {messageItems = new List<MessageItem>()};
            StreamReader file = new StreamReader(m_MessageFilePaths[m_SelectedFile]);
            var line = file.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = file.ReadLine();
            }
            file.Close();

            bool inItem = false;

            foreach (var line1 in lines)
            {
                if (string.IsNullOrWhiteSpace(line1))
                {
                    continue;
                }
                var words = line1.Trim().Split(' ');

                if (inItem)
                {
                    if (words[0] == "}")
                    {
                        continue;
                    }

                    var messageItem = new MessageItem {repeated = false};
                    bool hasType = false;
                    bool hasName = false;
                    foreach (var word in words)
                    {
                        switch (word)
                        {
                            case "repeated":
                                messageItem.repeated = true;
                                break;
                            case "int32":
                                messageItem.type = MessageType.Int32;
                                hasType = true;
                                break;
                            case "string":
                                messageItem.type = MessageType.String;
                                hasType = true;
                                break;
                            default:
                                if (!hasType)
                                {
                                    messageItem.type = MessageType.Null;
                                    messageItem.customType = word;
                                    hasType = true;
                                    break;
                                }
                                else if(!hasName)
                                {
                                    messageItem.name = word;
                                    hasName = true;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                        }
                    }
                    m_Message.messageItems.Add(messageItem);
                }
                else
                {
                    switch (words[0])
                    {
                        case "syntax":
                            for (var i = 0; i < m_SyntaxList.Length; i++)
                            {
                                var syntax = m_SyntaxList[i];
                                var tempWords = words[2].Split('"');
                                if (tempWords[1] == syntax)
                                {
                                    m_Message.syntax = (Syntax) i;
                                }
                            }
                            break;
                        case "package":
                            m_Message.package = words[1].Split(';')[0];
                            break;
                        case "message":
                            m_Message.name = words[1];
                            inItem = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            m_MessageItems = m_Message.messageItems;
        }

        private void OnInspectorUpdate()
        {
            ReadMessageFileLists();
            GetAllTypes();
        }

        private void GetAllTypes()
        {
            List<string> newTypeList = new List<string> {"int32", "string"};
            foreach (var fileName in m_MessageFileNames)
            {
                newTypeList.Add(fileName.Substring(0, 1).ToUpper() + fileName.Substring(1));
            }

            m_TypeList = newTypeList.ToArray();
        }

        private void ReadMessageFileLists()
        {
            List<string> newFileNameList = new List<string>();
            List<string> newFilePathList = new List<string>();
            DirectoryInfo protoFolder = new DirectoryInfo(ProtoPath);
            FileInfo[] protoFiles = protoFolder.GetFiles("*.proto");
            foreach (var protoFile in protoFiles)
            {
                newFileNameList.Add(protoFile.Name.Split('.')[0]);
                newFilePathList.Add(protoFile.FullName);
            }
            m_MessageFileNames = newFileNameList;
            m_MessageFilePaths = newFilePathList;
        }
    }
}
