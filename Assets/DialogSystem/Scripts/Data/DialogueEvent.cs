using UnityEngine;
using UnityEngine.Events;

namespace NuoYan.DialogSystem
{
    [CreateAssetMenu(fileName = "New Dialogue Event", menuName = "NuoYan/Dialog System/Dialogue Event")]
    public class DialogueEvent : SOEventBase
    {
        public UnityEvent onNodeEnter;

        public override void RaiseEvent(object? value)
        {
            onNodeEnter?.Invoke();
            base.RaiseEvent(value);
        }
    }
}
