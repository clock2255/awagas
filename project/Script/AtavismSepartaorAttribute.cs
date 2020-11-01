using UnityEngine;


namespace Atavism
{
    public class AtavismSeparatorAttribute : PropertyAttribute
    {
        public readonly string title;


        public AtavismSeparatorAttribute()
        {
            this.title = "";
        }

        public AtavismSeparatorAttribute(string _title)
        {
            this.title = _title;
        }
    }
}