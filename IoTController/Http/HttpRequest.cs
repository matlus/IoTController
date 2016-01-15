using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTController.Http
{
    class HttpRequest
    {
        public string Method { get; private set; }
        public string PathInfo { get; private set; }
        public NameValueCollection QueryStrings { get; private set; }

        public HttpRequest(string httpRequestData)
        {
            QueryStrings = new NameValueCollection();
            string httpHeaderLine1 = httpRequestData.Substring(0, httpRequestData.IndexOf('\r'));
            var httpHeaderLine1Parts = httpHeaderLine1.Split(' ');

            Method = httpHeaderLine1Parts[0];
            var pathAndQueryString = httpHeaderLine1Parts[1];

            var questionIndex = pathAndQueryString.IndexOf("?");

            if (questionIndex != -1)
            {
                PathInfo = pathAndQueryString.Substring(0, questionIndex);
                var queryParts = pathAndQueryString.Substring(questionIndex + 1);
                var namesValues = queryParts.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var nameAndValue in namesValues)
                {
                    var nameValue = nameAndValue.Split('=');
                    QueryStrings.Add(nameValue[0], nameValue[1]);
                }
            }
            else
            {
                PathInfo = "/";
            }
        }
    }
}
