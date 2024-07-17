using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnrealByte.EasyJira {

    [System.Serializable]
    public class JUser {
        public string accountId = "";
        public string emailAddress = "";
        public string displayName = "";
        public string avatarURL = "";

        public JUser() {}
    }
}
