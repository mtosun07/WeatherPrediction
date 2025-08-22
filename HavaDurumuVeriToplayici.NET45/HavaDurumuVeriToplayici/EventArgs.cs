using System;

namespace TSN.HavaDurumuVeriToplayici
{
    internal class FetchedWeatherDataEventArgs : EventArgs
    {
        public FetchedWeatherDataEventArgs(DateTime day) => _day = day;


        private readonly DateTime _day;

        public DateTime Day => _day;
    }
}