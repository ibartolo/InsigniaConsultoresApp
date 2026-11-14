using System;
using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Validations;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Estado de una fila de destinatario. Envuelve el DTO, expone el estado para la UI
    /// y delega las acciones Enviar/Deshacer a los callbacks del ViewModel padre.
    /// </summary>
    public sealed class RecipientItemViewModel : ViewModelBase
    {
        private readonly CampanaWhatsAppDestinatario _dto;
        private readonly Func<RecipientItemViewModel, Task> _onSend;
        private readonly Func<RecipientItemViewModel, Task> _onUndo;

        private bool _isSent;
        private bool _isBusy;
        private DateTime? _fechaEnvio;

        public RecipientItemViewModel(
            CampanaWhatsAppDestinatario dto,
            Func<RecipientItemViewModel, Task> onSend,
            Func<RecipientItemViewModel, Task> onUndo)
        {
            _dto = dto;
            _onSend = onSend;
            _onUndo = onUndo;

            // Estado inicial desde el DTO (Code 1 == Enviado).
            _isSent = dto.Estado.Code == 1;
            _fechaEnvio = dto.FechaEnvio;

            SendCommand = new Command(async () => await SendAsync(), () => CanSend);
            UndoCommand = new Command(async () => await UndoAsync());
        }

        public int Id => _dto.Id;

        public string Nombre => _dto.Nombre ?? string.Empty;

        public string Telefono => _dto.Telefono ?? string.Empty;

        /// <summary>Etiqueta del estado actual en español (Pendiente/Enviado).</summary>
        public string EstadoTexto => IsSent ? "Enviado" : "Pendiente";

        /// <summary>Fecha de envío formateada, o cadena vacía si aún está pendiente.</summary>
        public string FechaEnvioTexto => _fechaEnvio?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;

        /// <summary>Nota mostrada cuando el teléfono no es enviable.</summary>
        public string InvalidPhoneHint => "Teléfono inválido (se requieren 10 dígitos)";

        /// <summary>Teléfono normalizado a 10 dígitos, o null si no es enviable.</summary>
        public string? TenDigits =>
            PhoneHelper.TryNormalizeTo10Digits(Telefono, out var ten) ? ten : null;

        /// <summary>True cuando el teléfono no puede normalizarse a 10 dígitos.</summary>
        public bool HasInvalidPhone => TenDigits is null;

        /// <summary>True cuando la fila puede enviarse (teléfono válido y sin operación en vuelo).</summary>
        public bool CanSend => TenDigits is not null && !IsBusy;

        public bool IsSent
        {
            get => _isSent;
            private set
            {
                if (SetProperty(ref _isSent, value))
                {
                    OnPropertyChanged(nameof(EstadoTexto));
                    OnPropertyChanged(nameof(FechaEnvioTexto));
                }
            }
        }

        /// <summary>True mientras se ejecuta Enviar o Deshacer (protege el doble toque).</summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    OnPropertyChanged(nameof(CanSend));
                    ((Command)SendCommand).ChangeCanExecute();
                }
            }
        }

        public ICommand SendCommand { get; }

        public ICommand UndoCommand { get; }

        /// <summary>Marca la fila como enviada con la fecha dada (actualización optimista).</summary>
        public void ApplySent(DateTime when)
        {
            _fechaEnvio = when;
            IsSent = true;
        }

        /// <summary>Vuelve la fila a estado pendiente y limpia la fecha de envío.</summary>
        public void ApplyPending()
        {
            _fechaEnvio = null;
            IsSent = false;
        }

        private async Task SendAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await _onSend(this);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UndoAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await _onUndo(this);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
