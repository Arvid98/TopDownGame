using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite caveManSprite;
    [SerializeField] private Sprite caveWomanSprite;
    [SerializeField] private Sprite caveManTwoSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManager.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManager.PlayerCharacter.Cock:   return caveManSprite;
            case LobbyManager.PlayerCharacter.Builder:    return caveWomanSprite;
            case LobbyManager.PlayerCharacter.BlackSmith:   return caveManTwoSprite;
        }
    }

}