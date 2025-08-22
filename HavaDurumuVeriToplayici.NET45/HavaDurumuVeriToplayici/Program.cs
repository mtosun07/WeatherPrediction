using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TSN.HavaDurumuVeriToplayici
{
    internal static class Program
    {
        static Program()
        {
            Utility.FetchedWeatherData += Utility_FetchedWeatherData;
        }


        private const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss:ffff";
        private const double TestDataPercent = 10D;



        [STAThread] private static void Main(string[] args)
        {
            //FetchWeatherData(new DateTime(2020, 5, 1), new DateTime(2020, 5, 28));
            Console.WriteLine();
            var data = Utility.Deserialize<Weather[]>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WeatherData.bin"));
            ConsoleWriteLine("Deserialized Weather Data");
            var filtered = data.Where(x => x.WindSpeedKmph.HasValue).Distinct(WeatherInternalEqualityComparer.Default).Select(x => new { RoundedDateTime = x.DateTime.Round(TimeSpan.FromMinutes(15)), Weather = x }).ToLookup(x => new TimeSpan(x.RoundedDateTime.Hour, x.RoundedDateTime.Minute, 0)).OrderByDescending(x => x.Count()).FirstOrDefault()?.ToLookup(x => x.Weather.Station).OrderByDescending(x => x.Count()).FirstOrDefault()?.ToArray();
            var list = new List<Weather>(filtered.Length);
            ConsoleWriteLine("Filtered Weather Data");
            if (filtered != null)
            {
                Console.WriteLine();
                foreach (var w in filtered)
                {
                    Console.WriteLine(w.Weather);
                    list.Add(w.Weather);
                }
                Console.WriteLine();
                Console.WriteLine("Actual Data Length: {0}", data.Length);
                Console.WriteLine("Filtered Data Length: {0}", filtered.Length);
                Console.WriteLine("Considered Time: {0}", new TimeSpan(filtered[0].RoundedDateTime.Hour, filtered[0].RoundedDateTime.Minute, 0));
                Console.WriteLine("Weather Forecast Station: {0}", filtered[0].Weather.Station);
                Console.WriteLine();
                var testDataLength = (int)Math.Round(list.Count * (TestDataPercent / 100D));
                var testData = new Weather[testDataLength];
                var rnd = new Random();
                try
                {
                    for (int c = 0, i; c < testDataLength; c++)
                    {
                        testData[c] = list[i = rnd.Next(0, list.Count)];
                        list.RemoveAt(i);
                    }
                    Utility.SerializeWeatherDataAsArff(list.ToArray(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"DATASET_WeatherData_training_{list.Count}.arff"));
                    ConsoleWriteLine("Serialized Weather Training Data to .arff file");
                    Utility.SerializeWeatherDataAsArff(testData, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"DATASET_WeatherData_test_{testDataLength}.arff"));
                    ConsoleWriteLine("Serialized Weather Test Data to .arff file");
                }
                catch (Exception ex)
                {
                    ConsoleWriteLine($"An error occured: {ex}");
                }
            }
            else
                Console.WriteLine("No Data");
            Console.ReadLine();
        }

        private static void ConsoleWrite(string value) => Console.Write($"{DateTime.Now.ToString(DateTimeFormat)}\t>> {value}");
        private static void ConsoleWriteLine(string value) => ConsoleWrite(value + Environment.NewLine);
        private static void FetchWeatherData(DateTime first, DateTime last)
        {
            var data = new List<Weather>();
            using (var sw = new StreamWriter(new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WeatherData.csv"), FileMode.Create, FileAccess.Write), Encoding.UTF8))
            {
                try
                {
                    foreach (var w in Utility.FetchWeatherData(first, last))
                    {
                        data.Add(w);
                        sw.WriteLine($"{w.Station.Latitude.ToString(CultureInfo.InvariantCulture)};{w.Station.Longitude.ToString(CultureInfo.InvariantCulture)};{w.Station.AltitudeMeters.ToString(CultureInfo.InvariantCulture)};{w.Station.Name};{w.Station.Region};{w.DateTime.ToString(CultureInfo.InvariantCulture)};{w.TemperatureCelcius};{w.RelativeTemperatureCelcius};{w.WindSpeedKmph?.ToString() ?? string.Empty};{w.WindDirection360?.ToString() ?? string.Empty};{w.RelativeHumidityPercent};{w.DewPointCelcius};{w.AtmosphericPressureMb.ToString(CultureInfo.InvariantCulture)}");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWriteLine($"An error occured: {ex}");
                }
                sw.Flush();
            }
            if ((data?.Count ?? 0) > 0)
            {
                Utility.Serialize(data.ToArray(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WeatherData.bin"));
                ConsoleWriteLine("Serialized Weather Data");
            }
        }

        private static void Utility_FetchedWeatherData(object sender, FetchedWeatherDataEventArgs e) => ConsoleWriteLine($"Fetched Weather Data for {e.Day:yyyy.MM.dd}");
    }
}