using Godot;
using System;

namespace EchoesAcrossTime.Combat
{
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Thunder,
        Ice,
        Earth,
        Wind,
        Light,
        Dark,
        Almighty,
        Physical
    }
    
    public enum ElementAffinity
    {
        Normal,    // 100% damage
        Weak,      // 150% damage
        Resist,    // 50% damage
        Immune,    // 0% damage
        Absorb,     // Heals instead of damages
        Null,
        Reflect
    }
}