using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OB.Reservation.BL.Contracts
{
    /// <summary>
    /// Base class for Data classes that should respond to Property Changes.
    /// Implements INotifyPropertyChanged
    /// </summary>
    public abstract class DataContractBase : ContractBase, INotifyPropertyChanged
    {
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}