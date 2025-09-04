using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace theArkitectPackage.Editor
{
    public class LocalizationData : ScriptableObject
    {
        [ReadOnly, ShowInInspector] 
        public string LOC_RootPath = "Assets/Editor/";
    }
}