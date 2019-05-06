using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Global class that stores global values.
//////////////////////////////////////
namespace iMVR_Launcher
{
    public static class AzureRestClient
    {
        public static bool _needDownload = false;
        public static bool _CheckUpdate = true;
        public static string UrlEndpoint = "http://127.0.0.1:3000";
        public static string LoginEndpoint = "/users/login";
        public static string RegisterEndpoint = "/users/register";
        public static string ConfirmEmailEndpoint = "/users/resendConfirmationEmail";
        public static string ForgotPasswordEndpoint = "/users/forgotPassword";
        public static string version = "1.0";
        public static string CheckUpdateEndpoint = "/checkClientVersion?version=" + version;
        public static string _accessToken;
        public static string downloadURL;
        public static string AddPatchEndpoint = "/developers/addClientPatchData";
        public static string updateLatestClientVersion = "/developers/updateLatestClientVersion";
        public static string deleteClientPatchData = "/developers/deleteClientPatchData";
    }

    public class ErrorCode
    {
        public string Code;
        public string message;
        public string Resource;
        public string RequestId;
    }

    public class GetResult
    {
        public Dictionary<string, dynamic>[] Result;
    }
}
