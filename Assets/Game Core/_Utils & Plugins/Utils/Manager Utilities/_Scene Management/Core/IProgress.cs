using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgress {
    public bool ReportsProgress { get; }
    public float Progress { get; }
}

public class ProgressContainer : IProgress {
    public bool ReportsProgress { get; set; }
    public float Progress { get; set; }

    public ProgressContainer(bool reportsProgress) {
        ReportsProgress = reportsProgress;
    }
}
