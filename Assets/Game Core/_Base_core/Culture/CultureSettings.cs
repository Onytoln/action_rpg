using System.Globalization;
using UnityEngine;

public class CultureSettings : MonoBehaviour
{
    private void Awake() {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
    }
}
