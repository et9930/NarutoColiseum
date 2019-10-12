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
        public bool Optional;

        public static MessageItem Copy(MessageItem origin)
        {
            MessageItem newMessageItem = new MessageItem
            {
                Id = origin.Id,
                Name = origin.Name,
                Type = origin.Type,
                Repeated = origin.Repeated,
                Optional = origin.Optional
            };
            return newMessageItem;
        }

        public override bool Equals(object other)
        {
            var obj = (MessageItem) other;
            if (other == null)
            {
                return false;
            }

            if (Id != obj.Id)
            {
                return false;
            }

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

            if (Optional != obj.Optional)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Repeated.GetHashCode();
                hashCode = (hashCode * 397) ^ Optional.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            string str = "";

            str += "    ";

            if (Repeated)
            {
                str += "repeated ";
            }

            if (Optional)
            {
                str += "optional ";
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

        public static EnumItem Copy(EnumItem origin)
        {
            EnumItem newEnumItem = new EnumItem
            {
                Name = origin.Name, 
                EnumItems = new Dictionary<string, string>()
            };

            foreach (var originEnumItem in origin.EnumItems)
            {
                newEnumItem.EnumItems.Add(originEnumItem.Key, originEnumItem.Value);
            }

            return newEnumItem;
        }

        public override bool Equals(object other)
        {
            var obj = (EnumItem) other;
            if (obj == null)
            {
                return false;
            }

            if (Name != obj.Name)
            {
                return false;
            }

            if (EnumItems.Count != obj.EnumItems.Count)
            {
                return false;
            }

            foreach (var enumItem in EnumItems)
            {
                if (!obj.EnumItems.ContainsKey(enumItem.Key) || obj.EnumItems[enumItem.Key] != enumItem.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (EnumItems != null ? EnumItems.GetHashCode() : 0);
            }
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

        public static ProtobufMessage Copy(ProtobufMessage origin)
        {
            ProtobufMessage newProtobufMessage = new ProtobufMessage
            {
                Name = origin.Name, 
                EnumItems = new List<EnumItem>(),
                MessageItems = new List<MessageItem>()
            };
            origin.EnumItems.ForEach(i => newProtobufMessage.EnumItems.Add(EnumItem.Copy(i)));
            origin.MessageItems.ForEach(i => newProtobufMessage.MessageItems.Add(MessageItem.Copy(i)));
            return newProtobufMessage;
        }

        public override bool Equals(object other)
        {
            var obj = (ProtobufMessage) other;

            if (obj == null)
            {
                return false;
            }

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

            if (EnumItems.Count != obj.EnumItems.Count)
            {
                return false;
            }

            if (EnumItems.Where((t, i) => !t.Equals(obj.EnumItems[i])).Any())
            {
                return false;
            }

            return true;

        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MessageItems != null ? MessageItems.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (EnumItems != null ? EnumItems.GetHashCode() : 0);
                return hashCode;
            }
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