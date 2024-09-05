using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Util.Production
{
    public class ScreenSaver : MonoBehaviour
    {
        [SerializeField] List<UnityEngine.Camera> disables;
        [SerializeField] List<UnityEngine.Camera> savers;
        [SerializeField] List<UnityEngine.Camera> exception;

        [Header("Setting")]
        [SerializeField] float waitTime;
        [SerializeField] float checkCycle = 0.1f;
        [SerializeField] bool checkMouseEvent;
        [SerializeField] bool checkKeyboardEvent;
        [SerializeField] bool checkTouchEvent;

#if UNITY_EDITOR
        [SerializeField, ReadOnly] string _showWaitTime;
#endif

        [Header("Has data")]
        [SerializeField, ReadOnly] bool isEnable;
        [SerializeField, ReadOnly] float currentWaitTime;
        [SerializeField, ReadOnly] float currentCheckTime;

        [SerializeField, ReadOnly] List<Inspector.UniPair<bool, UnityEngine.Camera>> targets;

        public delegate void OnEnableCallback();
        private OnEnableCallback onEnableCallback;

#if UNITY_EDITOR
        private void OnValidate()
        {
            #region Target Setting
            disables = new List<UnityEngine.Camera>();

            UnityEngine.Camera[] finds = GameObject.FindObjectsOfType<UnityEngine.Camera>();
            disables.AddRange(finds);

            //== Exclude cameras related to the screen saver
            if (savers != null)
            {
                disables.RemoveAll((target) =>
                {
                    for(int i = 0; i < savers.Count; i++)
                    {
                        if (target == savers[i]) return true;
                    }

                    return false;
                });
            }

            //== Exclude cameras related to the exception
            if (exception != null)
            {
                disables.RemoveAll((target) =>
                {
                    for (int i = 0; i < exception.Count; i++)
                    {
                        if (target == exception[i]) return true;
                    }

                    return false;
                });
            }

            #endregion

            #region Inspector : Setting wait time
            if(waitTime != 0)
            {
                long waitTime = (long)this.waitTime;
                _showWaitTime = string.Format($"{waitTime/3600}h - {(waitTime%3600)/60}m - {waitTime%60}s", waitTime);
            }
            else
            {
                _showWaitTime = "Always";
            }
            #endregion
        }
#endif
        public void SetEnableCallback(OnEnableCallback onEnableCallback)
        {
            this.onEnableCallback = onEnableCallback;
        }

        public void Enable()
        {
            if (targets != null && targets.Count != 0) return;
            if (isEnable) return;

            targets = ListPool<Inspector.UniPair<bool, UnityEngine.Camera>>.Get();

            foreach(var camera in disables)
            {
                targets.Add(new Inspector.UniPair<bool, UnityEngine.Camera>(camera.enabled, camera));
                camera.enabled = false;
            }

            foreach(var saver in savers)
            {
                saver.enabled = true;
            }

            onEnableCallback?.Invoke();
            isEnable = true;

            SetTimer();
        }

        public void Disable(System.Action onComplateDisable)
        {
            if (!isEnable) return;

            foreach (var target in targets)
            {
                target.value.enabled = target.key;
            }

            foreach (var saver in savers)
            {
                saver.enabled = false;
            }

            ListPool<Inspector.UniPair<bool, UnityEngine.Camera>>.Release(targets);
            targets = null;

            onComplateDisable?.Invoke();

            isEnable = false;
        }

        private void Update()
        {
            #region Waiting for enable
            if (isEnable) return;

            currentWaitTime += Time.deltaTime;
            if(waitTime <= currentWaitTime)
            {
                Enable();
            }
            #endregion

            currentCheckTime += Time.deltaTime;
            if(checkCycle <= currentCheckTime)
            {
                if(checkCycle != 0) currentCheckTime %= checkCycle;
                else currentCheckTime = 0;

                if(checkMouseEvent && CheckMouseEvent())
                {
                    currentWaitTime = 0;
                }
                else if(checkKeyboardEvent && CheckKeyboardEvent())
                {
                    currentWaitTime = 0;
                }
                else if(checkTouchEvent && CheckTouchEvent())
                {
                    currentWaitTime = 0;
                }
            }
        }

        private void SetTimer()
        {
            currentCheckTime = 0;
            currentWaitTime = 0;
        }

        private bool CheckMouseEvent()
        {
            //== Mouse Input Event
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) ||
                //== Scroll Event
                Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                return true;
            }
            else return false;
        }

        private bool CheckKeyboardEvent()
        {
            if (Input.anyKey) return true;
            else return false;
        }

        private bool CheckTouchEvent()
        {
            if (Input.touchCount != 0 || Input.penEventCount != 0) return true;
            else return false;
        }
    }
}