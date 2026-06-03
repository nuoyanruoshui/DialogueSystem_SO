#nullable enable
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace NuoYan.DialogSystem
{
    public class SOEventBase : ScriptableObject
    {
        public UnityAction<object?> OnEventRaised;
        public virtual void RaiseEvent(object? value)
        {
            OnEventRaised?.Invoke(value);
        }
        public void RegisterListener(UnityAction<object?> listener)
        {
            OnEventRaised += listener;
        }
        public void UnregisterListener(UnityAction<object?> listener)
        {
            OnEventRaised -= listener;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SOEventBase), true)]
    public class SOEventBaseEditor : Editor
    {
        SOEventBase? m_SOEventBase;

        void OnEnable()
        {
            m_SOEventBase = (SOEventBase)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // 绘制 OnEventRaised 运行时监听器
            if (m_SOEventBase == null || m_SOEventBase.OnEventRaised == null)
                return;

            var invocations = m_SOEventBase.OnEventRaised.GetInvocationList();
            if (invocations.Length == 0) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Runtime Listeners", invocations.Length.ToString());

            foreach (var d in invocations)
            {
                EditorGUILayout.BeginVertical();

                var targetObj = d.Target as UnityEngine.Object;

                if (targetObj != null)
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField(targetObj, typeof(UnityEngine.Object), true);
                    GUI.enabled = true;
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
#endif
}

