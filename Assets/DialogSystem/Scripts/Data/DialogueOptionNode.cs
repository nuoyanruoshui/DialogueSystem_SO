using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NuoYan.DialogSystem
{
    [CreateAssetMenu(fileName = "New Dialog Option Node", menuName = "NuoYan/Dialog System/Option Node")]
    public class DialogueOptionNode : DialogueNodeBase
    {
        [SerializeField] private List<DialogueChoice> choiceList = new();

        public List<DialogueChoice> ChoiceList => choiceList;

        public override void VaildNode()
        {
            if (choiceList is null || choiceList.Count == 0)
            {
                Debug.LogError("ChoiceList is null or empty");
            }
        }

        public void AddChoice(DialogueChoice choice)
        {
            if (choiceList is null)
                choiceList = new List<DialogueChoice>();
            choiceList.Add(choice);
        }

        public void RemoveChoice(DialogueChoice choice)
        {
            if (choiceList is not null)
                choiceList.Remove(choice);
        }

        public DialogueChoice GetChoice(int index)
        {
            if (choiceList is not null && index >= 0 && index < choiceList.Count)
                return choiceList[index];
            return null;
        }

        public void ClearChoices()
        {
            if (choiceList is not null)
                choiceList.Clear();
        }
    }
}
