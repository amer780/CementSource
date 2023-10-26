using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CementTools.Modules.NotificationModule
{
    public class Notification : MonoBehaviour
    {
        public static event Action<Notification> OnNotificationStart;
        public static event Action<Notification, bool> OnNotificationClosed;
        public static event Action<Notification, ContentType> OnTextUpdated;

        private readonly static List<Notification> notifications = new List<Notification>();

        public string titleText = "EMPTY TITLE";
        public string contentText = "EMPTY MESSAGE";
        public float time = 1f;

        public int ID { get; private set; }

        private float curTime = 1f;

        [SerializeField] private Button closeButton;
        [SerializeField] private Slider timerBar;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text content;

        // exception messages
        private readonly string timerBarOrCloseButtonNullMessage = "Notification does not have a timerBar or a closeButton! Notification will not display correctly!";
        private readonly string titleOrContentNullMessage = "Notification title or content is unassigned! Notification will not display correct text!";

        public enum ContentType
        {
            Title,
            Content,
        }

        private void Start()
        {
            notifications.Add(this);
            ID = notifications.Count;

            curTime = time;

            // null handling
            if (timerBar == null) timerBar = GetComponentInChildren<Slider>();
            if (closeButton == null) closeButton = GetComponentInChildren<Button>();
            if (title == null) title = transform.Find("bg/Content (1)").GetComponent<TMPro.TMP_Text>();
            if (content == null) content = transform.Find("bg/Scroll View/Viewport/Content").GetComponent<TMP_Text>();

            if (title == null || content == null)
            {
                Cement.Log(titleOrContentNullMessage, BepInEx.Logging.LogLevel.Error);
            }

            if (timerBar == null || closeButton == null)
            {
                Cement.Log(timerBarOrCloseButtonNullMessage, BepInEx.Logging.LogLevel.Error);
                return;
            }

            closeButton?.onClick.AddListener(() => CloseNotification());

            OnNotificationStart?.Invoke(this);
        }

        private void Update()
        {
            try
            {
                // only update title's actual UI text if its different than titleText
                if (title != null && title.text != titleText)
                {
                    UpdateText(ContentType.Title);
                }
                // only update content's actual UI text if its different than contentText
                if (content != null && content.text != contentText)
                {
                    UpdateText(ContentType.Content);
                }

                // update timerBar
                if (timerBar == null) return;
                if (curTime > 0f) curTime -= Time.deltaTime; else
                {
                    CloseNotification(false);
                }
                if (timerBar.value != curTime / time) timerBar.value = curTime / time;
            }
            catch(Exception ex) // i have no idea how this could possibly throw an error but just in case
            {
                Cement.Log("Unhandled error in Notification.Update()! " + ex, BepInEx.Logging.LogLevel.Error);
            }
        }

        private void UpdateText(ContentType type)
        {
            switch(type)
            {
                case ContentType.Title:
                    if (title != null) title.text = titleText;
                    OnTextUpdated?.Invoke(this, ContentType.Title);

                    break;
                case ContentType.Content:
                    if (content != null) content.text = contentText;
                    OnTextUpdated?.Invoke(this, ContentType.Title);

                    break;
            }
        }

        public void CloseNotification(bool wasForcedClose=true)
        {
            if (wasForcedClose)
            {
                Close();
                curTime = -1f;
            }
            else
            {
                // DoCloseAnimation(delegate(){Close();});
                Close(false);
            }
        }

        private void Close(bool wasForcedClose=true)
        {
            OnNotificationClosed?.Invoke(this, wasForcedClose);
            Destroy(this);
        }
    }
}
