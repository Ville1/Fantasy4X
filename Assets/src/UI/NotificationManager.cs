using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour {
    public delegate void Notification_On_Click();

    public static NotificationManager Instance;

    public GameObject Parent_GameObject;
    public GameObject Notification_Panel;

    private List<NotificationData> active_notifications;
    private bool active;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;

        active_notifications = new List<NotificationData>();

        Notification_Panel.SetActive(false);
        active = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active) {
            return;
        }
        if (Input.GetMouseButtonDown(1)) {
            NotificationData notification_under_cursor = null;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(null) { position = Input.mousePosition }, results);
            foreach (RaycastResult result in results) {
                if (active_notifications.Any(x => x.Panel == result.gameObject)) {
                    notification_under_cursor = active_notifications.First(x => x.Panel == result.gameObject);
                    break;
                }
            }
            if(notification_under_cursor != null) {
                Delete_Active_Notification(notification_under_cursor, true);
            }
        }
    }

    public bool Active
    {
        get {
            return active;
        }
        set {
            active = value;
            foreach (NotificationData data in active_notifications) {
                data.Panel.SetActive(active);
            }
        }
    }

    public void Add_Notification(Notification notification)
    {
        if (Main.Instance.Other_Players_Turn) {
            return;
        }
        GameObject go = GameObject.Instantiate(Notification_Panel);
        go.SetActive(Active);
        go.transform.SetParent(Parent_GameObject.transform, false);
        go.name = "Notification" + notification.Id;
        go.transform.position = new Vector3(Notification_Panel.transform.position.x + (go.GetComponent<RectTransform>().rect.width * active_notifications.Count),
            Notification_Panel.transform.position.y, Notification_Panel.transform.position.z);
        Button button = go.GetComponentInChildren<Button>();
        button.image.overrideSprite = SpriteManager.Instance.Get_Sprite(notification.Texture, notification.Sprite_Type);

        TooltipManager.Instance.Register_Tooltip(go, notification.Name, Parent_GameObject);

        NotificationData data = new NotificationData() {
            Notification = notification,
            Panel = go
        };
        active_notifications.Add(data);

        Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
        Notification n = notification;
        on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
            active_notifications.Remove(data);
            Delete_Active_Notification(data, true);
            if(n.On_Click != null) {
                n.On_Click();
            }
        }));
        button.onClick = on_click_event;

        if (!string.IsNullOrEmpty(notification.Sound_Effect)) {
            AudioManager.Instance.Play_Sound_Effect(notification.Sound_Effect);
        }
    }

    public void Clear_Notifications()
    {
        foreach(NotificationData data in active_notifications) {
            Delete_Active_Notification(data, false);
        }
        active_notifications.Clear();
    }

    private void Delete_Active_Notification(NotificationData data, bool move_remaining_notifications)
    {
        TooltipManager.Instance.Unregister_Tooltip(data.Panel);
        GameObject.Destroy(data.Panel);
        if (!move_remaining_notifications) {
            return;
        }
        int i = 0;
        foreach(NotificationData d in active_notifications) {
            if(d == data) {
                continue;
            }
            d.Panel.transform.position = new Vector3(Notification_Panel.transform.position.x + (d.Panel.GetComponent<RectTransform>().rect.width * i),
                Notification_Panel.transform.position.y, Notification_Panel.transform.position.z);
            i++;
        }
    }

    private class NotificationData
    {
        public Notification Notification { get; set; }
        public GameObject Panel { get; set; }
    }
}
