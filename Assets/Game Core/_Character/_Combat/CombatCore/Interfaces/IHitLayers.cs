using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitLayers {

    LayerMask GetDirectHitLayer();
    string GetDirectHitLayerName();
    LayerMask GetAbilityObjectHitLayer();
    string GetAbilityObjectHitLayerName();

    void SetHitLayer(string layerName);
    void SetObjectHitLayer(string layerName);
}
