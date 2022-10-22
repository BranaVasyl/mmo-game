using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BV
{
    public interface IInteractable
    {
        void Interact();
        string GetDescription();
    }
}
