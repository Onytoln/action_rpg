using System;

public interface ICooldown {
    Action<ICooldown> OnCooldownStart { get; set; }
    Action<ICooldown> OnCooldownChanged { get; set; }
    Action<ICooldown> OnCooldownEnd { get; set; }

    float CurrentCooldown { get; }
    float CurrentStartingCooldown { get; }

    bool InCooldownUnusable { get; set; }

    void ApplyCooldown(float cooldownTime, float startingCooldown = 0);
    bool HandleCooldown(float deltaTime);
}
