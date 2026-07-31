using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace NuoYan.DialogSystem
{
    public class DialogueManager
    {
        private static DialogueManager _instance;
        public static DialogueManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DialogueManager();
                }
                _instance.OnInit();
                return _instance;
            }
        }
        private PanelDialogue m_PanelDialogue;
        private DialogueRunner m_DialogueRunner;
        private void OnInit()
        {
            if (m_PanelDialogue == null)
            {
                var v = Resources.Load<PanelDialogue>(nameof(PanelDialogue));
                m_PanelDialogue = GameObject.Instantiate(v);
                m_PanelDialogue.gameObject.SetActive(false);
                m_PanelDialogue.transform.SetParent(GameObject.Find("Canvas").transform, false);
            }
        }
        /// <summary>
        /// SetRunner方法用于设置当前对话的运行器，通常在开始对话时调用
        /// </summary>
        /// <param name="runner"></param> <summary>
        public void SetRunner(DialogueRunner runner)
        {
            if (m_PanelDialogue != null)
            {
                m_DialogueRunner = runner;
                m_PanelDialogue.gameObject.SetActive(true);
                m_PanelDialogue.Init(runner);
            }
        }
        /// <summary>
        /// CreateDialogueOptions方法用于在对话界面上创建选项按钮，通常在当前对话节点有多个选项时调用
        /// </summary>
        /// <param name="index"></param>
        /// <param name="choice"></param>
        /// <param name="onOptionSelected"></param> <summary>
        public void CreateDialogueOptions(int index, DialogueChoice choice, UnityAction onOptionSelected)
        {
            var prefab = Resources.Load<DialogueOption>(nameof(DialogueOption));
            if (prefab != null)
            {
                DialogueOption option = GameObject.Instantiate(prefab);
                option.transform.SetParent(m_PanelDialogue.OptionParent, false);
                option.SetOption(index, choice.labelText, () =>
                {
                    onOptionSelected?.Invoke();
                    m_PanelDialogue.RefUI();
                    ClearDialogueOptions();
                });
            }
        }

        public void ClearDialogueOptions()
        {
            if (m_PanelDialogue == null) return;
            var parent = m_PanelDialogue.OptionParent;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
