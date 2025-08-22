using System;
using System.Collections.Generic;

namespace TSN.HavaDurumuVeriToplayici
{
    internal sealed class WeatherDateTimeEqualityComparer : IEqualityComparer<Weather>
    {
        static WeatherDateTimeEqualityComparer() => _default = new Lazy<WeatherDateTimeEqualityComparer>(() => new WeatherDateTimeEqualityComparer());
        private WeatherDateTimeEqualityComparer() { }


        private static readonly Lazy<WeatherDateTimeEqualityComparer> _default;

        public static WeatherDateTimeEqualityComparer Default => _default.Value;



        public bool Equals(Weather x, Weather y) => ReferenceEquals(x, y) || (x != null && y != null && x.DateTime.Equals(y.DateTime));
        public int GetHashCode(Weather obj) => obj?.DateTime.GetHashCode() ?? 0;
    }
    internal sealed class WeatherInternalEqualityComparer : IEqualityComparer<Weather>
    {
        static WeatherInternalEqualityComparer() => _default = new Lazy<WeatherInternalEqualityComparer>(() => new WeatherInternalEqualityComparer());
        private WeatherInternalEqualityComparer() { }


        private static readonly Lazy<WeatherInternalEqualityComparer> _default;

        public static WeatherInternalEqualityComparer Default => _default.Value;



        public bool Equals(Weather x, Weather y) => ReferenceEquals(x, y) || (x != null && y != null && x.TemperatureCelcius == y.TemperatureCelcius && x.WindSpeedKmph == y.WindSpeedKmph && x.WindDirection360 == y.WindDirection360 && x.RelativeHumidityPercent == y.RelativeHumidityPercent && x.DewPointCelcius == y.DewPointCelcius && x.AtmosphericPressureMb == y.AtmosphericPressureMb);
        public int GetHashCode(Weather obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                int hash = 17;
                hash = 23 * hash + obj.TemperatureCelcius.GetHashCode();
                hash = 23 * hash + obj.WindSpeedKmph.GetHashCode();
                hash = 23 * hash + obj.WindDirection360.GetHashCode();
                hash = 23 * hash + obj.RelativeHumidityPercent.GetHashCode();
                hash = 23 * hash + obj.DewPointCelcius.GetHashCode();
                hash = 23 * hash + obj.AtmosphericPressureMb.GetHashCode();
                return hash;
            }
        }
    }
}