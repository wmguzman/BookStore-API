using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crafyc_UI.Static
{
    public static class Endpoints
    {
        public static string BaseUrl = "https://localhost:44395/";
        public static string AseguradorasEndpoint = $"{BaseUrl}api/aseguradoras/";
        public static string CarteraEndpoint = $"{BaseUrl}api/cartera/";
        public static string CiudadesEndpoint = $"{BaseUrl}api/ciudades/";
        public static string ClientesEndpoint = $"{BaseUrl}api/clientes/";
        public static string CronogramasEndpoint = $"{BaseUrl}api/cronogramas/";
        public static string EstadosDeCreditoEndpoint = $"{BaseUrl}api/estadosdecredito/";
        public static string IntermediariosEndpoint = $"{BaseUrl}api/intermediarios/";
        public static string MovimientosEndpoint = $"{BaseUrl}api/movimientos/";
        public static string PolizasEndpoint = $"{BaseUrl}api/polizas/";
        public static string TiposDeCreditoEndpoint = $"{BaseUrl}api/tiposdecredito/";
        public static string TiposdeIdentificacionEndpoint = $"{BaseUrl}api/tiposdeidentificacion/";
        public static string TiposDeMovimientoEndpoint = $"{BaseUrl}api/tiposdemovimiento/";
        public static string VariablesGlobalesEndpoint = $"{BaseUrl}api/variablesglobales/";
        public static string UsersEndpoint = $"{BaseUrl}api/users/";
        public static string RegisterEndpoint = $"{BaseUrl}api/users/register/";
        public static string LoginEndpoint = $"{BaseUrl}api/users/login/";
    }
}
