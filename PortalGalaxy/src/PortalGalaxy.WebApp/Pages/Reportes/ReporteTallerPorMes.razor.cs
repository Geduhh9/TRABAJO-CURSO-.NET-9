using BlazorBootstrap;
using Microsoft.AspNetCore.Components;

namespace PortalGalaxy.WebApp.Pages.Reportes;

public partial class ReporteTallerPorMes
{
    private BarChart _barChart = null!;
    private ChartData _chartData = null!;
    private BarChartOptions _chartOptions = null!;
    private List<int> ListAnios { get; set; } = new();
    private int _selectedYear;
    private int SelectedYear
    {
        get => _selectedYear;
        set
        {
            _selectedYear = value;
            _ = CargarDatos(_selectedYear);
        }
    }

    protected override void OnInitialized()
    {
        for (var year = DateTime.Now.Year; year >= 2020; year--)
            ListAnios.Add(year);

        SelectedYear = DateTime.Now.Year;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await DataInicialAsync();
            await CargarDatos(SelectedYear); 
        }
    }

    private static readonly string[] MesesOrdenados = new[]
    {
        "Enero","Febrero","Marzo","Abril","Mayo","Junio",
        "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"
    };


    private async Task DataInicialAsync()
    {
        _chartData = new ChartData
        {
            Labels = new List<string>
            {
                "Enero","Febrero","Marzo","Abril","Mayo","Junio",
                "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"
            },
            Datasets = new List<IChartDataset>
            {
                new BarChartDataset
                {
                    Label = "Cantidad de Talleres",
                    Data = Enumerable.Repeat<double?>(0, 12).ToList(),
                    BackgroundColor = new List<string> { "#000455" },
                    CategoryPercentage = 0.9,
                    BarPercentage = 1
                }
            }
        };

        _chartOptions = new BarChartOptions
        {
            Responsive = true
        };

        _chartOptions.Interaction.Mode = InteractionMode.Index;

        if (_chartOptions.Scales.X?.Title is not null)
        {
            _chartOptions.Scales.X.Title.Text = "Mes";
            _chartOptions.Scales.X.Title.Display = true;
        }

        if (_chartOptions.Scales.Y?.Title is not null)
        {
            _chartOptions.Scales.Y.Title.Text = "Cantidad";
            _chartOptions.Scales.Y.Title.Display = true;
        }

        await _barChart.InitializeAsync(_chartData, _chartOptions);
    }

    private async Task CargarDatos(int anio)
    {
        try
        {
            var response = await TallerProxy.ListarPorMesAsync(anio);
            var labels = MesesOrdenados.ToList();
            var valores = Enumerable.Repeat<double?>(0, 12).ToArray();

            if (response.Success && response.Data is not null && response.Data.Any())
            {
                foreach (var r in response.Data)
                {
                    int idx = -1;

                    if (int.TryParse(r.Mes, out var mesNum) && mesNum >= 1 && mesNum <= 12)
                        idx = mesNum - 1;
                    else
                    {
                        idx = Array.FindIndex(MesesOrdenados,
                            m => string.Equals(m, r.Mes, StringComparison.OrdinalIgnoreCase));
                    }

                    if (idx >= 0 && idx < 12)
                        valores[idx] = r.Cantidad;
                }
            }

            _chartData.Labels!.Clear();
            _chartData.Labels.AddRange(labels);

            var firstDataset = (BarChartDataset)_chartData.Datasets![0];
            firstDataset.Data = valores.ToList();

            await _barChart.UpdateAsync(_chartData, _chartOptions);
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
    }

    //private async Task CambiarAnio(ChangeEventArgs? e)
    //{
    //    if (e?.Value is null) return;
    //    SelectedYear = int.Parse(e.Value.ToString()!);
    //    await CargarDatos(SelectedYear); 
    //}
}