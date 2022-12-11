using System;

public class DamageException : Exception {
    public DamageException() { }

    public DamageException(string abilityName) : base($"Ability {abilityName} is trying to do damage with 0 input damage value modifier.") { }

}
