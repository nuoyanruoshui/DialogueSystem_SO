using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace NuoYan.DialogSystem
{
    public class PanelDialogue : MonoBehaviour
    {
        private Button m_Btn_Next;
        private TextMeshProUGUI m_Txt_Content;
        private TextMeshProUGUI m_Txt_Name;
        private Transform m_OptionParent;
        public Transform OptionParent => m_OptionParent;
        private DialogueRunner m_DialogueRunner;
        void Awake()
        {
            m_Btn_Next = GetComponent<Button>();
            m_Txt_Content = transform.Find("TXT/Tmp_Content").GetComponent<TextMeshProUGUI>();
            m_Txt_Name = transform.Find("TXT/Tmp_Name").GetComponent<TextMeshProUGUI>();
            m_OptionParent = transform.Find("Options");
            m_Btn_Next.onClick.AddListener(OnBtnNextClick);
        }

        public void Init(DialogueRunner dialogueRunner)
        {
            m_DialogueRunner = dialogueRunner;
            m_DialogueRunner.StartDialogue();
            m_DialogueRunner.OnNodeChanged += (node) => { RefUI(); };
            m_DialogueRunner.OnDialogueEnd += () => { gameObject.SetActive(false); };
            RefUI();
        }

        private void OnBtnNextClick()
        {
            m_DialogueRunner.DialogueStep();
        }

        public void RefUI()
        {
            m_Txt_Content.text = m_DialogueRunner.CurrentNode?.DialogText;
            m_Txt_Name.text = m_DialogueRunner.CurrentNode?.SpeakerName;
        }
    }
}
