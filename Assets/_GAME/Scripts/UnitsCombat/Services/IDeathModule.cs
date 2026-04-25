using System;
using Cysharp.Threading.Tasks;

public interface IDeathModule
{
    UniTask HandleDeathAsync(Action beforeDisable = null);
}
