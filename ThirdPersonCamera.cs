using UnityEngine;

namespace Alezinfire
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public float yaw, pitch = 15f, distance = 4.2f;
        float kick;

        public void Kick(float k = 1f) { kick = Mathf.Min(kick + k, 6f); }

        void LateUpdate()
        {
            if (!target) return;
            float sens = Config.Sensitivity;
            yaw += InputHub.Look.x * sens;
            pitch = Mathf.Clamp(pitch - InputHub.Look.y * sens, -35f, 75f);
            kick = Mathf.Lerp(kick, 0f, 10f * Time.deltaTime);
            var rot = Quaternion.Euler(pitch - kick, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * 1.6f;
            Vector3 want = pivot + rot * new Vector3(.55f, .2f, -distance);
            Vector3 pos = want;
            RaycastHit hit;
            if (Physics.Linecast(pivot, want, out hit, ~0, QueryTriggerInteraction.Ignore) && !hit.collider.transform.IsChildOf(target.root))
                pos = hit.point + hit.normal * .25f;
            transform.position = pos;
            transform.rotation = rot;
        }
    }
}
