﻿using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common
{
    public class Message
    {
        public Message(string rawMessage)
        {
            Key = rawMessage.Substring(0, rawMessage.IndexOf(':'));
            var message = rawMessage.Split(':')[1];
            string command = message.Substring(1, message.IndexOf(">") - 1);
            string data = message.Substring(message.IndexOf(">") + 1);
            Command = command;
            Data = data;
        }

        public string Key { get; set; }
        public string Command { get; set; }
        public string Data { get; set; }
        public int DataCount { get { return Data.Split('#').Length; } }

        public string GetData(int index)
        {
            string result = "";
            if (index < DataCount)
            {
                result = Data.Split('#')[index];
            }
            return result;
        }
    }

    public static class MessageProcessor
    {
        public static void ProcessMessage(string message)
        {
            new Message(message).PublishEvent(EventTopicNames.MessageReceivedEvent);
        }
    }
}
