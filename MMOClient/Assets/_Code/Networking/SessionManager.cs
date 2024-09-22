using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public class SessionManager : Singleton<SessionManager>
    {
        public CharacterData characterData = new();
    }
}
