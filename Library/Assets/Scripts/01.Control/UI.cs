using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Control
{
    public class UI
    {
        public Dictionary<string, IRegisterUI> database = new Dictionary<string, IRegisterUI>();

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

        public (bool,IRegisterUI) GetUI(string id)
        {
            if(database.ContainsKey(id))
            {
                return (true,database[id]);
            }

            return (false, null);
        }


    }
}
