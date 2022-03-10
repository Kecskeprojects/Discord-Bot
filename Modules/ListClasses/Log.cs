using System;

namespace Discord_Bot.Modules.ListClasses
{
    public class Log
    {
        public string Content { get; set; }

        public Log(string type, string message, string error = "")
        {
            type = type.ToUpper();
            string hour = DateTime.Now.Hour < 10 ? "0" + DateTime.Now.Hour.ToString() : DateTime.Now.Hour.ToString();
            string minute = DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString();
            string second = DateTime.Now.Second < 10 ? "0" + DateTime.Now.Second.ToString() : DateTime.Now.Second.ToString();
            Content = "[" + hour + ":" + minute + ":" + second + "]" + "[" + type + "]" + ":\t";
            if (error == "")
            {
                Content += message;
            }
            else
            {
                Content += "Something went wrong! Error throw location: " + message + "\n" + error;
            }
        }
    }
}
