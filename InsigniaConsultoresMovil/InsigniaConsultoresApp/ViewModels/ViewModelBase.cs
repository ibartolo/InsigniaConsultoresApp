using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Base para los ViewModels: implementación mínima de <see cref="INotifyPropertyChanged"/>.
    /// Se evita CommunityToolkit.Mvvm por tener solo dos ViewModels (ver Decisión 13 del diseño).
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Establece el valor de un campo y notifica solo si realmente cambió.
        /// Devuelve <c>true</c> si el valor cambió.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
