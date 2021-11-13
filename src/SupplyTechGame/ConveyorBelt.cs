using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;

namespace SupplyTechGame {

    public enum RotationDirection {
        Clockwise,
        CounterClockwise,
    }

    [RequireComponent(typeof(Collider2D))]
    public class ConveyorBelt : MonoBehaviour {

        // HIDDEN FIELDS
        private const int ROT_INCREMENT = 90;
        private static Vector2 s_upRight = (Vector2.up + Vector2.right).normalized;
        private static Vector2 s_upLeft = (Vector2.up + Vector2.left).normalized;
        private static Vector2 s_downLeft = (Vector2.down + Vector2.left).normalized;
        private static Vector2 s_downRight = (Vector2.down + Vector2.right).normalized;

        private bool _rotating = false;
        private int _angle = 0;
        private Vector2 _direction = Vector2.zero;

        // INSPECTOR FIELDS
        [Header("Rotation Behavior")]
        public Transform TransformToRotate;
        public RotationDirection RotationDirection;
        public float RotationDuration = 0.25f;
        [Tooltip("Defines the ease-in/ease-out behavior of rotation.  X and Y axes must both go from 0 to 1.")]
        public AnimationCurve RotationCurve;

        [Header("Allowed Directions")]
        [Tooltip("Can this object be rotated to 0° counter-clockwise?")]
        public bool CanBe0 = true;
        [Tooltip("Can this object be rotated to 90° counter-clockwise?")]
        public bool CanBe90 = true;
        [Tooltip("Can this object be rotated to 180° counter-clockwise?")]
        public bool CanBe180 = true;
        [Tooltip("Can this object be rotated to 270° counter-clockwise?")]
        public bool CanBe270 = true;

        // EVENT HANDLERS
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Awake() {
            // Assert that at least ONE rotation is allowed
            bool someRotAllowed = CanBe0 || CanBe90 || CanBe180 || CanBe270;
            Assert.IsTrue(someRotAllowed, $"A {nameof(ConveyorBelt)} must have at least one allowed rotation!");
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Start() {
            _angle = ((int)TransformToRotate.localRotation.eulerAngles.z + 360) % 360;
            Debug.Log($"{TransformToRotate.name} has initial angle of {_angle}°");
            _direction = getDirectionFrom(_angle);
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnDrawGizmos() {
            float offset = 0.1f;
            float len = 0.65f;
            Gizmos.color = Color.black;
            if (CanBe0) {
                Vector2 root = (Vector2)transform.position + offset * s_upRight;
                Gizmos.DrawLine(root, root + len * Vector2.right);
            }
            if (CanBe90) {
                Vector2 root = (Vector2)transform.position + offset * s_upLeft;
                Gizmos.DrawLine(root, root + len * Vector2.up);
            }
            if (CanBe180) {
                Vector2 root = (Vector2)transform.position + offset * s_downLeft;
                Gizmos.DrawLine(root, root + len * Vector2.left);
            }
            if (CanBe270) {
                Vector2 root = (Vector2)transform.position + offset * s_downRight;
                Gizmos.DrawLine(root, root + len * Vector2.down);
            }
        }

        // API INTERFACE
        public void Rotate() {
            if (!_rotating) {
                // Get the next rotation
                int oldAngle = _angle;
                int nextAngle = nextAngleFrom(oldAngle);
                _angle = (nextAngle + 360) % 360;
                _direction = getDirectionFrom(_angle);

                // Do the rotation, unless it would just be a complete circle
                int delta = nextAngle - oldAngle;
                int mod = (delta + 360) % 360;
                if (mod != 0) {
                    Debug.Log($"Rotating {delta}° from {oldAngle} to {_angle}");
                    StartCoroutine(doRotation(oldAngle, delta));
                }
            }
        }
        public Vector2 Direction => _direction;

        // HELPERS
        private Vector2 getDirectionFrom(int angle) => Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        private int nextAngleFrom(int oldAngle) {
            bool good = false;

            int angle = oldAngle;
            do {
                angle += ROT_INCREMENT * ((RotationDirection == RotationDirection.Clockwise) ? -1 : 1);
                int mod = (angle + 360) % 360;
                good = (
                    (mod == 0 && CanBe0) ||
                    (mod == 90 && CanBe90) ||
                    (mod == 180 && CanBe180) ||
                    (mod == 270 && CanBe270) ||
                    (mod == 360 && CanBe0));
            } while (!good);

            return angle;
        }
        private IEnumerator doRotation(int oldAngle, int angleToRotate) {
            _rotating = true;

            // Rotate towards that direction each frame
            float t = 0f;
            float lastAngle = 0f;
            float start = Time.time;
            float currAngle, delta;
            while (t < 1f) {
                t = (Time.time - start) / RotationDuration;
                currAngle = angleToRotate * RotationCurve.Evaluate(t);
                delta = (currAngle - lastAngle);
                TransformToRotate.Rotate(Vector3.forward, currAngle - lastAngle);
                lastAngle = currAngle;
                yield return null;
            }

            _rotating = false;
        }

    }

}
