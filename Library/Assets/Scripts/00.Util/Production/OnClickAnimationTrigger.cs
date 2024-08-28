using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Util.Production
{
    [RequireComponent(typeof(EventTrigger))]
    public class OnClickAnimationTrigger : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField, ReadOnly] EventTrigger trigger;

        [SerializeField, ReadOnly] Vector3 baseScale;
        [SerializeField, ReadOnly] Vector3 pressScale;

        [SerializeField, ReadOnly] float currentTime;
        [SerializeField] float duration = 0.05f;
        [SerializeField] float offsetPercent = 0.95f;

        Coroutine pressRoutine = null;
        Coroutine unPressRoutine = null;

        private void OnValidate()
        {
            if (Application.isEditor)
            {
                if (target == null) target = GetComponent<Transform>();
                trigger = GetComponent<EventTrigger>();
                baseScale = transform.localScale;
                pressScale = baseScale * offsetPercent;
            }
        }
        private void OnEnable()
        {
            target.transform.localScale = baseScale;
        }

        private void Start()
        {
            // OnPointerDown 이벤트에 대한 이벤트 핸들러 추가
            EventTrigger.Entry entryPointerDown = new EventTrigger.Entry();
            entryPointerDown.eventID = EventTriggerType.PointerDown;
            entryPointerDown.callback.AddListener((eventData) => { OnPointerDown(); });
            trigger.triggers.Add(entryPointerDown);

            // OnPointerUp 이벤트에 대한 이벤트 핸들러 추가
            EventTrigger.Entry entryPointerUp = new EventTrigger.Entry();
            entryPointerUp.eventID = EventTriggerType.PointerUp;
            entryPointerUp.callback.AddListener((eventData) => { OnPointerUp(); });
            trigger.triggers.Add(entryPointerUp);
        }

        public void OnPointerDown()
        {
            if (pressRoutine == null)
            {
                pressRoutine = StartCoroutine(PressRoutine());
                if (unPressRoutine != null)
                {
                    StopCoroutine(unPressRoutine);
                    unPressRoutine = null;
                }
            }
        }

        public void OnPointerUp()
        {
            if (unPressRoutine == null)
            {
                unPressRoutine = StartCoroutine(UnpressRoutine());

                if (pressRoutine != null)
                {
                    StopCoroutine(pressRoutine);
                    pressRoutine = null;
                }
            }
        }

        IEnumerator PressRoutine()
        {
            currentTime = 0.0f;
            while (currentTime <= duration && unPressRoutine == null)
            {
                if (currentTime != 0.0f)
                    target.transform.localScale = baseScale - (baseScale - pressScale) * (currentTime / duration);
                else
                    target.transform.localScale = baseScale;

                yield return null;
                currentTime += Time.deltaTime;
            }

            target.transform.localScale = pressScale;
            pressRoutine = null;
        }

        IEnumerator UnpressRoutine()
        {
            currentTime = 0.0f;
            while (currentTime <= duration * 1.05f && pressRoutine == null)
            {
                if (currentTime != 0.0f)
                    target.transform.localScale = pressScale + (baseScale - pressScale) * (currentTime / duration);
                else
                    target.transform.localScale = pressScale;

                yield return null;
                currentTime += Time.deltaTime;
            }

            target.transform.localScale = baseScale;
            unPressRoutine = null;
        }
    }

}
