using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace TSN.HavaDurumuVeriToplayici
{
    internal static class Utility
    {
        private const string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string UrlFormat = "https://tr.freemeteo.com/weather/eskisehir/history/daily-history/?gid=315202&station=5372&date={0}&language=english&country=turkey";
        private const string UrlDateFormat = "yyyy-MM-dd";
        private const string JavaScriptCode_GetInnerHtml = "return arguments[0].innerHTML;";
        private const string CssSelector_WeatherStation = "#content > div.right-col > div.weather-now > div.tool-block > span.station";
        private const string CssSelector_WeatherTable = "#content > div.right-col > div.weather-now > div.today.table > div.table.hourly > table > tbody";
        private const string WeatherHourFormat = "HH:mm";
        private const string WeatherStationLocation = "Station Location";
        private const string WeatherNoWindText = "Calm";
        private const char Space = ' ';
        private const char Dot = '.';
        private const char Minus = '-';
        private const string Div = "div";
        private const string Span = "span";
        private const string Tr = "tr";
        private const string Td = "td";
        private const string A = "a";
        private const string B = "b";

        private static readonly int _weatherStationLocation_Length = WeatherStationLocation.Length;
        private static readonly string[] _weatherStationLocation_Splitters = new[] { "Lat", "Lon", "Altitude", "m" };

        public static event EventHandler<FetchedWeatherDataEventArgs> FetchedWeatherData;



        public static string MakeInvariant(this string value) => new string(value.Select(c => char.IsWhiteSpace(c) ? Space : c).Select(c => ValidChars.Contains(c) ? c : Space).ToArray()).Trim();
        public static string MakePascalInvariant(this string value) => string.Join(string.Empty, MakeInvariant(value).Split(new[] { Space }, StringSplitOptions.RemoveEmptyEntries).Select(s => char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1).ToLowerInvariant() : string.Empty)).ToArray());
        public static string MakePascalInvariantWithSpace(this string value) => string.Join(Space.ToString(), MakeInvariant(value).Split(new[] { Space }, StringSplitOptions.RemoveEmptyEntries).Select(s => char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1).ToLowerInvariant() : string.Empty)).ToArray());
        public static DateTime Round(this DateTime dt, TimeSpan d)
        {
            DateTime roundedUp = new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind), roundedDown = roundedUp - d;
            return roundedUp - dt <= dt - roundedDown ? roundedUp : roundedDown;
        }
        public static void Serialize<T>(T data, string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                new BinaryFormatter().Serialize(fs, data);
                fs.Flush();
            }
        }
        public static T Deserialize<T>(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                return (T)new BinaryFormatter().Deserialize(fs);
        }
        public static IEnumerable<Weather> FetchWeatherData(DateTime first, DateTime last)
        {
            if (first > last)
                throw new ArgumentOutOfRangeException();
            string s;
            IWebElement spanStation, tbody;
            HtmlDocument doc;
            ForecastStation station;
            using (var driver = new ChromeDriver(new ChromeOptions { PageLoadStrategy = PageLoadStrategy.Normal }))
            {
                var js = (IJavaScriptExecutor)driver;
                driver.Manage().Window.Minimize();
                var navigator = driver.Navigate();
                for (DateTime day = last; day >= first; day = day.AddDays(-1))
                {
                    navigator.GoToUrl(string.Format(UrlFormat, day.ToString(UrlDateFormat)));
                    Thread.Sleep(300);
                    while (true)
                    {
                        try
                        {
                            spanStation = driver.FindElementByCssSelector(CssSelector_WeatherStation);
                            var spanStation_as = spanStation.FindElements(By.TagName(A));
                            var values = (s = spanStation.Text.MakePascalInvariantWithSpace()).Substring(s.IndexOf(WeatherStationLocation) + _weatherStationLocation_Length).Split(_weatherStationLocation_Splitters, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Replace(Space, Dot)).Where(x => !string.Empty.Equals(x)).Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                            station = new ForecastStation(values[0], values[1], values[2], spanStation_as[0].Text, spanStation_as[1].Text);
                            tbody = driver.FindElementByCssSelector(CssSelector_WeatherTable);
                            break;
                        }
                        catch (NoSuchElementException)
                        {
                            Thread.Sleep(300);
                            navigator.Refresh();
                        }
                    }
                    (doc = new HtmlDocument()).LoadHtml((string)js.ExecuteScript(JavaScriptCode_GetInnerHtml, tbody));
                    foreach (var tds in doc.DocumentNode.Elements(Tr).Select(tr => tr.Descendants(Td).ToArray()))
                    {
                        Weather w = null;
                        try
                        {
                            var hour = DateTime.ParseExact(tds[0].InnerText, WeatherHourFormat, CultureInfo.InvariantCulture);
                            var dateTime = new DateTime(day.Year, day.Month, day.Day, hour.Hour, hour.Minute, 0);
                            var temperatureCelcius = sbyte.Parse(new string(tds[1].SelectSingleNode(B).InnerText.TakeWhile(c => char.IsNumber(c) || c == Minus).ToArray()), CultureInfo.InvariantCulture);
                            var relativeTemperatureCelcius = sbyte.Parse(new string(tds[2].InnerText.TakeWhile(c => char.IsNumber(c) || c == Minus).ToArray()), CultureInfo.InvariantCulture);
                            var spansWind = tds[3].Elements(Span)?.ToArray() ?? new HtmlNode[0];
                            ushort? windSpeedKmph = null, windDirection360 = null;
                            if (spansWind.Length == 2)
                            {
                                windDirection360 = ushort.Parse(new string(spansWind[0].SelectSingleNode(Div).InnerText.TakeWhile(c => char.IsNumber(c)).ToArray()), CultureInfo.InvariantCulture);
                                windSpeedKmph = ushort.Parse(new string(spansWind[1].InnerText.TakeWhile(c => char.IsNumber(c)).ToArray()), CultureInfo.InvariantCulture);
                            }
                            else if (!tds[3].InnerText.Equals(WeatherNoWindText))
                                windSpeedKmph = ushort.Parse(new string(tds[3].InnerText.SkipWhile(c => !char.IsNumber(c)).TakeWhile(c => char.IsNumber(c)).ToArray()), CultureInfo.InvariantCulture);
                            var relativeHumidityPercent = byte.Parse(new string(tds[5].InnerText.TakeWhile(c => char.IsNumber(c)).ToArray()), CultureInfo.InvariantCulture);
                            var dewPointCelcius = sbyte.Parse(new string(tds[6].InnerText.TakeWhile(c => char.IsNumber(c) || c == Minus).ToArray()), CultureInfo.InvariantCulture);
                            var atmosphericPressureMb = double.Parse(new string(tds[7].InnerText.TakeWhile(c => char.IsNumber(c) || c == Dot).ToArray()), CultureInfo.InvariantCulture);
                            w = new Weather(station, dateTime, temperatureCelcius, relativeTemperatureCelcius, windSpeedKmph, windDirection360, relativeHumidityPercent, dewPointCelcius, atmosphericPressureMb);
                        }
                        catch (NullReferenceException) { continue; }
                        catch (IndexOutOfRangeException) { continue; }
                        catch (FormatException) { continue; }
                        yield return w;
                    }
                    FetchedWeatherData?.Invoke(null, new FetchedWeatherDataEventArgs(day));
                }
                driver.Close();
            }
        }
        public static void SerializeWeatherDataAsArff(this IEnumerable<Weather> data, string filePath)
        {
            using (var sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write), Encoding.ASCII))
            {
                sw.WriteLine("@relation WeatherForecast");
                sw.WriteLine("@attribute WindSpeedKmph numeric");
                sw.WriteLine($"@attribute WindDirection360 {{Variable,{string.Join(",", Enumerable.Range(10, 360).Where(x => x % 10 == 0))}}}");
                sw.WriteLine("@attribute RelativeHumidityPercent numeric");
                sw.WriteLine("@attribute AmosphericPressureMb numeric");
                sw.WriteLine("@attribute DewPointCelcius numeric");
                sw.WriteLine("@attribute TemperatureCelcius numeric");
                sw.WriteLine("@data");
                sw.Write(string.Join(Environment.NewLine, data.Where(x => x.WindSpeedKmph.HasValue).Select(x => $"{x.WindSpeedKmph.Value.ToString(CultureInfo.InvariantCulture)},{x.WindDirection360?.ToString(CultureInfo.InvariantCulture) ?? "Variable"},{x.RelativeHumidityPercent.ToString(CultureInfo.InvariantCulture)},{x.AtmosphericPressureMb.ToString(CultureInfo.InvariantCulture)},{x.DewPointCelcius.ToString(CultureInfo.InvariantCulture)},{x.TemperatureCelcius.ToString(CultureInfo.InvariantCulture)}")));
            }
        }
    }
}