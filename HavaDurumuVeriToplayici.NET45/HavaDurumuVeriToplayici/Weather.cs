using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TSN.HavaDurumuVeriToplayici
{
    [Serializable] internal sealed class Weather : ISerializable, IEquatable<Weather>
    {
        public Weather(ForecastStation station, DateTime dateTime, sbyte temperatureCelcius, sbyte relativeTemperatureCelcius, ushort? windSpeedKmph, ushort? windDirection360, byte relativeHumidityPercent, sbyte dewPointCelcius, double atmosphericPressureMb)
        {
            ValidateParameters(station, dateTime, temperatureCelcius, relativeTemperatureCelcius, windSpeedKmph, windDirection360, relativeHumidityPercent, dewPointCelcius, atmosphericPressureMb);
            _station = station;
            _dateTime = dateTime;
            _temperatureCelcius = temperatureCelcius;
            _relativeTemperatureCelcius = relativeTemperatureCelcius;
            _windSpeedKmph = windSpeedKmph;
            _windDirection360 = windDirection360;
            _relativeHumidityPercent = relativeHumidityPercent;
            _dewPointCelcius = dewPointCelcius;
            _atmosphericPressureMb = atmosphericPressureMb;
        }
        private Weather(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            var station = (ForecastStation)info.GetValue(nameof(Station), typeof(ForecastStation));
            var dateTime = info.GetDateTime(nameof(DateTime));
            var temperatureCelcius = info.GetSByte(nameof(TemperatureCelcius));
            var relativeTemperatureCelcius = info.GetSByte(nameof(RelativeTemperatureCelcius));
            var windSpeedKmph = (ushort?)info.GetValue(nameof(WindSpeedKmph), typeof(ushort?));
            var windDirection360 = (ushort?)info.GetValue(nameof(WindDirection360), typeof(ushort?));
            var relativeHumidityPercent = info.GetByte(nameof(RelativeHumidityPercent));
            var dewPointCelcius = info.GetSByte(nameof(DewPointCelcius));
            var atmosphericPressureMb = info.GetDouble(nameof(AtmosphericPressureMb));
            ValidateParameters(station, dateTime, temperatureCelcius, relativeTemperatureCelcius, windSpeedKmph, windDirection360, relativeHumidityPercent, dewPointCelcius, atmosphericPressureMb);
            _station = station;
            _dateTime = dateTime;
            _temperatureCelcius = temperatureCelcius;
            _relativeTemperatureCelcius = relativeTemperatureCelcius;
            _windSpeedKmph = windSpeedKmph;
            _windDirection360 = windDirection360;
            _relativeHumidityPercent = relativeHumidityPercent;
            _dewPointCelcius = dewPointCelcius;
            _atmosphericPressureMb = atmosphericPressureMb;
        }


        private readonly ForecastStation _station;
        private readonly DateTime _dateTime;
        private readonly sbyte _temperatureCelcius;
        private readonly sbyte _relativeTemperatureCelcius;
        private readonly ushort? _windSpeedKmph;
        private readonly ushort? _windDirection360;
        private readonly byte _relativeHumidityPercent;
        private readonly sbyte _dewPointCelcius;
        private readonly double _atmosphericPressureMb;

        internal ForecastStation Station => _station;
        public DateTime DateTime => _dateTime;
        public sbyte TemperatureCelcius => _temperatureCelcius;
        public sbyte RelativeTemperatureCelcius => _relativeTemperatureCelcius;
        public ushort? WindSpeedKmph => _windSpeedKmph;
        public ushort? WindDirection360 => _windDirection360;
        public byte RelativeHumidityPercent => _relativeHumidityPercent;
        public sbyte DewPointCelcius => _dewPointCelcius;
        public double AtmosphericPressureMb => _atmosphericPressureMb;



        private static void ValidateParameters(ForecastStation station, DateTime dateTime, sbyte temperatureCelcius, sbyte relativeTemperatureCelcius, ushort? windSpeedKmph, ushort? windDirection360, byte relativeHumidityPercent, sbyte dewPointCelcius, double atmosphericPressureMb)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));
            if (windDirection360.HasValue && (!windSpeedKmph.HasValue || windDirection360 < 0 || windDirection360 > 360))
                throw new ArgumentOutOfRangeException(nameof(windDirection360));
            if (atmosphericPressureMb < 0)
                throw new ArgumentOutOfRangeException(nameof(atmosphericPressureMb));
            if (relativeHumidityPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(relativeHumidityPercent));
        }
        
        public override string ToString() => $"{_dateTime:yyyy-MM-dd HH:mm} | {_temperatureCelcius} degree Celcius";
        public override int GetHashCode() => _dateTime.GetHashCode();
        public override bool Equals(object obj) => Equals(obj as Weather);

        public bool Equals(Weather other) => other != null && _station.Equals(other._station) && _dateTime == other._dateTime && _temperatureCelcius == other._temperatureCelcius && _relativeTemperatureCelcius == other._relativeTemperatureCelcius && _windSpeedKmph.Equals(other._windSpeedKmph) && _windDirection360.Equals(other.WindDirection360) && _relativeHumidityPercent == other._relativeHumidityPercent && _dewPointCelcius == other._dewPointCelcius && _atmosphericPressureMb == other._atmosphericPressureMb;
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.AddValue(nameof(Station), _station, typeof(ForecastStation));
            info.AddValue(nameof(DateTime), _dateTime);
            info.AddValue(nameof(TemperatureCelcius), _temperatureCelcius);
            info.AddValue(nameof(RelativeTemperatureCelcius), _relativeTemperatureCelcius);
            info.AddValue(nameof(WindSpeedKmph), _windSpeedKmph, typeof(ushort?));
            info.AddValue(nameof(WindDirection360), _windDirection360, typeof(ushort?));
            info.AddValue(nameof(RelativeHumidityPercent), _relativeHumidityPercent);
            info.AddValue(nameof(DewPointCelcius), _dewPointCelcius);
            info.AddValue(nameof(AtmosphericPressureMb), _atmosphericPressureMb);
        }
    }
}