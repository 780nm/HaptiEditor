using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputHandler : SingletonMonoBehavior<InputHandler>
{
    public List<KeyCode> KeysToWatch;
    public UnityAction<KeyCode> OnKeyDownEvent;
    public UnityAction<KeyCode> OnKeyUpEvent;

    void Update()
    {
        foreach (KeyCode key in KeysToWatch)
        {
            if (Input.GetKeyDown(key))
            {
                OnKeyDownEvent.Invoke(key);
            }
            if (Input.GetKeyUp(key))
            {
                OnKeyUpEvent.Invoke(key);
            }
        }
    }
}