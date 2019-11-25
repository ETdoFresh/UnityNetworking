using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class InputFieldSubmit : MonoBehaviour
{
    public TMP_InputField inputField;

    public UnityEventString onSubmit = new UnityEventString();

    private void Update()
    {
        if (inputField && inputField.isActiveAndEnabled)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Submit();
    }

    public void Submit()
    {
        if (!inputField) return;

        onSubmit.Invoke(inputField.text);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    [Serializable] public class UnityEventString : UnityEvent<string> { }
}
