using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEditor;

#if HAS_DOTWEEN
using DG.Tweening;

namespace Cocoa.Util.Production
{
    //== EventTrigger not supported.
    public class UI : MonoBehaviour
    {
        public enum Event
        {
            OnOpen,
            OnClose,
            OnClick
        }

        public enum AnimationType
        {
            #region Default Animations
            Punch,
            Rotation,
            ScrollUp,
            ScrollDown,
            ScrollDrop,
            ComeOut,
            ComeIn,
            EnterFromLeft,
            EnterFromRight,
            OutToLeft,
            OutToRight,
            #endregion
            Custom
        }

        [System.Serializable]
        public class Info
        {
            public RectTransform target;
            public Event eventType;
            public AnimationType animationType;
            public bool fixTransform;

            //== Custom reference
            [ShowIf("animationType", true, AnimationType.Custom)]
            public Vector2 startPositionOffset;
            [ShowIf("animationType", true, AnimationType.Custom)]
            public Vector2 endPositionOffset;
            [ShowIf("animationType", true, AnimationType.Custom)]
            public AnimationCurve curve;

            //== Animation reference
            [Header("Animation reference")]
            [ReadOnly] public Vector2 defaultPosition;
            [ReadOnly] public Vector2 defaultScale;
            public Tweener hasTween;

            //== Detailed settings
            [Header("Set Detail")]
#if UNITY_EDITOR
            public bool autoSetDuration = true;
#endif
            public float duration = -1f;

            public Info Clone()
            {
                Info info = new Info();

                info.target = target;
                info.eventType = eventType;
                info.animationType = animationType;

                info.defaultPosition = defaultPosition;
                info.defaultScale = defaultScale;

                info.duration = duration;

                return info;
            }
        }

        #region Inner members
        [SerializeField, ArrayTitle("target")] List<Info> info;

        //== Used to control UI interactions during the effect.
        [SerializeField] List<UIBehaviour> enabledControls;
        [SerializeField] bool autoSetEnabledControls;

        //== Flag to prevent duplicate execution while the animation is in progress.
        [SerializeField, ReadOnly] bool isOpenRunning;
        [SerializeField, ReadOnly] bool isCloseRunning;

        //== Flag for forced termination regardless of animation progress.
        [SerializeField] bool isForcedClose;

        [SerializeField, ReadOnly] float width;
        [SerializeField, ReadOnly] float height;

        Coroutine closeRoutine;
        Coroutine openRoutine;
        #endregion End - Inner members

        #region Property list
        public bool IsOpenRunning { get => isOpenRunning; }
        public bool IsCloseRunning { get => isCloseRunning; }
        public bool ForceCloseFlag { get => isForcedClose; }

        #endregion End - Property list

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;

            //== OnClick event cannot be applied if the target is not a Button.
            if (Selection.activeGameObject == this.gameObject)
            {
                if (info != null)
                {
                    for (int i = 0; i < info.Count; i++)
                    {
                        if (info[i].eventType == Event.OnClick)
                        {
                            if (info[i].target != null)
                            {
                                if (info[i].target.GetComponent<Button>() == null)
                                {
                                    Debug.LogError("Production target is not button");
                                    info[i].target = null;
                                }
                            }
                        }
                    }
                }
            }

            //== Caches the original position and scale to avoid issues with the effect.
            if (Selection.activeGameObject == this.gameObject)
            {
                if (info != null)
                {
                    for (int i = 0; i < info.Count; i++)
                    {
                        if (info[i].target != null)
                        {
                            info[i].defaultPosition = info[i].target.anchoredPosition;
                            info[i].defaultScale = info[i].target.localScale;

                            if (info[i].autoSetDuration)
                            {
                                info[i].duration = AutoDuration(info[i].animationType);
                            }
                        }
                    }
                }
            }

            if (autoSetEnabledControls)
            {
                if (enabledControls != null) enabledControls.Clear();
                else enabledControls = new List<UIBehaviour>();

                UIBehaviour[] uis = GetComponentsInChildren<UIBehaviour>(true);
                for (int i = 0; i < uis.Length; i++)
                {
                    if (uis[i].enabled)
                    {
                        enabledControls.Add(uis[i]);
                    }
                }
            }
#endif
        }

        public void Init(float width, float height)
        {
            //== Button Flag set
            for (int i = 0; i < info.Count; i++)
            {
                int index = i;
                Info info = this.info[index];

                if (info.eventType == Event.OnClick)
                {
                    Button button = info.target.GetComponent<Button>();

                    if (button != null)
                    {
                        button.onClick.AddListener(() =>
                        {
                            RunProduction(info);
                        });
                    }
                }
            }

            this.width = width;
            this.height = height;
        }
        public void CallOpenProcessing()
        {
            if (openRoutine == null)
            {
                openRoutine = StartCoroutine(OpenningProduction());
            }
        }

        public void CallCloseProduction()
        {
            if (closeRoutine == null)
            {
                closeRoutine = StartCoroutine(CloseProduction());
            }
        }

        public Tweener RunProduction(Info info)
        {
            RectTransform transform = info.target;

            if (info.hasTween != null)
            {
                info.hasTween.Kill(true);
            }

            switch (info.animationType)
            {
                case AnimationType.Punch:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.DOPunchScale(Vector3.one * 0.2f,
                        info.duration)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.Rotation:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.transform.DORotate(new Vector3(0, 0, -360) * (int)(info.duration + 1),
                        info.duration,
                        RotateMode.FastBeyond360)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                            info.target.rotation = Quaternion.Euler(
                                info.target.rotation.eulerAngles.x,
                                info.target.rotation.eulerAngles.y,
                                info.target.rotation.eulerAngles.z + (new Vector3(0, 0, 360) * (int)(info.duration + 1)).z);
                        });

