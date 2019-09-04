using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Editor
{
    public enum MessageType
    {
        Null = -1,
        Int32,
        String,
    }

    public enum Syntax
    {
        Proto2,
        Proto3
    }

    [Serializable]
    public class MessageItem
    {
        public string name;
        public MessageType type;
        public string customType;
        public bool repeated;

        public bool Equals(MessageItem obj)
        {
            if (name != obj.name)
            {
                return false;
            }

            if (type != obj.type)
            {
                return false;
            }

            if (type != MessageType.Null && customType != obj.customType)
            {
                return false;
            }

            if (repeated != obj.repeated)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            string str = "";

            str += "    ";

            if (repeated)
            {
                str += "repeated ";
            }

            switch (type)
            {
                case MessageType.Null:
                    str += customType + " ";
                    break;
                case MessageType.Int32:
                    str += "int32 ";
                    break;
                case MessageType.String:
                    str += "string ";
                    break;
                default:
                    break;
            }

            str += name + " ";

            return str;
        }
    }

    [Serializable]
    public class ProtobufMessage
    {
        public Syntax syntax;
        public string package;
        public string name;
        public List<MessageItem> messageItems;

        public bool Equals(ProtobufMessage obj)
        {
            if (syntax != obj.syntax)
            {
                return false;
            }

            if (package != obj.package)
            {
                return false;
            }

            if (name != obj.name)
            {
                return false;
            }

            if (messageItems.Count != obj.messageItems.Count)
            {
                return false;
            }

            if (messageItems.Where((t, i) => !t.Equals(obj.messageItems[i])).Any())
            {
                return false;
            }

            return true;

        }

        public override string ToString()
        {
            string str = "";

            str += "syntax = \"";
            switch (syntax)
            {
                case Syntax.Proto2:
                    str += "proto2";
                    break;
                case Syntax.Proto3:
                    str += "proto3";
                    break;
                default:
                    break;
            }
            str += "\";\n";

            str += "package " + package + ";\n\n";

            str += "message " + name + " {\n";

            for (var i = 1; i <= messageItems.Count; i++)
            {
                var messageItem = messageItems[i-1];
                str += messageItem.ToString();
                str += "= " + i + ";\n";
            }

            str += "}";

            return str;
        }
    }
}