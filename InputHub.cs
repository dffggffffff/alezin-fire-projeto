using UnityEngine;
using UnityEngine.EventSystems;

namespace Alezinfire
{
    public static class InputHub
    {
        public static Vector2 Move, Look, UiMove, UiLook;
        public static bool Fire, UiFire;
        public static bool JumpDown, CrouchDown, ReloadDown, PickupDown, SwapDown;
        public static bool UiJump, UiCrouch, UiReload, UiPickup, UiSwap;

        public static void Poll()
        {
            Vector2 k = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Move = UiMove.sqrMagnitude > 0.01f ? UiMove : Vector2.ClampMagnitude(k, 1f);
            Look = UiLook;
            if (Input.GetMouseButton(1)) Look += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 3f;
            UiLook = Vector2.zero;
            Fire = UiFire || Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.LeftControl);
            JumpDown = UiJump || Input.GetKeyDown(KeyCode.Space);
            CrouchDown = UiCrouch || Input.GetKeyDown(KeyCode.C);
            ReloadDown = UiReload || Input.GetKeyDown(KeyCode.R);
            PickupDown = UiPickup || Input.GetKeyDown(KeyCode.E);
            SwapDown = UiSwap || Input.GetKeyDown(KeyCode.Q);
            UiJump = UiCrouch = UiReload = UiPickup = UiSwap = false;
        }
    }

    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform knob;
        public float radius = 110f;
        public void OnPointerDown(PointerEventData e) { OnDrag(e); }
        public void OnDrag(PointerEventData e)
        {
            Vector2 p;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out p);
            Vector2 v = Vector2.ClampMagnitude(p / radius, 1f);
            if (knob) knob.anchoredPosition = v * radius;
            InputHub.UiMove = v;
        }
        public void OnPointerUp(PointerEventData e) { if (knob) knob.anchoredPosition = Vector2.zero; InputHub.UiMove = Vector2.zero; }
        void OnDisable() { InputHub.UiMove = Vector2.zero; }
    }

    public class LookPad : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData e) { InputHub.UiLook += e.delta * 0.12f; }
    }

    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData e) { InputHub.UiFire = true; }
        public void OnPointerUp(PointerEventData e) { InputHub.UiFire = false; }
        void OnDisable() { InputHub.UiFire = false; }
    }
}