                case AnimationType.ScrollDrop:
                    TransformSet(info,
                        info.defaultPosition + new Vector2(0, height * 2),
                        info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                        info.defaultPosition,
                        info.duration)
                        .SetEase(Ease.InOutBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.ScrollUp:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                        info.defaultPosition + new Vector2(0, height * 2),
                        info.duration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.ScrollDown:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                        info.defaultPosition - new Vector2(0, height * 2),
                        info.duration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.ComeOut:
                    TransformSet(info,
                        info.defaultPosition,
                        Vector3.zero);

                    return info.hasTween = info.target.DOScale(
                        info.defaultScale,
                        info.duration)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.ComeIn:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.DOScale(
                        Vector3.zero,
                        info.duration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.EnterFromLeft:
                    TransformSet(info,
                        info.defaultPosition - new Vector2(width, 0),
                        info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                        info.defaultPosition,
                        info.duration)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.EnterFromRight:
                    TransformSet(info,
                        info.defaultPosition + new Vector2(width, 0),
                        info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                        info.defaultPosition,
                        info.duration)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.OutToLeft:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.DOLocalMove(
                        info.defaultPosition - new Vector2(width, 0),
                        info.duration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });

                case AnimationType.OutToRight:
                    TransformSet(info,
                        info.defaultPosition,
                        info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                         info.defaultPosition + new Vector2(width, 0),
                         info.duration)
                         .SetEase(Ease.InBack)
                         .OnComplete(() =>
                         {
                             info.hasTween = null;
                         });

                case AnimationType.Custom:
                    TransformSet(info,
                       info.defaultPosition + info.startPositionOffset,
                       info.defaultScale);

                    return info.hasTween = info.target.DOAnchorPos(
                        info.defaultPosition + info.endPositionOffset,
                        info.duration)
                        .SetEase(info.curve)
                        .OnComplete(() =>
                        {
                            info.hasTween = null;
                        });
            }

            return null;

            void TransformSet(Info production, Vector2 position, Vector3 scale)
            {
                if (production.fixTransform == false || production.target.localScale == Vector3.zero)
                {
                    production.target.transform.localScale = scale;
                }
                production.target.anchoredPosition = position;
            }
        }

        public void ForceCloseOn()
        {
            isForcedClose = true;
        }
        public void ForceCloseOff()
        {
            isForcedClose = false;
        }

        private IEnumerator CloseProduction()
        {
            isCloseRunning = true;

            EnableControl(false);

            List<Info> infos = info.FindAll((v) => v.eventType == Event.OnClose);
            List<Tweener> closeTweeners = ListPool<Tweener>.Get();

            if (infos != null && infos.Count != 0)
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    Tweener tween = RunProduction(infos[i]);
                    if (tween != null)
                    {
                        closeTweeners.Add(tween);
                    }
                }
            }
            else
            {
                //== External yield return : need it.
                yield return null;
                ListPool<Tweener>.Release(closeTweeners);
                EnableControl(true);
                closeRoutine = null;
                isCloseRunning = false;
                yield break;
            }

            yield return new WaitUntil(() =>
            {
                if (isForcedClose) return true;
                if (closeTweeners == null) return true;

                return closeTweeners.TrueForAll((tween) =>
                {
                    return tween.IsPlaying() == false;
                });
            });

            if (isForcedClose)
            {
                if (closeTweeners != null)
                {
                    for (int i = 0; i < closeTweeners.Count; i++)
                    {
                        if (closeTweeners[i].IsPlaying())
                        {
                            closeTweeners[i].Kill();
                        }
                    }
                }

            }

            ListPool<Tweener>.Release(closeTweeners);
            EnableControl(true);
            //== Kill Coroutine flag
            closeRoutine = null;
            isCloseRunning = false;
        }
        private IEnumerator OpenningProduction()
        {
            isOpenRunning = true;

            EnableControl(false);

            List<Info> infos = info.FindAll((v) => v.eventType == Event.OnOpen);
            List<Tweener> openTweeners = ListPool<Tweener>.Get();

            if (infos != null && infos.Count != 0)
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    Tweener tween = RunProduction(infos[i]);
                    if (tween != null)
                    {
                        openTweeners.Add(tween);
                    }
                }
            }
            else
            {
                yield return null;
                ListPool<Tweener>.Release(openTweeners);

                EnableControl(true);

                openRoutine = null;
                isOpenRunning = false;
                yield break;
            }

            yield return new WaitUntil(() =>
            {
                if (openTweeners == null) return true;

                return openTweeners.TrueForAll((tween) =>
                {
                    return tween.IsPlaying() == false;
                });
            });

            EnableControl(true);

            ListPool<Tweener>.Release(openTweeners);

            //== Kill Coroutine flag
            openRoutine = null;
            isOpenRunning = false;
        }

        private float AutoDuration(AnimationType types)
        {
            switch (types)
            {
                case AnimationType.Punch:
                case AnimationType.Rotation: return 0.5f;

                case AnimationType.ScrollDrop:
                case AnimationType.ScrollDown:
                case AnimationType.ScrollUp: return 1.0f;
                case AnimationType.ComeOut:
                case AnimationType.ComeIn:
                case AnimationType.EnterFromLeft:
                case AnimationType.EnterFromRight:
                case AnimationType.OutToLeft:
                case AnimationType.OutToRight: return 0.8f;
            }

            return 0.2f;
        }

        private void EnableControl(bool enabled)
        {
            if (enabledControls != null && enabledControls.Count != 0)
            {
                for (int i = 0; i < enabledControls.Count; i++)
                {
                    enabledControls[i].enabled = enabled;
                }
            }
        }
    }

}
#endif