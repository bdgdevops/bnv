// Services/MarketData.cs
using BVN.WinForms.Models;

namespace BVN.WinForms.Services;

public static class MarketData
{
    public static List<StockModel> GetStocks() =>
    [
        new() { Ticker="AMXL",  Name="América Móvil",          Icon="📱", ColorHex="#4F9CF9", Sector="Telecomunicaciones", Price=14.85, Open=14.85, Volatility=0.010 },
        new() { Ticker="WALMEX",Name="Walmart de México",       Icon="🛒", ColorHex="#34D399", Sector="Retail",            Price=57.30, Open=57.30, Volatility=0.008 },
        new() { Ticker="FEMSA", Name="Fomento Económico Mex.",  Icon="🍺", ColorHex="#F59E0B", Sector="Consumo",           Price=115.40,Open=115.40,Volatility=0.012 },
        new() { Ticker="BIMBO", Name="Grupo Bimbo",             Icon="🍞", ColorHex="#A78BFA", Sector="Alimentos",         Price=84.60, Open=84.60, Volatility=0.009 },
        new() { Ticker="GFNORTE",Name="Gpo. Financiero Banorte",Icon="🏦", ColorHex="#22D3EE", Sector="Financiero",        Price=138.70,Open=138.70,Volatility=0.013 },
        new() { Ticker="CEMEX", Name="CEMEX",                   Icon="🏗", ColorHex="#F87171", Sector="Materiales",        Price=5.92,  Open=5.92,  Volatility=0.018 },
        new() { Ticker="TLEVISA",Name="Televisa",               Icon="📺", ColorHex="#FB923C", Sector="Medios",            Price=11.25, Open=11.25, Volatility=0.016 },
        new() { Ticker="PINFRA",Name="Promotora y Op. Infraestr.",Icon="🛣",ColorHex="#6EE7B7",Sector="Infraestructura",   Price=185.20,Open=185.20,Volatility=0.011 },
        new() { Ticker="ELEKTRA",Name="Grupo Elektra",          Icon="⚡", ColorHex="#FBBF24", Sector="Retail Especializado",Price=764.50,Open=764.50,Volatility=0.022 },
        new() { Ticker="ARCA",  Name="Arca Continental",        Icon="🥤", ColorHex="#60A5FA", Sector="Bebidas",           Price=178.80,Open=178.80,Volatility=0.010 },
        new() { Ticker="TECNO",  Name="Tecnología Avanzada",    Icon="💻", ColorHex="#60A5FA", Sector="Tecnología",        Price=178.80,Open=178.80,Volatility=0.010 },
    ];

    public static List<string> GetHeadlines() =>
    [
        "🌎 AMXL reporta crecimiento de 8% en suscriptores móviles — Q1 2026",
        "📈 WALMEX supera estimaciones: ventas comparables +5.2% en marzo",
        "🏦 Banxico mantiene tasa en 9.0% en reunión de política monetaria",
        "⚡ ELEKTRA anuncia expansión a Colombia y Perú para Q3 2026",
        "🏗️ CEMEX obtiene contrato de infraestructura por $2,300 MDD",
        "🥤 ARCA Continental firma alianza estratégica con PepsiCo para LatAm",
        "🛣️ PINFRA inaugura autopista México-Puebla ampliada con 4 carriles",
        "📱 AMX lanza servicio satelital IoT para zonas rurales — 15 países",
        "🏦 GFNORTE obtiene calificación AAA de S&P por sexto año consecutivo",
        "🍺 FEMSA divulga plan de sostenibilidad 2030 con inversión de $800M",
        "📊 Índice BVN alcanza máximo histórico en sesión de alta volatilidad",
        "🌿 Inversores extranjeros incrementan posición en acciones de BMV",
    ];
}
