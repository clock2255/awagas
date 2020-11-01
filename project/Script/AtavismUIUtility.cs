using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Atavism
{
    public static class AtavismUIUtility {

        public static void BringToFront(GameObject obj) {
            Canvas canvas = AtavismUIUtility.FindInParents<Canvas>(obj);
            if (canvas != null)
                obj.transform.SetParent(canvas.transform, true);
            obj.transform.SetAsLastSibling();
        }

        public static T FindInParents<T>(GameObject obj) where T : Component {
            if (obj == null)
                return null;
            var comp = obj.GetComponent<T>();
            if (comp != null)
                return comp;
            Transform t = obj.transform.parent;
            while (t != null && comp == null) {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }
            return comp;
        }
    }
}