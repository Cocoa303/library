using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public static partial class Function
    {
        private class KeyMaker
        {
            private int _accumulated = 0;
            public int Create()
            {
                return _accumulated++;
            }
        }

        private static KeyMaker _keyMaker = new();
        public static int CreateKey() => _keyMaker.Create();
    }
}
