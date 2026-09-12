using System;
using UnityEngine.Events;

// Unity no dibuja bien un UnityEvent<T> genérico en el Inspector a menos
// que exista una subclase concreta y serializable. Estas son las que usan
// FelineAbilityBase y FelineAbilityController para exponer sus eventos al
// HUD sin acoplarse a él.

[Serializable] public class FloatUnityEvent : UnityEvent<float> { }
[Serializable] public class BoolUnityEvent : UnityEvent<bool> { }
[Serializable] public class IntUnityEvent : UnityEvent<int> { }
[Serializable] public class FelineAbilityStateUnityEvent : UnityEvent<FelineAbilityState> { }
[Serializable] public class FelineAbilityUnityEvent : UnityEvent<FelineAbilityBase> { }
