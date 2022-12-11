using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoadHandler {

    private readonly static Dictionary<ILoadable, List<Action<ILoadable>>> onLoadNotificationRequests = new Dictionary<ILoadable, List<Action<ILoadable>>>();
    private readonly static HashSet<ILoadable> subscribedTo = new HashSet<ILoadable>();

    public static void NotifyOnLoad(ILoadable loadable, Action<ILoadable> onLoadNotification) {
        if (loadable == null) return;

        if (loadable.IsLoaded) {
            onLoadNotification.SafeInvoke(loadable);
        } else {
            Utils.AddToDictList(onLoadNotificationRequests, loadable, onLoadNotification);

            if (!subscribedTo.Contains(loadable)) {
                loadable.OnLoad += LoadableLoaded;
                subscribedTo.Add(loadable);
            }
        }
    }

    private static void LoadableLoaded(ILoadable loadable) {
        if(onLoadNotificationRequests.TryGetValue(loadable, out var notifList)) {
            for (int i = 0; i < notifList.Count; i++) {
                notifList[i].SafeInvoke(loadable);
            }

            onLoadNotificationRequests.Remove(loadable);
        }

        subscribedTo.Remove(loadable);
        loadable.OnLoad -= LoadableLoaded;
    }
}
