using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Manager
{
    public class UI : Util.Inherited.DisposableSingleton<UI>
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform uiParent;

        //== Cover to block background clicks in UI
        [SerializeField] private RectTransform backgroundCover;

        [SerializeField] private List<Base.UI> database;
        [SerializeField] private Control.UI control;

        //== Work to prevent UI from remaining open during Play or Build when it is in the middle of processing
        [SerializeField] private List<UIBehaviour> toCloseList;

        [Header("Has Data")]
        [SerializeField] private HashSet<Base.UI> onCoverUIs;

        private void OnValidate()
        {
            #region Create
            //== Create the Main Canvas of the UI [initially once]
            if (canvas == null)
            {
                //== Canvas Create
                GameObject canvasObject = new GameObject("UI Main canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.pixelPerfect = false;
                canvas.sortingOrder = 0;
                canvas.targetDisplay = 0;
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

                //== Canvas scaler set
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 720);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                scaler.referencePixelsPerUnit = 100;

                //== Graphic raycaster set
                GraphicRaycaster raycaster = canvasObject.AddComponent<GraphicRaycaster>();
                raycaster.ignoreReversedGraphics = true;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

                //== Transform set
                canvasObject.transform.SetParent(this.transform);
            }

            if (backgroundCover == null)
            {
                GameObject uiBackgroundCover = new GameObject("Background Cover", typeof(RectTransform));

                //** Color Setting **//
                Image cover = uiBackgroundCover.AddComponent<Image>();
                cover.color = new Color(0, 0, 0, 20.0f / 255.0f);
                cover.raycastTarget = true;

                cover.transform.SetParent(canvas.transform);
                cover.transform.localScale = Vector3.one;
                cover.rectTransform.anchorMin = Vector3.zero;
                cover.rectTransform.anchorMax = Vector3.one;
                cover.rectTransform.offsetMin = Vector3.zero;
                cover.rectTransform.offsetMax = Vector3.zero;

                backgroundCover = cover.rectTransform;
                backgroundCover.gameObject.SetActive(false);
            }

            if (uiParent == null)
            {
                GameObject parent = new GameObject("UI Items", typeof(RectTransform));
                parent.transform.SetParent(canvas.transform);
                uiParent = parent.transform as RectTransform;

                uiParent.localScale = Vector3.one;
                uiParent.anchorMin = Vector3.zero;
                uiParent.anchorMax = Vector3.one;
                uiParent.offsetMin = Vector3.zero;
                uiParent.offsetMax = Vector3.zero;
            }

            EventSystem findEventSystem = GameObject.FindObjectOfType<EventSystem>(true);
            if (findEventSystem == null)
            {
                GameObject eventSystem = new GameObject("Event System");
                eventSystem.transform.SetParent(null);
                eventSystem.transform.localPosition = Vector3.zero;
                eventSystem.transform.localScale = Vector3.one;
                EventSystem system = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
            #endregion

#if UNITY_EDITOR
            //== Database flag option setting
            UnityEngine.Object caching = UnityEditor.Selection.activeObject;
            if (database != null && database.Count > 0)
            {
                for (int i = 0; i < database.Count; i++)
                {
                    UnityEditor.Selection.activeObject = database[i];

                    //== Prefabs do not have active Transform states.
                    //== Used as a method to distinguish prefabs.
                    if (UnityEditor.Selection.activeTransform != null)
                    {
                        database[i].Flag.has |= Base.UI.InnerFlag.active;
                        database[i].Flag.has |= Base.UI.InnerFlag.single;
                    }
                    else
                    {
                        database[i].Flag.has &= ~Base.UI.InnerFlag.active;
                    }
                }
            }
            UnityEditor.Selection.activeObject = caching;
#endif
        }

        private void Start()
        {
            for (int i = 0; i < database.Count; i++)
            {
                if (database[i].Flag.Exist(Base.UI.InnerFlag.active))
                {
                    control.Register(database[i]);
                }
            }

            for (int i = 0; i < toCloseList.Count; i++)
            {
                toCloseList[i].gameObject.SetActive(false);
            }

            onCoverUIs = new HashSet<Base.UI>();
        }

        public (string id, Base.UI ui) Open(string id, bool coverAble, Base.UI.Callback openCallback, Base.UI.Callback closeCallback)
        {
            var result = UIControl(id, coverAble, openCallback, closeCallback);
            var ui = result.ui;

            if (ui != null)
            {
                ui.Init(canvas.pixelRect.width, canvas.pixelRect.height, ui.GetHashCode());

                if (coverAble)
                {
                    backgroundCover.gameObject.SetActive(true);
                    onCoverUIs.Add(ui);
                }
                control.Open(ui);
                return result;
            }
            else
            {
                return result;
            }
        }

        private (string message, Base.UI ui) UIControl(string id, bool coverAble = true, Base.UI.Callback openCallback = null, Base.UI.Callback closeCallback = null)
        {
            Base.UI ui = database.Find((item) => item.GetID().CompareTo(id) == 0);
            if (ui == null)
            {
                return ($"Not find ui component [{id}]", null);
            }

            //== When an exit animation is in progress, forcefully close and then reactivate the UI.
            //== This handles cases where rapidly closing and reopening the UI prevents it from opening correctly.
            if (ui.Flag.Exist(Base.UI.InnerFlag.active)
#if HAS_DOTWEEN
                && ui.IsClosing
#endif
                )
            {
                ui.ForceClose((_) =>
                {
                    control.RemoveOpenRecords(ui);
                    Open(id, coverAble, openCallback, closeCallback);
                });
                return ("Is Close running : Invoke open running", null);
            }

            if (ui.Flag.Exist(Base.UI.InnerFlag.single) && control.IsOpend(ui))
            {
                return ("That ui is single object.", null);
            }

            Base.UI baseUI = null;
            if (ui.Flag.Exist(Base.UI.InnerFlag.active))
            {
                ui.gameObject.SetActive(true);
                baseUI = ui;
            }
            else
            {
                baseUI = Instantiate(ui);
                baseUI.gameObject.name = "Clone : " + ui.GetID() + "[ Hash : " + ui.Hash + " ]";

                //== Hierarchy transform set
                baseUI.transform.SetParent(uiParent.transform);
                baseUI.transform.localPosition = Vector3.zero;
                baseUI.transform.localScale = Vector3.one;
                baseUI.transform.rotation = Quaternion.identity;
            }

            //== Set priority
            var priority = control.Search((v) => v.GetPriority() <= ui.GetPriority());

            if (priority != null)
            {
                int indexer = priority.Count;
                baseUI.transform.SetSiblingIndex(indexer);
            }
            else
            {
                baseUI.transform.SetAsFirstSibling();
            }

            baseUI.CloseButton.onClick.RemoveAllListeners();
            baseUI.CloseButton.onClick.AddListener(() => ui.Close());

            baseUI.SetEvent(openCallback, closeCallback);

            return ("Success", baseUI);
        }
    
        public void Close(Base.UI ui)
        {
            if(!ui.IsClosing)
            {
                bool closeResult = control.Close(ui);

                if (closeResult)
                {
                    if (onCoverUIs.Contains(ui))
                    {
                        onCoverUIs.Remove(ui);
                    }
                    if(onCoverUIs.Count == 0)
                    {
                        backgroundCover.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}

