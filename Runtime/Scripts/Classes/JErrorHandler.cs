using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace UnrealByte.EasyJira {
    public class JErrorHandler {
        
        public static string ReadError (string errorCode, string text, string errorText) {
            string ret = "";
#pragma warning disable 0168
            try {
                JsonData data = JsonMapper.ToObject(text);
                if (data["errorMessages"].Count>0) {
                    ret = data["errorMessages"][0].ToString();
                }
            } catch (Exception e) {
                if (errorCode.Equals("401")) {
                    ret = "Please check the Jira user and password.";
                } else if (errorCode.Equals("404")) {
                    ret = "Host not found, check de base URL.";
                } else {
                    ret = errorText;
                }
            }
#pragma warning restore 0168
            return ret;
        }
    }
}
