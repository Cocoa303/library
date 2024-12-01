using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Cocoa.Control;

public class Test : MonoBehaviour
{
    public enum DDDDSSS
    {
        AAAA,
        BBBB,
        CCCC
    }

    public enum WWWKKK
    {
        RRRR,
        EEEE,
        GGGG
    }

    // Start is called before the first frame update
    void Start()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for(int i = 0; i < 1000000; i++)
        {
            string aaa = DDDDSSS.AAAA.ToString();
            string bbb = DDDDSSS.BBBB.ToString();
            string ccc = DDDDSSS.CCCC.ToString();

            string rrr= WWWKKK.RRRR.ToString();
            string eee = WWWKKK.EEEE.ToString();
            string ggg = WWWKKK.GGGG.ToString();
        }

        stopwatch.Stop();
        UnityEngine.Debug.Log($"���� ���� �ð� {stopwatch.ElapsedMilliseconds}");
        long gc = System.GC.GetTotalMemory(false);
        UnityEngine.Debug.Log($"���� Memory �Һ� {gc}");

        stopwatch.Reset();
        stopwatch.Start();

        for (int i = 0; i < 1000000; i++)
        {
            string aaa = EnumString<DDDDSSS>.ToString(DDDDSSS.AAAA);
            string bbb = EnumString<DDDDSSS>.ToString(DDDDSSS.BBBB);
            string ccc = EnumString<DDDDSSS>.ToString(DDDDSSS.CCCC);

            string rrr = EnumString<WWWKKK>.ToString(WWWKKK.RRRR);
            string eee = EnumString<WWWKKK>.ToString(WWWKKK.EEEE);
            string ggg = EnumString<WWWKKK>.ToString(WWWKKK.GGGG);
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"ĳ�� ���� �ð� {stopwatch.ElapsedMilliseconds}");
        UnityEngine.Debug.Log($"ĳ�� Memory �Һ� {(System.GC.GetTotalMemory(false) - gc)}");
    }

}
