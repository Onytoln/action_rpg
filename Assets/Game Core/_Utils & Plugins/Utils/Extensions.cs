using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class Extensions {

    public static void AppendLineMultipleTimes(this StringBuilder sb, int times = 2) {
        if (times <= 0) return;

        for (int i = 0; i < times; i++) {
            sb.AppendLine();
        }
    }
}
