using UnityEngine;
using UnityEngine.UI;

namespace CUAchievements.UI
{
    internal class LayoutIgnorer : MonoBehaviour, ILayoutIgnorer
    {
        public bool ignoreLayout => true;
    }
}
