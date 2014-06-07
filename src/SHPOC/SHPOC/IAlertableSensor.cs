using System;

namespace SHPOC
{
    public interface IAlertableSensor : ISensor
    {
         event EventHandler Changed;
    }
}