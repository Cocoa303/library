using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
    [CreateAssetMenu(fileName = "Sheet Parsing Data", menuName = "Asset/Create/SheetParsing/Data")]
    public class SheetParsingData : ScriptableObject
    {
        public string sheetURL;     //== 통합 Sheet ID
        public string listKey;      //== Index를 중복해서 처리하기 위해 들어가는 키값
        public string ignoreKey;    //== 특정 Column을 무시하기 위한 Key

        //== Sheet내의 type을 지정하기 위한 key
        public const string TypeString = "string";        
        public const string TypeInt = "int";             
        public const string TypeFloat = "float";
        public const string TypeArrayFloat = "arrayFloat";
        public const string TypeArrayString = "arrayString";
        public const string TypeArrayInt = "arrayInt";
        public const string TypeLong = "long";
        public const string TypeByte = "byte";
        public const string TypeBool = "bool";
    }
}
