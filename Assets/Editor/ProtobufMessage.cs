using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Editor
{

    [Serializable]
    public class MessageItem
    {
        public string Id;
        public string Name;
        public string Type;
        public bool Repeated;

        public bool Equals(MessageItem obj)
        {
            if (Name != obj.Name)
            {
                return false;
            }

            if (Type != obj.Type)
            {
                return false;
            }

            if (Repeated != obj.Repeated)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            string str = "";

            str += "    ";

            if (Repeated)
            {
                str += "repeated ";
            }

            str += Type + " ";

            str += Name + " ";

            str += "= " + Id + ";\n";

            return str;
        }
    }

    [Serializable]
    public class EnumItem
    {
        public string Name;
        public Dictionary<string, string> EnumItems;

        public bool Equals(EnumItem obj)
        {
            if (Name != obj.Name)
            {
                return false;
            }

            if (EnumItems != obj.EnumItems)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var str = "    ";

            str += "enum " + Name + " {\n";

            foreach (var item in EnumItems)
            {
                str += "        " + item.Key + " = " + item.Value + ";\n";
            }

            str += "    }\n";

            return str;
        }
    }

    [Serializable]
    public class ProtobufMessage
    {
        public string Name;
        public List<MessageItem> MessageItems;
        public List<EnumItem> EnumItems;

        public bool Equals(ProtobufMessage obj)
        {
            if (Name != obj.Name)
            {
                return false;
            }

            if (MessageItems.Count != obj.MessageItems.Count)
            {
                return false;
            }

            if (MessageItems.Where((t, i) => !t.Equals(obj.MessageItems[i])).Any())
            {
                return false;
            }

            if (EnumItems.Where((t, i) => !t.Equals(obj.EnumItems[i])).Any())
            {
                return false;
            }

            return true;

        }

        public override string ToString()
        {
            string str = "";

            str += "syntax = \"proto3\";\n";

            str += "package protobuf;\n\n";

            str += "message " + Name.Substring(0, 1).ToUpper() + Name.Substring(1) + " {\n";

            foreach (var enumItem in EnumItems)
            {
                str += enumItem.ToString();
            }

            foreach (var messageItem in MessageItems)
            {
                str += messageItem.ToString();
            }

            str += "}";

            return str;
        }
    }
}