using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsoleMessageReader : MonoBehaviour
{
    [SerializeField] private TMP_Text messageTextTemplate;
    [SerializeField] private GameObject messageTextHolder;
    [SerializeField] private GameObject messageTextPoolHolder;

    [SerializeField] private int poolSize = 10;
    [SerializeField] private int activeMessageLimit = 4;

    private Queue<TMP_Text> textPool = new Queue<TMP_Text>();

    private int activeMessageCount;

    private void Awake()
    {
        CreateMessageTextPool();
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
        CreateMessageText(logString);
    }

    private void CreateMessageTextPool()
    {
        if (messageTextTemplate != null)
        {
            for (int i = 0; i < poolSize; i++)
            {
                var text = Instantiate(messageTextTemplate, messageTextPoolHolder.transform, false);
                text.gameObject.SetActive(false);
                textPool.Enqueue(text);
            }
        }
    }

    private void CreateMessageText(string text)
    {
        TMP_Text poolText = textPool.Dequeue();
        poolText.gameObject.SetActive(true);
        poolText.text = text;
        poolText.gameObject.transform.SetParent(messageTextHolder.transform, false);
        poolText.transform.SetAsFirstSibling();
        activeMessageCount++;
    }

    private void DeleteMessageText(TMP_Text deletedText)
    {
        deletedText.transform.SetParent(messageTextPoolHolder.transform, false);
        deletedText.gameObject.SetActive(false);
        textPool.Enqueue(deletedText);

        activeMessageCount--;
    }
}