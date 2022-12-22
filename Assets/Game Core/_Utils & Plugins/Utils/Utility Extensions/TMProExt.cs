using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class TMProExt
{
   public static void ResetColor(this TextMeshPro tmpro) {
        tmpro.color = Color.white;
    }
}
