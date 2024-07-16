using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
    [CreateAssetMenu(fileName = "Sheet Parsing Data", menuName = "Asset/Create/SheetParsing/Data")]
    public class SheetParsingData : ScriptableObject
    {
        public string sheetURL;     //== ���� Sheet ID
        public string listKey;      //== Index�� �ߺ��ؼ� ó���ϱ� ���� ���� Ű��
        public string ignoreKey;    //== Ư�� Column�� �����ϱ� ���� Key

        //== Sheet���� type�� �����ϱ� ���� key
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
