using UnityEngine;

public static class StatStringConversion {
    public static string StatValueToStringByStatStringType(this float statValue, StatStringType stringType, int decimals = 2) {
        decimals = Mathf.Clamp(decimals, 0, 3);

        string format = "0.000";
        if(decimals == 2) {
            format = "0.00";
        } else if (decimals == 1) {
            format = "0.0";
        } else if(decimals == 0) {
            format = "0";
        }

        return stringType switch {
            StatStringType.Absolute => statValue.ToString(format),
            StatStringType.Percentage => string.Format("{0:" + format + "} %", statValue * 100),
            StatStringType.MovementSpeed => string.Format("{0:" + format + "} m/s", statValue),
            StatStringType.PerSecond => string.Format("{0:" + format + "} /s", statValue),
            StatStringType.Seconds => string.Format("{0:" + format + "} s", statValue),
            StatStringType.Meters => string.Format("{0:" + format + "} m", statValue),
            _ => "Undefined type",
        };
    }

    public static string StatValueToStringByStatStringTypeNoSpace(this float statValue, StatStringType stringType, int decimals = 2) {
        decimals = Mathf.Clamp(decimals, 0, 3);

        string format = "0.000";
        if (decimals == 2) {
            format = "0.00";
        } else if (decimals == 1) {
            format = "0.0";
        } else if (decimals == 0) {
            format = "0";
        }

        return stringType switch {
            StatStringType.Absolute => statValue.ToString("0.00"),
            StatStringType.Percentage => string.Format("{0:" + format + "}%", statValue * 100),
            StatStringType.MovementSpeed => string.Format("{0:" + format + "} m/s", statValue),
            StatStringType.PerSecond => string.Format("{0:" + format + "}/s", statValue),
            StatStringType.Seconds => string.Format("{0:" + format + "}s", statValue),
            StatStringType.Meters => string.Format("{0:" + format + "}m", statValue),
            _ => "Undefined type",
        };
    }
}
