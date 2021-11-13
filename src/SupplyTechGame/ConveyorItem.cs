using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace SupplyTechGame {

    public enum ItemType {
        Nut,
        Bolt,
        Washer,
    }

    [RequireComponent(typeof(Collider2D))]
    public class ConveyorItem : MonoBehaviour {

        // HIDDEN FIELDS
        private Vector3? _target = null;
        private Vector2 _nextDir = Vector2.zero;

        // INSPECTOR FIELDS
        public ItemType Type;
        [Tooltip("This is the Transform that will be moved along the conveyor.")]
        public Transform Root;
        public float Speed = 1f;
        public Vector2 Direction = Vector2.zero;
        [Tooltip("If the item's offset from the target is less than or equal to this value, then it has 'arrived'.")]
        public float ArrivedOffset = 0.05f;

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Update() {
            Root.Translate(Speed * Time.deltaTime * Direction);
            if (_target.HasValue) {
                float newOffsetSq = Vector3.SqrMagnitude(_target.Value - Root.position);
                if (newOffsetSq <= ArrivedOffset * ArrivedOffset)
                    arrive();
            }
        }

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void OnTriggerEnter2D(Collider2D other) {
            ConveyorBelt cb = other.GetComponent<ConveyorBelt>();
            if (cb != null) {
                _target = cb.transform.position;
                _nextDir = cb.Direction;
            }
        }

        private void arrive() {
            Root.position = _target.Value;
            Direction = _nextDir;
            _target = null;
        }

    }

}
