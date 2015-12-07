using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GitHub_Services_WEBAPI.App_Code
{
    public class logginglibrary
    {
        LoggingModel _log;

        public enum MessageType { Info, Warning, Error };
        public enum IsBusinessEvent { Yes, No };
        public string LoggingEndpoint = "http://redisfacadeservice.dev.otpp.com/api/Logging";

        public logginglibrary(string AppGroup, string AppName, String AppVersion, string _loggingendpoint)
        {
             _log = new LoggingModel(AppGroup, AppName, AppVersion);
             if (LoggingEndpoint != null)
             {
                 LoggingEndpoint = _loggingendpoint;
             }
        }

        public void LogToElk(MessageType _MessageType, IsBusinessEvent _isBusinessEvent, string _Message, string _MessageDetails)
        {
            _log.messageType = _MessageType.ToString();
            _log.isBusinessEvent = _isBusinessEvent.ToString();
            _log.Message = _Message;
            _log.MessageDetails = _MessageDetails;

            try
            {

                var request = (HttpWebRequest)WebRequest.Create(LoggingEndpoint);

                var postData = JsonConvert.SerializeObject(_log); ;
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch(Exception error){

                string cs = _log.AppName;
                EventLog elog = new EventLog();
                if (!EventLog.SourceExists(cs))
                {
                    EventLog.CreateEventSource(cs, "Application");
                }
                elog.Source = cs;
                elog.EnableRaisingEvents = true;
                elog.WriteEntry(error.Message, EventLogEntryType.Error);
                elog.WriteEntry("Origional Error Message: " + _log.Message, EventLogEntryType.Error);
                elog.WriteEntry("Origional Error Message: " + _log.MessageDetails, EventLogEntryType.Error);
            }
        }
    }

    [Serializable]
    public class LoggingModel
    {

        public string AppGroup;
        public string AppName;
        public string AppVersion;
        public string messageType;
        public string isBusinessEvent; // true, False
        public string Message;
        public string MessageDetails;
        public Dictionary<string, string> AdditionalProperties = new Dictionary<string, string>();

        public LoggingModel(string _AppGroup, string _AppName, string _AppVersion)
        {
            AppGroup = _AppGroup;
            AppName = _AppName;
            AppVersion = _AppVersion;
        }

    }
}