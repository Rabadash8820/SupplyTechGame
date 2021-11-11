using UnityEngine;
using UnityEngine.Events;

namespace SupplyTechGame {

    [RequireComponent(typeof(Collider2D))]
    public class ItemBin : MonoBehaviour {

        // INSPECTOR FIELDS
        public ItemType Type;
        public UnityEvent ItemCollected = new UnityEvent();

        private void OnTriggerEnter2D(Collider2D other) {
            ConveyorItem item = other.GetComponent<ConveyorItem>();
            if (item != null) {
                Destroy(item.Root.gameObject);
                bool correctType = (item.Type == Type);
                Debug.Log($"{nameof(ItemBin)} {name} received a {(correctType ? "correct" : "wrong")} item in frame {Time.frameCount}!");
                if (correctType)
                    ItemCollected.Invoke();
            }
        }

    }

}