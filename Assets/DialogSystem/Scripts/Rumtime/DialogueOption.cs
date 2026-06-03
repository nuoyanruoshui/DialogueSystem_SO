using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueOption : MonoBehaviour
{
    private Button m_Btn_Option;
    private TextMeshProUGUI m_Txt_Option;
    private int m_OptionIndex;
    private UnityAction m_OnOptionSelected;
    void Awake()
    {
        m_Btn_Option = GetComponent<Button>();
        m_Txt_Option = transform.Find("Txt_Option").GetComponent<TextMeshProUGUI>();
        m_Btn_Option.onClick.AddListener(OnBtnOptionClick);
    }

    private void OnBtnOptionClick()
    {
        m_OnOptionSelected?.Invoke();
        Debug.Log($"Option {m_OptionIndex} clicked");
    }
    public void SetOption(int index, string optionText, UnityAction onOptionSelected)
    {
        m_OptionIndex = index;
        m_Txt_Option.text = optionText;
        m_OnOptionSelected = onOptionSelected;
    }
}
