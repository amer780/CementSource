using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace CementTools.Modules.NotificationModule
{
    /// <summary>
    /// The main accessor for Notifications. Simply use NotificationModule.Send(...) to create one.
    /// </summary>
    public class NotificationModule : CementMod
    {
        private static readonly List<NotificationInfo> notificationsQueue = new();
        private static readonly List<Notification> activeNotifications = new();
        private static readonly int maxActiveNotifications = 6;

        private static AssetBundle Bundle => Cement.Bundle;

        public static GameObject NotificationPrefab
        {
            get
            {
                _notificationGO ??= Bundle.LoadAsset("Notification", Il2CppType.Of<GameObject>()).Cast<GameObject>();
                return _notificationGO;
            }
            private set
            {
                _notificationGO = value;
            }
        }
        private static GameObject _notificationGO;

        public static Transform NotificationCanvas
        {
            get
            {
                if (_canvasTransform is null)
                {
                    _canvasTransform = Bundle.LoadAsset("NotificationCanvas", Il2CppType.Of<GameObject>()).Cast<GameObject>().transform;
                    DontDestroyOnLoad(_canvasTransform);
                }
                return _canvasTransform;
            }
            private set
            {
                _canvasTransform = value;
            }
        }
        private static Transform _canvasTransform;

        public static Transform NotificationContainer
        {
            get
            {
                _containerTransform ??= NotificationCanvas.GetComponentInChildren<VerticalLayoutGroup>().transform;
                return _containerTransform;
            }
            private set
            {
                _containerTransform = value;
            }
        }
        private static Transform _containerTransform;

        public NotificationModule(IntPtr ptr) : base(ptr)
        {
        }

        private static void Update()
        {
            foreach (var notification in notificationsQueue.ToArray())
            {
                if (activeNotifications.Count >= maxActiveNotifications) break;
                SpawnNotification(notification);
                notificationsQueue.Remove(notification);
            }
        }

        public static void Send(string title="hello title!", string content="hello content!", float timer = 1f)
        {
            NotificationInfo info = new(title, content, timer);

            notificationsQueue.Add(info);
        }

        private static Notification SpawnNotification(NotificationInfo info)
        {
            GameObject notifObj = Instantiate(NotificationPrefab, NotificationContainer);
            Notification notif = notifObj.GetComponent<Notification>();
            if (notif == null)
            {
                Cement.Log("Could not find Notification component on prefab. Attempting to create one.");
                notif = notifObj.AddComponent<Notification>();
            }

            notif.titleText = info.title;
            notif.contentText = info.content;
            notif.time = info.time;

            activeNotifications.Add(notif);
            Notification.OnNotificationClosed += (notify, wasForced) =>
            {
                if (notify == notif)
                    activeNotifications.Remove(notif);
            };
            return notif;
        }
    }

    public struct NotificationInfo
    {
        public string title;
        public string content;
        public float time;

        public NotificationInfo(string title, string content, float timer)
        {
            this.title = title;
            this.content = content;
            time = timer;
        }
    }
}
