using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SWE_A1Grandner_MTCG.Enum;

namespace SWE_A1Grandner_MTCG.BusinessLogic
{
    internal class HttpResponse
    {
        private string? _content;
        public HttpStatusCode Status { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public string? Content
        {
            get => _content;
            set
            {
                _content = value;
                Headers["Content-Length"] = value!.Length.ToString();
                Headers.Add("Content-Type", "text/plain");
            }
            
        }


        public HttpResponse()
        {
            Headers = new Dictionary<string, string>
            {
                { "Content-Length", "0" },
                { "Connection", "Closed" }
            };
        }
        public HttpResponse(HttpStatusCode status)
        {
            Status = status;
            Headers = new Dictionary<string, string>
            {
                { "Content-Length", "0" },
                { "Connection", "Closed" }
            };
        }
        public HttpResponse(HttpStatusCode status, string content)
        {
            Status = status;
            Headers = new Dictionary<string, string>
            {
                { "Connection", "Closed" }
            };
            Content = content;
        }

        public override string ToString()
        {
            var message = $"HTTP/1.1 {(int)Status} {Status}" + Environment.NewLine;
            
            foreach (var pair in Headers)
            {
                message += $"{pair.Key}: {pair.Value}" + Environment.NewLine;
            }
            message += Environment.NewLine;

            if (Content != null)
            {
                message += Content + Environment.NewLine;
            }

            Console.WriteLine(message);
            return message;
        }
    }

  
}
