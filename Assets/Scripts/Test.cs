using System.Collections;
using System.Collections.Generic;
using NuoYan.DialogSystem;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        DialogueManager.Instance.SetRunner(GetComponent<DialogueRunner>());
    }
    public void TestMethod()
    {
        Debug.Log("Test");
    }
}
