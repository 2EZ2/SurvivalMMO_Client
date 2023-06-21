using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthDisplay : MonoBehaviour
{
    public TMP_Text m_text;
    public RiftPlayerController m_controller;


    // Update is called once per frame
    void Update()
    {
        m_text.text = m_controller.Health.ToString();
    }
}
