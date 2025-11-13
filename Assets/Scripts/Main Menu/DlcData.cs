using System;
using UnityEngine;

[Serializable]
public class DlcData
{
    public string id;          // Ex: "expansao_1"
    public string displayName; // Ex: "Expansão 1: Mundo Gelado"
    public Sprite image;
    public string downloadUrl; // Opcional, se fores mesmo fazer download.
}
