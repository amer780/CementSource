using Il2CppTMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CementTools.Modules.NotificationModule
{
    // Class for individual "toasts", or notifications, or popups, or whatever you wanna call em
    public class Notification : MonoBehaviour
    {
        public Notification(IntPtr ptr) : base(ptr) { }

        public static event Action<Notification> OnNotificationStart;
        public static event Action<Notification, bool> OnNotificationClosed;
        public static event Action<Notification, ContentType> OnTextUpdated;

        private readonly static List<Notification> notifications = new List<Notification>();

        public string titleText = "EMPTY TITLE";
        public string contentText = "EMPTY MESSAGE";
        public float time = 1f;

        public int ID { get; private set; }

        private float curTime = 1f;

        public Button closeButton;
        public Slider timerBar;
        public TMP_Text title;
        public TMP_Text content;

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
            timerBar ??= GetComponentInChildren<Slider>();
            closeButton ??= GetComponentInChildren<Button>();
            title ??= transform.Find("Content (1)").GetComponent<TMP_Text>();
            content ??= transform.Find("Scroll View/Viewport/Content").GetComponent<TMP_Text>();

            if (title == null || content == null || closeButton == null || timerBar == null)
            {
                Cement.Log("Could not find some references in Notification component.");
                Destroy(gameObject);
                return;
            }

            closeButton?.onClick.AddListener((UnityAction)(() => CloseNotification()));

            OnNotificationStart?.Invoke(this);
        }

        private void Update()
        {
            // only update title's actual UI text if its different than titleText
            if (title != null && title.text != titleText)
                UpdateText(ContentType.Title);
            // only update content's actual UI text if its different than contentText
            if (content != null && content.text != contentText)
                UpdateText(ContentType.Content);

            // update timerBar
            if (timerBar == null) return;
            if (curTime > 0f) curTime -= Time.deltaTime; else
                CloseNotification(false);
            if (timerBar.value != curTime / time) timerBar.value = curTime / time;
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
            Destroy(gameObject);
        }
    }
}
