using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace com.antlersoft.BBQClient
{
    public interface IBrowseByQuery
    {
        QueryResponse PerformQuery( QueryRequest request);
    }

    public class QueryRequest
    {
        public QueryRequest()
        { ObjectKeys = new List<String>(); }

        public QueryRequest(String text)
        {
            queryText = text;
            ObjectKeys = new List<String>();
        }

        [XmlText]
        public String QueryText
        {
            get { return queryText; }
            set { queryText=value; }
        }

        [XmlAttribute]
        public string DatabaseName
        {
            get; set;
        }

        [XmlArray]
        public List<String> ObjectKeys { get; set; }

        private String queryText=String.Empty;

    }

    public class QueryResponse
    {
        private int responseCount = 0;

		[XmlAttribute]
        public int ResponseCount
        {
            get { return responseCount; }
            set { responseCount = value; }
        }
        private ResponseObject[]? responses = null;

        [XmlArray]
        public ResponseObject[]? Responses
        {
            get { return responses; }
            set { responses = value; }
        }

        private RequestException? exception = null;

        public RequestException? RequestException
        {
            get { return exception; }
            set { exception = value; }
        }
    }

    public class RequestException
    {
        [XmlElement]
        public String Message
        {
            get { return message; }
            set { message = value; }
        }

        [XmlElement]
        public String StackTrace
        {
            get { return stackTrace; }
            set { stackTrace = value; }
        }

        private String message = String.Empty;
        private String stackTrace = String.Empty;
    }

    public class ResponseObject
    {
        private String objectType = String.Empty;

        [XmlAttribute]
        public String ObjectType
        {
            get { return objectType; }
            set { objectType = value; }
        }

        private String objectKey;

        [XmlAttribute]
        public String ObjectKey
        {
            get { return objectKey; }
            set { objectKey = value; }
        }

        private String description = String.Empty;

		[XmlAttribute]
        public String Description
        {
            get { return description; }
            set { description = value; }
        }
        private String fileName = String.Empty;

		[XmlAttribute]
        public String FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        private int lineNumber = 0;

		[XmlAttribute]
        public int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
