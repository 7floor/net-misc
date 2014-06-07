using System;
using System.Reflection;
using System.ServiceProcess;

namespace SharpAquosControlService
{
    partial class Service
    {
        Delegate _forwardCallback;

        private void ApplyPoweEventPatch()
        {
            // Find the initialisation routine - may break in later versions
            MethodInfo init = typeof(ServiceBase).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

            // Call it to set up all members
            init.Invoke(this, new object[] { false });

            // Find the service callback handler
            FieldInfo handlerEx = typeof(ServiceBase).GetField("commandCallbackEx", BindingFlags.NonPublic | BindingFlags.Instance);

            if (null != handlerEx)
            {
                // Read the base class provided handler
                _forwardCallback = (Delegate) handlerEx.GetValue(this);

                // Create a new delegate to our handler
                Delegate patch = Delegate.CreateDelegate(_forwardCallback.GetType(), this, "ServiceCallbackEx");

                // Install our handler
                handlerEx.SetValue(this, patch);
            }
        }

// ReSharper disable UnusedMember.Local
        private int ServiceCallbackEx(int command, int eventType, IntPtr eventData, IntPtr eventContext)
// ReSharper restore UnusedMember.Local
        {
            // Call the base class implementation which is fine for all but power and session management 
            if (13 != command)
                return (int)_forwardCallback.DynamicInvoke(command, eventType, eventData, eventContext);

            // Process and forward success code
            if (OnPowerEvent((PowerBroadcastStatus) eventType)) return 0;

            // Abort power operation
            return 0x424d5144;
        }
    }
}
