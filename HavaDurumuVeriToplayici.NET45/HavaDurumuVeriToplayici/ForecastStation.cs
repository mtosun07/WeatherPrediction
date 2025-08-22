using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TSN.HavaDurumuVeriToplayici
{
    [Serializable] internal sealed class ForecastStation : ISerializable, IEquatable<ForecastStation>
    {
        public ForecastStation(double latitude, double longitude, double altitudeMeters, string name, string region)
        {
            string name_ = name, region_ = region;
            ValidateParameters(latitude, longitude, altitudeMeters, ref name_, ref region_);
            _latitude = latitude;
            _longitude = longitude;
            _altitudeMeters = altitudeMeters;
            _name = name_;
            _region = region_;
        }
        private ForecastStation(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            var latitude = info.GetDouble(nameof(Latitude));
            var longitude = info.GetDouble(nameof(Longitude));
            var altitudeMeters = info.GetDouble(nameof(AltitudeMeters));
            var name = info.GetString(nameof(Name));
            var region = info.GetString(nameof(Region));
            ValidateParameters(latitude, longitude, altitudeMeters, ref name, ref region);
            _latitude = latitude;
            _longitude = longitude;
            _altitudeMeters = altitudeMeters;
            _name = name;
            _region = region;
        }


        private readonly double _latitude;
        private readonly double _longitude;
        private readonly double _altitudeMeters;
        private readonly string _name;
        private readonly string _region;

        public double Latitude => _latitude;
        public double Longitude => _longitude;
        public double AltitudeMeters => _altitudeMeters;
        public string Name => _name;
        public string Region => _region;


        private static void ValidateParameters(double latitude, double longitude, double altitudeMeters, ref string name, ref string region)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.Empty.Equals(name))
                throw new ArgumentOutOfRangeException(nameof(name));
            if (region == null)
                throw new ArgumentNullException(nameof(region));
            if (string.Empty.Equals(region))
                throw new ArgumentOutOfRangeException(nameof(region));
            name = name.MakePascalInvariantWithSpace();
            region = region.MakePascalInvariantWithSpace();
        }

        public override string ToString() => $"{_name} ({_region})";
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _latitude.GetHashCode();
                hash = hash * 23 + _longitude.GetHashCode();
                hash = hash * 23 + _altitudeMeters.GetHashCode();
                return hash;
            }
        }
        public override bool Equals(object obj) => Equals(obj as ForecastStation);


        public bool Equals(ForecastStation other) => other != null && _latitude == other._latitude && _longitude == other._longitude && _altitudeMeters == other._altitudeMeters;
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.AddValue(nameof(Latitude), _latitude);
            info.AddValue(nameof(Longitude), _longitude);
            info.AddValue(nameof(AltitudeMeters), _altitudeMeters);
            info.AddValue(nameof(Name), _name, typeof(string));
            info.AddValue(nameof(Region), _region, typeof(string));
        }
    }
}