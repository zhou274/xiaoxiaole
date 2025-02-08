﻿using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ButtonAttribute : DrawerAttribute
    {
        private string text;
        public string Text => text;

        private object[] methodParams;
        public object[] Params => methodParams;

        private string visibilityConditionName;
        public string VisibilityConditionName => visibilityConditionName;

        private ButtonVisibility visibilityOption;
        public ButtonVisibility VisibilityOption => visibilityOption;
        
        public ButtonAttribute()
        {
            text = null;
            methodParams = null;

            visibilityConditionName = null;
        }

        public ButtonAttribute(string text = null, params object[] methodParams)
        {
            this.text = text;
            this.methodParams = methodParams;

            visibilityConditionName = null;
        }

        public ButtonAttribute(string text = null, string visibilityMethodName = "", ButtonVisibility visibilityOption = ButtonVisibility.ShowIf, params object[] methodParams)
        {
            this.text = text;
            this.visibilityConditionName = visibilityMethodName;
            this.visibilityOption = visibilityOption;
            this.methodParams = methodParams;
        }
    }
}
