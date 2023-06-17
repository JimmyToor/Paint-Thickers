using UnityEngine;

namespace Src.Scripts.Attributes
{
    public class ShowIfAttribute : PropertyAttribute
    {
        public string Condition { get; private set; }

        public ShowIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}