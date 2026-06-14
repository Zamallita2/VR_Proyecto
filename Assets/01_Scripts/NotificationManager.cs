using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("UI Componentes")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText;

    [Header("Configuración")]
    [SerializeField] private float displayDuration = 3f;

    private Queue<string> notificationQueue = new Queue<string>();
    private bool isDisplaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void ShowNotification(string message)
    {
        notificationQueue.Enqueue(message);
        if (!isDisplaying)
        {
            StartCoroutine(DisplayNextNotification());
        }
    }

    private IEnumerator DisplayNextNotification()
    {
        isDisplaying = true;

        while (notificationQueue.Count > 0)
        {
            string message = notificationQueue.Dequeue();
            
            if (notificationText != null)
            {
                notificationText.text = message;
            }

            if (notificationPanel != null)
            {
                notificationPanel.SetActive(true);
            }

            // Reproducir sonido de notificación
            if (SoundManager.Instance != null && SoundManager.Instance.notificationDonation != null)
            {
                SoundManager.Instance.PlaySFX(SoundManager.Instance.notificationDonation);
            }

            yield return new WaitForSeconds(displayDuration);

            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }

            // Pequeña pausa antes de mostrar la siguiente si hay varias
            yield return new WaitForSeconds(0.25f);
        }

        isDisplaying = false;
    }
}
