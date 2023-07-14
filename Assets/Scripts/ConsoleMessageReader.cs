using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConsoleMessageReader : MonoBehaviour
{
    [SerializeField] private TMP_Text messageTextTemplate;
    [SerializeField] private Transform messageTextHolder;

    [SerializeField] private Button infinityLogButton;
    [SerializeField] private Button clearLogButton;

    [SerializeField] private Color infoTextColor = Color.white;
    [SerializeField] private Color warningTextColor = Color.yellow;
    [SerializeField] private Color errorTextColor = Color.red;

    [SerializeField] private int poolSize = 10;

    [SerializeField] private bool infinityLog = false;


    private int activeMessageCount;

    private void Awake()
    {
        CreateMessageTextPool();

        infinityLogButton.onClick.AddListener(() => {
            infinityLog = !infinityLog;
            CreateMessageText("INFINITE LOG MODE: " + infinityLog, Color.green);
        });

        clearLogButton.onClick.AddListener(() => {
            ClearLog();
        });
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        Color textColor = SetMessageColor(type);
        CreateMessageText(logString, textColor);
    }

    private void CreateMessageTextPool()
    {
        if (messageTextTemplate != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                var text = Instantiate(messageTextTemplate, messageTextHolder, false);
                text.gameObject.SetActive(false);
            }
        }
    }

    private void CreateMessageText(string text, Color messageColor)
    {
        TMP_Text poolText;

        if (infinityLog)
        {
            poolText = Instantiate(messageTextTemplate, messageTextHolder, false);
        }
        else
        {
            poolText = messageTextHolder.GetChild(messageTextHolder.childCount - 1).GetComponent<TMP_Text>();
        }

        poolText.color = messageColor;
        poolText.gameObject.SetActive(true);

        string message = "[" + DateTime.Now.TimeOfDay.ToString().Substring(0, 8) + "] " + text;
        poolText.text = message;
        poolText.transform.SetAsFirstSibling();
    }

    private Color SetMessageColor(LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            return errorTextColor;
        }
        else if (type == LogType.Warning)
        {
            return warningTextColor;
        }
        else
        {
            return infoTextColor;
        }
    }

    private void ClearLog()
    {
        for (int i = 0; i < messageTextHolder.childCount; i++)
        {
            if (i < poolSize)
            {
                messageTextHolder.GetChild(i).gameObject.SetActive(false);
            }
            else
            {
                Destroy(messageTextHolder.GetChild(i).gameObject);
            }
        }

        CreateMessageText("LOG CLEARED!", Color.green);
    }
}
