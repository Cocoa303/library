using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if HAS_DOTWEEN
using Production = Util.Production.UI;
#endif

namespace Base
{
    public class UI : UIBehaviour, IRegisterUI
    {
        [System.Serializable]
        public class InnerFlag
        {
            //== Activeable
            //== 1 if it exists in the Hierarchy, 0 if it needs to be created as a Prefab
            public const int active = 0x0001;

            //== Overlap possibility
            //== 2 if only single creation is allowed, 0 if multiple creations are possible
            public const int single = 0x0002;

            //== Flag data ownership
            public int has;

            public bool Exist(int flag)
            {
                if ((has & flag) == flag) return true;
                else return false;
            }

            //** string output about flag data
            public string GetExistString()
            {
                StringBuilder makeString = new StringBuilder();
                if (Exist(active)) makeString.Append("Active, ");
                if (Exist(single)) makeString.Append("Single, ");

                if (makeString.Length != 0)
                {
                    makeString.Remove(makeString.Length - 2, 2);
                }

                return makeString.ToString();
            }
        }

        [SerializeField] protected string id;
        [SerializeField] protected int hash;
        [SerializeField] protected int priority;
        [SerializeField] protected InnerFlag flag;

        //== UI reference
        [SerializeField] protected Button closeButton;

#if HAS_DOTWEEN
        [SerializeField] protected Production production;
#endif

        public delegate void Callback(UI ui);
        private Callback openCallback;
        private Callback closeCallback;
        private Callback forceCloseCallback;

        private Coroutine openProduction;
        private Coroutine closeProduction;

        #region Property list
        public int Hash { get { return hash; } }
        public InnerFlag Flag { get { return flag; } }
        public Button CloseButton { get { return closeButton; } }

#if HAS_DOTWEEN
        public bool IsOpenning
        {
            get
            {
                if (production == null) { return false; }
                else
                {
                    return production.IsOpenRunning;
                }
            }
        }

        public bool IsClosing
        {
            get
            {
                if (production == null) { return false; }
                else
                {
                    return production.IsCloseRunning;
                }
            }
        }
#endif
        #endregion End

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            ValidateMathod();
        }

        // NOTE: The overridden OnValidate is not called automatically, so it must be created and called separately.
        protected virtual void ValidateMathod()
        {
            if (closeButton == null)
            {
                GameObject obj = new GameObject("Close");
                obj.transform.SetParent(this.transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;

                Image targetGraphic = obj.AddComponent<Image>();
                closeButton = obj.AddComponent<Button>();
                closeButton.targetGraphic = targetGraphic;
            }

#if HAS_DOTWEEN
            if (production == null)
            {
                Production production = GetComponent<Production>();
                if (production == null)
                {
                    production = gameObject.AddComponent<Production>();
                }

                this.production = production;
            }
#endif
        }
#endif
        //== NOTE: Declared for pre-processing initialization due to the nature of UI objects starting as inactive.
        public virtual void Register() { }

        public virtual void Init(float width, float height, int hash)
        {
#if HAS_DOTWEEN
            production.Init(width, height);
#endif
            this.hash = hash;
        }

        public virtual void Open()
        {
            if (openProduction == null)
            {
                openProduction = StartCoroutine(OpenProgress());
            }
        }

        public virtual void Close()
        {
            if (closeProduction == null)
            {
                closeProduction = StartCoroutine(CloseProgress(!flag.Exist(InnerFlag.active)));
            }
        }

        public void ForceClose(Callback closeCallback)
        {
#if HAS_DOTWEEN
            production.ForceCloseOn();
#endif
            forceCloseCallback = closeCallback;

            Close();
        }

        public void SetEvent(Callback openCallback, Callback closeCallback)
        {
            this.openCallback = openCallback;
            this.closeCallback = closeCallback;
        }

        IEnumerator OpenProgress()
        {
#if HAS_DOTWEEN
            if (production.IsOpenRunning == true) yield break;
            production.CallOpenProcessing();

            //== Waiting for animation to start
            yield return new WaitUntil(() => production.IsOpenRunning == true);
            yield return new WaitUntil(() => production.IsOpenRunning == false);
#else
            yield return null;
#endif
            openCallback?.Invoke(this);
            openCallback = null;

            openProduction = null;
        }

        IEnumerator CloseProgress(bool destroy)
        {
#if HAS_DOTWEEN
            if (production.IsCloseRunning == true) yield break;
            production.CallCloseProduction();

            //== Waiting for animation to start
            yield return new WaitUntil(() => production.IsCloseRunning == true);
            yield return new WaitUntil(() => production.IsCloseRunning == false);
#else
            yield return null;
#endif
            closeCallback?.Invoke(this);
            closeCallback = null;

            if (destroy) { Destroy(gameObject); }
            else
            {
                gameObject.SetActive(false);
            }

#if HAS_DOTWEEN
            if (production.ForceCloseFlag)
            {
                forceCloseCallback?.Invoke(this);
                forceCloseCallback = null;

                production.ForceCloseOff();
            }
#endif

            closeProduction = null;
        }

        public virtual string GetState()
        {
            return $"[ ID\t: {id} ]\n" +
                    $"[ Hash\t\t: {hash} ]\n" +
                    $"[ Priority\t: {priority} ]" +
                    $"[ Flag\t\t: {flag.GetExistString()} ]"
#if HAS_DOTWEEN
                    +
                    $"[ IsOpenning\t: {IsOpenning} ]" +
                    $"[ IsClosing\t: {IsClosing} ]";
#else
                    + "Animation Not has tween";

#endif
        }

        public string GetID()
        {
            return id;
        }

        public int GetPriority()
        {
            return priority;
        }

        public bool IsSingle()
        {
            return flag.Exist(InnerFlag.single);
        }
    }
}
