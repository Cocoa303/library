#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DefineSymbols
{
    static DefineSymbols()
    {
        //== DOTWeen
        Setting("HAS_DOTWEEN", "Assets/Plugins/Demigiant/DOTween", "DOTween is not installed. This function and class cannot be used.");
    }

    private static void Setting(string symbol, string path, string noPackageMessage)
    {
        string log = string.Empty;
        bool isAlreadyInstall = false;
        #region Select Build Target
        var defaultSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        if (!defaultSymbols.Contains(symbol))
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defaultSymbols + ',' + symbol);
                log += $"Defualt : {symbol} symbol has been added.\n";
            }
        }
        else
        {
            isAlreadyInstall = true;
        }

        #endregion

        #region Android
        var androidSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

        if (!androidSymbols.Contains(symbol))
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, androidSymbols + ',' + symbol);
                log += $"Android : {symbol} symbol has been added.\n";
            }
        }
        else
        {
            isAlreadyInstall = true;
        }
        #endregion

        #region IOS
        var iosSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);

        if (!iosSymbols.Contains(symbol))
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, iosSymbols + ',' + symbol);
                log += $"IOS : {symbol} symbol has been added.\n";
            }
        }
        else
        {
            isAlreadyInstall = true;
        }
        #endregion

        if (log != string.Empty)
        {
            Debug.Log(log);
        }
        else if(!isAlreadyInstall) 
        {
            Debug.Log(noPackageMessage);
        }
    }
}
#endif