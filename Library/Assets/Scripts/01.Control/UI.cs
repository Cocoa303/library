using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cocoa.Control
{
    public class UI
    {
        private Dictionary<string, IRegisterUI> database = new Dictionary<string, IRegisterUI>();
        private List<IRegisterUI> opened = new List<IRegisterUI>();

        #region Register & UnRegister
        public bool Register(IRegisterUI ui)
        {
            string id = ui.GetID();

            if (!database.ContainsKey(id))
            {
                database.Add(id, ui);
                ui.Register();

                return true;
            }

            return false;
        }

        public bool IsRegister(IRegisterUI ui)
        {
            string id = ui.GetID();
            return IsRegister(id);
        }

        public bool IsRegister(string id)
        {
            return database.ContainsKey(id);
        }

        public bool UnRegister(IRegisterUI ui)
        {
            string id = ui.GetID();

            return UnRegister(id);
        }

        public bool UnRegister(string id)
        {
            if (database.ContainsKey(id))
            {
                return database.Remove(id);
            }

            return false;
        }
        #endregion

        #region Open & Close
        public bool Open(string id, bool checkRegist = true)
        {
            if (!database.ContainsKey(id))
            {
                return false;
            }

            return Open(database[id], checkRegist);
        }
        public bool Open(IRegisterUI ui, bool checkRegist = true)
        {
            if(checkRegist && !IsRegister(ui))
            {
                return false;
            }

            //== Duplicate check for UI that cannot be opened multiple times
            bool isSingle = ui.IsSingle();
            if (isSingle)
            {
                foreach (var item in opened)
                {
                    if(item == ui) 
                    { 
                        return false; 
                    }
                }
            }

            ui.Open();
            opened.Add(ui);

            return true;
        }
        public bool Close(string id)
        {
            if (!database.ContainsKey(id))
            {
                return false;
            }

            return Close(database[id]);
        }

        //== Closes the most recently opened UI first.
        public bool Close(IRegisterUI ui)
        {
            int findIndex = opened.FindLastIndex((item) => item == ui);

            if (findIndex != -1)
            {
                ui.Close();
                opened.RemoveAt(findIndex);

                return true;
            }

            return false;
        }

        //== Closes the UI opened most recently.
        public bool CloseRecentUI()
        {
            if (opened.Count <= 0)
            {
                return false;
            }

            IRegisterUI ui = opened[opened.Count - 1];
            ui.Close();

            opened.RemoveAt(opened.Count - 1);

            return true;
        }
        #endregion

        #region Check
        public bool IsOpend(IRegisterUI ui)
        {
            if (opened.Count == 0) { return false; }

            int findIndex = opened.FindIndex((item)=> item == ui);
            if (findIndex != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        public List<IRegisterUI> Search(Predicate<IRegisterUI> predicate)
        {
            return opened.FindAll(predicate);
        }

        public void RemoveOpenRecords(IRegisterUI ui)
        {
            int findIndex = opened.FindLastIndex((item) => item == ui);

            if (findIndex != -1)
            {
                opened.RemoveAt(findIndex);
            }
        }

        public (bool, IRegisterUI) GetUI(string id)
        {
            if (database.ContainsKey(id))
            {
                return (true, database[id]);
            }

            return (false, null);
        }
    }
}
