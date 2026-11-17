using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaConsultoresApp.Models.Responses;
using InsigniaConsultoresApp.Services.Api;
using InsigniaConsultoresApp.Services.Storage;
using Microsoft.Maui.Controls;

namespace InsigniaConsultoresApp.ViewModels
{
    /// <summary>
    /// Estado de la pantalla Home (post-autenticación): usuario de la sesión, cierre de
    /// sesión y sección de indicadores (KPIs) con selector de rango de fechas.
    /// Hereda de <see cref="CampaignViewModelBase"/> el estado de carga/error y el manejo
    /// del 401 (limpiar sesión + //login una sola vez).
    /// </summary>
    public sealed class HomeViewModel : CampaignViewModelBase
    {
        private static readonly CultureInfo EsMx = CultureInfo.GetCultureInfo("es-MX");

        private readonly IReportsService _reportsService;
        private readonly ITokenStore _tokenStore;

        private string _username = "";
        private DateTime _desde;
        private DateTime _hasta;
        private IndicadoresPeriodo? _indicadores;
        private bool _isRefreshing;
        private string _periodoTexto = "";
        private bool _loaded;

        public HomeViewModel(IReportsService reportsService, ITokenStore tokenStore)
            : base(tokenStore)
        {
            _reportsService = reportsService;
            _tokenStore = tokenStore;

            // Defaults del rango: primer día del mes actual hasta hoy.
            var today = DateTime.Today;
            _desde = new DateTime(today.Year, today.Month, 1);
            _hasta = today;
            _periodoTexto = BuildPeriodoTexto(_desde, _hasta);

            LoadCommand = new Command(async () => await LoadAsync());
            RefreshCommand = new Command(async () => await RefreshAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        /// <summary>Nombre de usuario autenticado (para el header del flyout).</summary>
        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        /// <summary>Fecha inicial del rango (Desde).</summary>
        public DateTime Desde
        {
            get => _desde;
            set
            {
                var fecha = value.Date;
                if (EsMismaFecha(_desde, fecha))
                    return;

                _desde = fecha;
                OnPropertyChanged();
                _ = OnRangoCambiadoAsync();
            }
        }

        /// <summary>Fecha final del rango (Hasta).</summary>
        public DateTime Hasta
        {
            get => _hasta;
            set
            {
                var fecha = value.Date;
                if (EsMismaFecha(_hasta, fecha))
                    return;

                _hasta = fecha;
                OnPropertyChanged();
                _ = OnRangoCambiadoAsync();
            }
        }

        /// <summary>Hoy (límite superior del selector "Hasta").</summary>
        public DateTime Hoy => DateTime.Today;

        /// <summary>Indicadores del periodo cargado (null hasta la primera respuesta).</summary>
        public IndicadoresPeriodo? Indicadores
        {
            get => _indicadores;
            private set
            {
                if (SetProperty(ref _indicadores, value))
                {
                    OnPropertyChanged(nameof(ClientesNuevosTexto));
                    OnPropertyChanged(nameof(PolizasNuevasTexto));
                    OnPropertyChanged(nameof(PolizasRenovadasTexto));
                    OnPropertyChanged(nameof(PolizasVencidasTexto));
                    OnPropertyChanged(nameof(PolizasCanceladasTexto));
                }
            }
        }

        /// <summary>True mientras se ejecuta el pull-to-refresh (RefreshView).</summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set => SetProperty(ref _isRefreshing, value);
        }

        /// <summary>Etiqueta "Del {inicio} al {fin}" (fechas devueltas por la API o el rango seleccionado).</summary>
        public string PeriodoTexto
        {
            get => _periodoTexto;
            private set => SetProperty(ref _periodoTexto, value);
        }

        public ICommand LoadCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand LogoutCommand { get; }

        // Valores formateados es-MX (separador de miles) para las tarjetas KPI.
        public string ClientesNuevosTexto => Formatear(Indicadores?.ClientesNuevos);

        public string PolizasNuevasTexto => Formatear(Indicadores?.PolizasNuevas);

        public string PolizasRenovadasTexto => Formatear(Indicadores?.PolizasRenovadas);

        public string PolizasVencidasTexto => Formatear(Indicadores?.PolizasVencidas);

        public string PolizasCanceladasTexto => Formatear(Indicadores?.PolizasCanceladas);

        /// <summary>
        /// Carga el nombre de usuario y los indicadores del rango seleccionado.
        /// Invocado una sola vez desde el code-behind al aparecer la página; también
        /// lo invocan LoadCommand (reintentar), RefreshCommand y el cambio de fechas.
        /// </summary>
        public async Task LoadAsync()
        {
            // Nombre de usuario para el flyout (se conserva el comportamiento previo).
            var session = await _tokenStore.GetSessionAsync();
            Username = session?.Username ?? "";

            await RunAsync(async () =>
            {
                // Validación del rango antes de llamar a la API.
                if (EsMayor(Desde, Hasta))
                {
                    ErrorMessage = "La fecha inicial no puede ser mayor que la final";
                    return;
                }

                if (EsMayor(Hasta, DateTime.Today))
                {
                    ErrorMessage = "La fecha final no puede ser futura";
                    return;
                }

                var result = await _reportsService.GetIndicadoresAsync(Desde, Hasta);
                await HandleUnauthorizedAsync(result.Kind);

                if (!result.IsSuccess || result.Value is null)
                {
                    ErrorMessage = result.Message;
                    return;
                }

                Indicadores = result.Value;
                PeriodoTexto = BuildPeriodoTexto(result.Value.FechaInicio, result.Value.FechaFin);
            });

            // A partir de aquí el cambio de fechas recarga; antes no (carga inicial en curso).
            _loaded = true;
        }

        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task OnRangoCambiadoAsync()
        {
            // Evita recargar durante la carga inicial o en medio de otra carga.
            if (!_loaded || IsBusy)
                return;

            await LoadAsync();
        }

        private async Task LogoutAsync()
        {
            await _tokenStore.ClearAsync();

            // Navegación absoluta a login: limpia el stack y el flyout.
            await Shell.Current.GoToAsync("//login");
        }

        /// <summary>
        /// Construye "Del {dd/MM/yyyy} al {dd/MM/yyyy}" usando las fechas devueltas por
        /// la API; si vienen vacías (default), cae al rango seleccionado.
        /// </summary>
        private string BuildPeriodoTexto(DateTime inicio, DateTime fin)
        {
            var desde = inicio == default ? Desde : inicio;
            var hasta = fin == default ? Hasta : fin;
            return $"Del {FormatearFecha(desde)} al {FormatearFecha(hasta)}";
        }

        private static string FormatearFecha(DateTime fecha)
            => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        private static string Formatear(int? valor)
            => (valor ?? 0).ToString("N0", EsMx);

        /// <summary>Clave YYYYMMDD para comparar fechas ignorando hora y <see cref="DateTime.Kind"/>.</summary>
        private static int ClaveFecha(DateTime fecha)
            => (fecha.Year * 10000) + (fecha.Month * 100) + fecha.Day;

        private static bool EsMismaFecha(DateTime a, DateTime b)
            => ClaveFecha(a) == ClaveFecha(b);

        private static bool EsMayor(DateTime a, DateTime b)
            => ClaveFecha(a) > ClaveFecha(b);
    }
}
