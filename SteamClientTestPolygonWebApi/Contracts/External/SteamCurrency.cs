using System.Runtime.Serialization;

namespace SteamClientTestPolygonWebApi.Contracts.External;

public enum SteamCurrency
{
    [EnumMember(Value = "USD")] Usd = 1,

    [EnumMember(Value = "GBP")] Gbp = 2,

    [EnumMember(Value = "EUR")] Eur = 3,

    [EnumMember(Value = "CHF")] Chf = 4,

    [EnumMember(Value = "RUB")] Rub = 5,

    [EnumMember(Value = "PLN")] Pln = 6,

    [EnumMember(Value = "BRL")] Brl = 7,

    [EnumMember(Value = "JPY")] Jpy = 8,

    [EnumMember(Value = "NOK")] Nok = 9,

    [EnumMember(Value = "IDR")] Idr = 10,

    [EnumMember(Value = "MYR")] Myr = 11,

    [EnumMember(Value = "PHP")] Php = 12,

    [EnumMember(Value = "SGD")] Sgd = 13,

    [EnumMember(Value = "THB")] Thb = 14,

    [EnumMember(Value = "VND")] Vnd = 15,

    [EnumMember(Value = "KRW")] Krw = 16,

    [EnumMember(Value = "TRY")] Try = 17,

    [EnumMember(Value = "UAH")] Uah = 18,

    [EnumMember(Value = "MXN")] Mxn = 19,

    [EnumMember(Value = "CAD")] Cad = 20,

    [EnumMember(Value = "AUD")] Aud = 21,

    [EnumMember(Value = "NZD")] Nzd = 22,

    [EnumMember(Value = "CNY")] Cny = 23,

    [EnumMember(Value = "INR")] Inr = 24,

    [EnumMember(Value = "CLP")] Clp = 25,

    [EnumMember(Value = "PEN")] Pen = 26,

    [EnumMember(Value = "COP")] Cop = 27,

    [EnumMember(Value = "ZAR")] Zar = 28,

    [EnumMember(Value = "HKD")] Hkd = 29,

    [EnumMember(Value = "TWD")] Twd = 30,

    [EnumMember(Value = "SAR")] Sar = 31,

    [EnumMember(Value = "AED")] Aed = 32,

    [EnumMember(Value = "ARS")] Ars = 34,

    [EnumMember(Value = "ILS")] Ils = 35,

    [EnumMember(Value = "KZT")] Kzt = 37,

    [EnumMember(Value = "KWD")] Kwd = 38,

    [EnumMember(Value = "QAR")] Qar = 39,

    [EnumMember(Value = "CRC")] Crc = 40,

    [EnumMember(Value = "UYU")] Uyu = 41,
}

// 1 - USD / United States Dollar
// 2 - GBP / United Kingdom Pound
// 3 - EUR / European Union Euro
// 4 - CHF / Swiss Francs
// 5 - RUB / Russian Rouble
// 6 - PLN / Polish Złoty
// 7 - BRL / Brazilian Reals
// 8 - JPY / Japanese Yen
// 9 - NOK / Norwegian Krone
// 10 - IDR / Indonesian Rupiah
// 11 - MYR / Malaysian Ringgit
// 12 - PHP / Philippine Peso
// 13 - SGD / Singapore Dollar
// 14 - THB / Thai Baht
// 15 - VND / Vietnamese Dong
// 16 - KRW / South Korean Won
// 17 - TRY / Turkish Lira
// 18 - UAH / Ukrainian Hryvnia
// 19 - MXN / Mexican Peso
// 20 - CAD / Canadian Dollars
// 21 - AUD / Australian Dollars
// 22 - NZD / New Zealand Dollar
// 23 - CNY / Chinese Renminbi (yuan)
// 24 - INR / Indian Rupee
// 25 - CLP / Chilean Peso
// 26 - PEN / Peruvian Sol
// 27 - COP / Colombian Peso
// 28 - ZAR / South African Rand
// 29 - HKD / Hong Kong Dollar
// 30 - TWD / New Taiwan Dollar
// 31 - SAR / Saudi Riyal
// 32 - AED / United Arab Emirates Dirham
// 34 - ARS / Argentine Peso
// 35 - ILS / Israeli New Shekel
// 37 - KZT / Kazakhstani Tenge
// 38 - KWD / Kuwaiti Dinar
// 39 - QAR / Qatari Riyal
// 40 - CRC / Costa Rican Colón
// 41 - UYU / Uruguayan Peso
//[Description]