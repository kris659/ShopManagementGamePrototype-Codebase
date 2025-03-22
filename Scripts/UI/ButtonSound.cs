using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    private void Awake()
    {
        Button button = GetComponent<Button>();
        if(button != null) {
            button.onClick.AddListener(() => AudioManager.PlaySound(Sound.UIButtonClicked));
        }
    }
}
