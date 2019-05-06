using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: The difference between this and AzureRestUpdate is AzureRestRequestAPI does not need to
//              create delegate functions at runtime, all needed info are given by a serialized string in parameter or a method reference.
//////////////////////////////////////

namespace iMVR_Launcher
{
    public class AzureRestRequestAPI : AzureRestAPI
    {
        /// <summary>
        /// Send Request with given class to serialized data into json format.
        /// </summary>
        /// <param name="EndPoint">URL To the web server</param>
        /// <param name="httpMethod">HTTP Method of web request</param>
        /// <param name="serializedClass">class of data for json serialization</param>
        /// <returns></returns>
        public ResponseMessage Request(string EndPoint, HttpVerb httpMethod, string serializedClass)
        {
            WebRequest request = RequestBase(EndPoint, httpMethod.ToString());
            this.httpMethod = httpMethod;
            string response = SendRequest(request, serializedClass).Result;

            return JsonConvert.DeserializeObject<ResponseMessage>(response);
        }

        /// <summary>
        /// Send Request with given class to serialized data into json format.
        /// </summary>
        /// <param name="EndPoint">URL To the web server</param>
        /// <param name="httpMethod">HTTP Method of web request</param>
        /// <param name="serializedClass">class of data for json serialization</param>
        /// <param name="accessToken">True if need accessToken</param>
        /// <returns></returns>
        public ResponseMessage Request(string EndPoint, HttpVerb httpMethod, string serializedClass, string accessToken)
        {
            WebRequest request = RequestBase(EndPoint, httpMethod.ToString());
            this.httpMethod = httpMethod;
            request = AddAccessHeader(request, accessToken);
            string response = SendRequest(request, serializedClass).Result;

            return JsonConvert.DeserializeObject<ResponseMessage>(response);
        }

        protected async Task<string> SendRequest(WebRequest request, string serializedClass)
        {
            if (httpMethod != HttpVerb.GET)
            {
                request.ContentType = "application/json";
                using (StreamWriter jsonPayload = new StreamWriter(request.GetRequestStream()))
                {
                    jsonPayload.Write(serializedClass);
                }
            }
            return await WaitForResponse(request);
        }

        protected async Task<string> SendRequest(WebRequest request, Func<string> serializedClass)
        {
            if (httpMethod != HttpVerb.GET)
            {
                request.ContentType = "application/json";
                using (StreamWriter jsonPayload = new StreamWriter(request.GetRequestStream()))
                {
                    jsonPayload.Write(serializedClass?.Invoke());
                }
            }
            return await WaitForResponse(request);
        }

        protected async Task<string> SendRequest(WebRequest request, Func<HttpVerb, string> serializedClass)
        {
            if (httpMethod != HttpVerb.GET)
            {
                request.ContentType = "application/json";
                using (StreamWriter jsonPayload = new StreamWriter(request.GetRequestStream()))
                {
                    jsonPayload.Write(serializedClass?.Invoke(httpMethod));
                }
            }
            return await WaitForResponse(request);
        }
    }
}
