using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Editor
{
    public class ProtobufMessageEditor : EditorWindow
    {
        [SerializeField] private bool reset = true;
        [SerializeField] private bool enable = false;
        private const string EditorTitle = "Protobuf Message Editor";
        private const string EditorMenu = "Tools/Protobuf Message Editor";
        private const string ProtoPath = "Assets\\MessagePrototype";
        [SerializeField] private List<string> m_MessageFileNames;
        [SerializeField] private List<string> m_MessageFilePaths;
        [SerializeField] private int m_SelectedFile = -1;
        [SerializeField] private List<MessageItem> m_MessageItems;
        [SerializeField] private List<EnumItem> m_EnumItems;
        [SerializeField] private ProtobufMessage m_Message;
        [SerializeField] private ProtobufMessage m_OriginMessage;
        [SerializeField] private Vector2 m_V1 = new Vector2(0, 0);
        [SerializeField] private Vector2 m_V2 = new Vector2(0, 0);
        [SerializeField] private string[] m_TypeList;
        [SerializeField] private bool m_HasError;
        [SerializeField] private List<int> m_IdList = new List<int>();
        [SerializeField] private List<string> m_NameList = new List<string>();
        [SerializeField] private static Rect windoClickArea;

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
            var window = GetWindow(typeof(ProtobufMessageEditor));
            window.minSize = new Vector2(850, 500);
            windoClickArea = window.position;
        }


        private void OnEnable()
        {
            if (reset)
            {
                if (m_SelectedFile == -1)
                {
                    m_Message = null;
                }
                else
                {
                    LoadAndDeserializeFile();
                }
            }
        }

        private void OnGUI()
        {
            if (m_Styles == null)
            {
                m_Styles = new Styles();
            }

            Event e = Event.current;
            windoClickArea = GUI.Window(0, windoClickArea, null, "MyWindow");
            if (e.type == EventType.MouseDown && windoClickArea.Contains(e.mousePosition))
            {
                GUI.FocusControl(null);
            }

            m_HasError = false;
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

            //Left
            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.3f));
            m_V1 = GUILayout.BeginScrollView(m_V1, false, false, GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(Screen.height));
            {
                var old = m_SelectedFile;
                var temp_select = GUILayout.SelectionGrid(m_SelectedFile, m_MessageFileNames.ToArray(), 1, m_Styles.ListItem);
                if (old != temp_select)
                {
                    GUI.FocusControl(null);
                    if (CheckSave())
                    {
                        m_SelectedFile = temp_select;
                        LoadAndDeserializeFile();
                    }
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
                m_Message.Name = EditorGUILayout.TextField("message name", m_Message.Name);
                GUILayout.Space(5);
                GUILayout.Label(" Enum Items", m_Styles.TitleStyle);
                GUILayout.BeginVertical();
                var tempEnumName = new List<string>();
                for (var i = 0; i < m_EnumItems.Count; i++)
                {
                    GUILayout.Space(5);
                    var enumItem = m_EnumItems[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label(" Enum Item " + (i + 1), m_Styles.BoldStyle);
                    enumItem.Name = EditorGUILayout.TextField("name", enumItem.Name);
                    var enumNameError = CheckEnumNameError(ref tempEnumName, enumItem.Name);
                    if (!string.IsNullOrWhiteSpace(enumNameError))
                    {
                        m_HasError = true;
                        EditorGUILayout.HelpBox(enumNameError, UnityEditor.MessageType.Error);
                    }
                    var enumNameNumberError = CheckEnumNameNumberError(enumItem.Name);
                    if (!string.IsNullOrWhiteSpace(enumNameNumberError))
                    {
                        m_HasError = true;
                        EditorGUILayout.HelpBox(enumNameNumberError, UnityEditor.MessageType.Error);
                    }
                    var tempItems = new Dictionary<string, string>();
                    var tempKeys = new List<string>();
                    var tempValues = new List<string>();
                    foreach (var enumItemEnumItem in enumItem.EnumItems)
                    {
                        GUILayout.BeginHorizontal();
                        var tempKey = EditorGUILayout.TextField("key", enumItemEnumItem.Key);
                        var tempValue = EditorGUILayout.TextField("value", enumItemEnumItem.Value);
                        tempItems[tempKey] = tempValue;
                        if (GUILayout.Button("Delete"))
                        {
                            tempItems.Remove(tempKey);
                        }
                        GUILayout.EndHorizontal();
                        var keyNumberError = CheckEnumKeyNumberError(tempKey);
                        if (!string.IsNullOrWhiteSpace(keyNumberError))
                        {
                            m_HasError = true;
                            EditorGUILayout.HelpBox(keyNumberError, UnityEditor.MessageType.Error);
                        }
                        var valueNumberError = CheckEnumValueNumberError(tempValue);
                        if (!string.IsNullOrWhiteSpace(valueNumberError))
                        {
                            m_HasError = true;
                            EditorGUILayout.HelpBox(valueNumberError, UnityEditor.MessageType.Error);
                        }
                        var enumRepeatedError = CheckEnumRepeatedError(ref tempKeys, ref tempValues, tempKey, tempValue);
                        if (!string.IsNullOrWhiteSpace(enumRepeatedError))
                        {
                            m_HasError = true;
                            EditorGUILayout.HelpBox(enumRepeatedError, UnityEditor.MessageType.Error);
                        }
                    }
                    enumItem.EnumItems = tempItems;
                    var enumZeroError = CheckEnumZeroError(enumItem);
                    if (!string.IsNullOrWhiteSpace(enumZeroError))
                    {
                        m_HasError = true;
                        EditorGUILayout.HelpBox(enumZeroError, UnityEditor.MessageType.Error);
                    }
                    if (GUILayout.Button("Add enum key"))
                    {
                        AddEnumKey(enumItem);
                    }
                    if (GUILayout.Button("Delete enum item"))
                    {
                        m_EnumItems.Remove(enumItem);
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
                if (GUILayout.Button("Add a enum item"))
                {
                    m_EnumItems.Add(new EnumItem{EnumItems = new Dictionary<string, string>()});
                    GetAllTypes();
                }
                GUILayout.Space(5);
                GUILayout.Label(" Message Items", m_Styles.TitleStyle);
                GUILayout.BeginVertical();
                m_IdList.Clear();
                m_NameList.Clear();
                for (var i = 0; i < m_MessageItems.Count; i++)
                {
                    GUILayout.Space(5);
                    var messageItem = m_MessageItems[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    GUILayout.Label(" Message Item " + (i+1), m_Styles.BoldStyle);
                    messageItem.Id = EditorGUILayout.TextField("id", messageItem.Id);
                    var idError = CheckIdError(messageItem.Id);
                    if (!string.IsNullOrWhiteSpace(idError))
                    {
                        m_HasError = true;
                        EditorGUILayout.HelpBox(idError, UnityEditor.MessageType.Error);
                    }

                    messageItem.Name = EditorGUILayout.TextField("name", messageItem.Name);
                    var nameError = CheckNameError(messageItem.Name);
                    if (!string.IsNullOrWhiteSpace(nameError))
                    {
                        m_HasError = true;
                        EditorGUILayout.HelpBox(nameError, UnityEditor.MessageType.Error);
                    }

                    var temp = 0;

                    for (var index = 0; index < m_TypeList.Length; index++)
                    {
                        var s = m_TypeList[index];
                        if (s == messageItem.Type)
                        {
                            temp = index;
                        }
                    }
                    
                    temp = EditorGUILayout.Popup("type", temp, m_TypeList);
                    messageItem.Type = m_TypeList[temp];
                    messageItem.Repeated = EditorGUILayout.Toggle("repeated", messageItem.Repeated);
                    messageItem.Optional = EditorGUILayout.Toggle("optional", messageItem.Optional);
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(GUILayout.MaxWidth(100));
                    GUILayout.Space(14);
                    if (GUILayout.Button("Delete", GUILayout.Height(90)))
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
                if (m_Message.Equals(m_OriginMessage))
                {
                    if (GUILayout.Button("Save message"))
                    {
                        SaveMessage();
                    }
                }
                else
                {
                    if (GUILayout.Button("* Save message *"))
                    {
                        SaveMessage();
                    }
                }

                GUILayout.Space(5);
                if (GUILayout.Button("Delete message"))
                {
                    // Delete message
                    DeleteMessage();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        private string CheckEnumNameError(ref List<string> tempNameList, string enumItemName)
        {
            if (tempNameList.Contains(enumItemName))
            {
                return "Enum name " + enumItemName + " is repeated!";
            }
            tempNameList.Add(enumItemName);
            return "";
        }

        private string CheckEnumNameNumberError(string name)
        {
            try
            {
                int tempId = int.Parse(name);
                return "Enum name " + name + " can't be number!";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string CheckEnumKeyNumberError(string key)
        {
            try
            {
                int tempId = int.Parse(key);
                return "Key " + key + " can't be number!";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string CheckEnumValueNumberError(string value)
        {
            // is number
            try
            {
                int tempId = int.Parse(value);
                return "";
            }
            catch (Exception)
            {
                return "Value " + value + " must be number!";
            }
        }

        private void SaveMessage()
        {
            if (m_HasError)
            {
                return;
            }
            string massageStr = m_Message.ToString();
            string newName = ProtoPath + "\\" + m_Message.Name + ".proto";
            if (!string.IsNullOrEmpty(newName) && newName != m_MessageFilePaths[m_SelectedFile])
            {
                FileInfo fileInfo = new FileInfo(m_MessageFilePaths[m_SelectedFile]);
                fileInfo.MoveTo(newName);
            }
            File.WriteAllText(newName, massageStr);
            ReadMessageFileLists();
            LoadAndDeserializeFile();
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
            m_Message.MessageItems.RemoveAt(index);
        }

        private void AddMessageItem()
        {
            m_Message.MessageItems.Add(new MessageItem());
        }

        private void AddEnumKey(EnumItem enumItem)
        {
            if (!enumItem.EnumItems.ContainsKey("new_key"))
            {
                enumItem.EnumItems.Add("new_key", "");
            }
            else
            {
                var id = 1;
                while (enumItem.EnumItems.ContainsKey("new_key_" + id))
                {
                    id++;
                }
                enumItem.EnumItems.Add("new_key_" + id, "");
            }
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
            m_Message = new ProtobufMessage {MessageItems = new List<MessageItem>(), EnumItems = new List<EnumItem>()};
            StreamReader file = new StreamReader(m_MessageFilePaths[m_SelectedFile]);
            var line = file.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = file.ReadLine();
            }
            file.Close();

            bool inItem = false;
            bool inEnum = false;
            EnumItem enumItem = null;

            foreach (var line1 in lines)
            {
                if (string.IsNullOrWhiteSpace(line1))
                {
                    continue;
                }
                var words = line1.Trim().Split(' ');

                if (inItem)
                {


                    if (words[0] == "enum")
                    {
                        inEnum = true;
                        enumItem = new EnumItem{EnumItems = new Dictionary<string, string>()};
                    }

                    if (inEnum)
                    {
                        if (words[0] == "enum")
                        {
                            enumItem.Name = words[1];
                        }
                        else if (words[0] == "}")
                        {
                            inEnum = false;
                            m_Message.EnumItems.Add(enumItem);
                        }
                        else
                        {
                            enumItem.EnumItems[words[0]] = words[2].Substring(0, words[2].Length - 1);
                        }
                        continue;
                    }
                    if (words[0] == "}")
                    {
                        continue;
                    }
                    var messageItem = new MessageItem {Repeated = false, Optional = false};
                    bool hasType = false;
                    bool hasName = false;
                    foreach (var word in words)
                    {
                        switch (word)
                        {
                            case "repeated":
                                messageItem.Repeated = true;
                                break;
                            case "optional":
                                messageItem.Optional = true;
                                break;
                            default:
                                if (!hasType)
                                {
                                    messageItem.Type = word;
                                    hasType = true;
                                    break;
                                }
                                else if(!hasName)
                                {
                                    messageItem.Name = word;
                                    hasName = true;
                                    break;
                                }
                                else if (word.EndsWith(";"))
                                {
                                    var numStr = word.Substring(0, word.Length - 1);
                                    messageItem.Id = numStr;
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                        }
                    }
                    m_Message.MessageItems.Add(messageItem);
                }
                else
                {
                    switch (words[0])
                    {
                        case "message":
                            m_Message.Name = words[1];
                            inItem = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            m_MessageItems = m_Message.MessageItems;
            m_EnumItems = m_Message.EnumItems;
            GetAllTypes();
            m_OriginMessage = ProtobufMessage.Copy(m_Message);
        }

        private void OnInspectorUpdate()
        {
            ReadMessageFileLists();
            
        }

        private void GetAllTypes()
        {
            List<string> newTypeList = new List<string>
            {
                "double", "float", "int32", 
                "int64", "uint32", "uint64", 
                "sint32", "sint64", "fixed32", 
                "fixed64", "sfixed32", "sfixed64", 
                "bool", "string", "bytes"
            };
            newTypeList.Add("");
            foreach (var fileName in m_MessageFileNames)
            {
                newTypeList.Add(fileName.Substring(0, 1).ToUpper() + fileName.Substring(1));
            }
            newTypeList.Add("");
            foreach (var enumItem in m_EnumItems)
            {
                newTypeList.Add(enumItem.Name);
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

        private string CheckIdError(string id)
        {
            // is number
            int tempId;
            try
            {
                tempId = int.Parse(id);
            }
            catch (Exception)
            {
                return "ID must to be number!";
            }

            // repeated
            if (m_IdList.Contains(tempId))
            {
                return "ID is repeated!";
            }

            // id between 19000 to 19999
            if (tempId >= 19000 && tempId <= 19999)
            {
                return "ID cannot between 19,000 to 19,999!";
            }

            if (tempId < 1)
            {
                return "ID must >= 1";
            }

            if (tempId > 536870911)
            {
                return "ID must <= 536,870,911";
            }

            m_IdList.Add(tempId);
            return "";
        }

        private string CheckNameError(string name)
        {
            // repeated
            if (m_NameList.Contains(name))
            {
                return "Name " + name + " is repeated!";
            }

            m_NameList.Add(name);
            return "";
        }

        private string CheckEnumZeroError(EnumItem enumItem)
        {
            if (!enumItem.EnumItems.ContainsValue("0"))
            {
                return "Enum must have 0 value!";
            }

            return "";
        }

        private string CheckEnumRepeatedError(ref List<string> tempKey, ref List<string> tempValue, string key, string value)
        {
            if (tempKey.Contains(key))
            {
                return "Key " + key + " is repeated!";
            }

            tempKey.Add(key);

            if (tempValue.Count == 0 && value != "0")
            {
                tempValue.Add(value);
                return "First key " + key + "'s value must is 0!";
            }

            if (tempValue.Contains(value))
            {
                return "Value " + value + " is repeated!";
            }

            tempValue.Add(value);

            return "";
        }

        private bool CheckSave()
        {
            if (m_Message == null || m_OriginMessage == null)
            {
                return true;
            }
            if (m_Message.Equals(m_OriginMessage))
            {
                return true;
            }

            return EditorUtility.DisplayDialog("Not saved", "This Message is not saved, continue?", "Yes", "No");
        }

        private void OnFocus()
        {
            enable = true;
        }

        private void OnLostFocus()
        {
            enable = false;
        }

        private void OnDestroy()
        {
            if (!CheckSave())
            {
                reset = false;
                var newWin = Instantiate(this);
                newWin.m_Message = m_Message;
                newWin.m_OriginMessage = m_OriginMessage;
                newWin.m_EnumItems = m_EnumItems;
                newWin.reset = true;
                newWin.Show();
            }
        }
    }
}
