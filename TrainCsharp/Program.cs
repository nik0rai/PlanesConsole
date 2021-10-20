using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TrainCsharp
{
    enum PlaneType
    {
        Airbus_A220 = 0,
        Airbus_A310 = 1,
        Airbus_A320,
        Airbus_A330,
        Airbus_A340,
        Airbus_A350,
        Airbus_A380,
        Boeing_717,
        Boeing_737,
        Boeing_747,
        Boeing_757,
        Boeing_767,
        Boeing_777,
        Boeing_787,
        ATR_42_72,
        BAe_Avro_RJ,
        Bombardier_Dash_8,
        Bombardier_CRJ,
        Embraer_ERJ,
        Embraer_170_190,
        Saab,
        SUPERJET_100,
        Typolev_Ty_204,
        Ilushin_Il_96,
        Ilushin_Il_114,
        Antonov_An_38,
        Antonov_An_140,
        Antonov_An_148,
        Comac_ARJ21,
        Comac_C919,
        CRAIC_CR929,
        Harbin_Y_12,
        Xian_MA60,
        invalidType //если указано выше 32 или ниже 0
    }

    class Flight
    {      
        [JsonProperty("flight_id")]
        public string Id { get; set; } //id рейса

        [JsonProperty("type")]
        public PlaneType Type { get; set; } //тип самолета

        [JsonProperty("date")]
        public DateTime Date { get; set; } //дата совершения рейса

        [JsonProperty("status")]
        public byte Status { get; set; } //оценка качества самолета

        [JsonProperty("onboard_passangers")]
        public ushort Passangers { get; set; } //кол-во пассажиров на самолете

        [JsonProperty("lethals")]
        public ushort Deaths { get; set; } //смертей во время перелета   

        public override string ToString() => '#' + Id + ", Type = " + Type +
            ", Day = " + Date.Day + ", Status = " + Status +
            ", Passengers = " + Passangers + ", RIP = " + Deaths;

        
    }

    class Filter
    {
        public Filter(LinkedList<Flight> _arr) => array = _arr;

        /// <summary>
        /// Sorts by days, when false returns least deadliest days
        /// </summary>
        /// <param name="Descending">When false => least deadliest days </param>
        /// <returns></returns>
        public Dictionary<int, int> DeadliestDays(bool Descending = true) {

            Dictionary<int, int> top_days = new(); //от самых смертельных дней
            
            foreach (var item in array.Distinct().OrderByDescending(x => x.Deaths))
                if (!top_days.ContainsKey(item.Date.Day))
                    top_days.Add(item.Date.Day, array.Where(x => x.Date.Day == item.Date.Day).Sum(x => x.Deaths));

            if (!Descending) top_days = top_days.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            return top_days;
        }


        /// <summary>
        /// Sorts by days, when false returns worst models
        /// </summary>
        /// <param name="Descending">When false => worst models </param>
        /// <returns></returns>
        public Dictionary<PlaneType, byte> BestModels(bool Descending = true)
        {
            Dictionary<PlaneType, byte> quality = new(); //от самых хороших моделей до, самых плохих

            foreach (var item in array.Distinct().OrderByDescending(x => x.Status))
                if (!quality.ContainsKey(item.Type))
                    {
                        int sum = array.Where(x => x.Type == item.Type).Sum(x => x.Status);
                        int ammount = array.Where(x => x.Type == item.Type).Count();
                        quality.Add(item.Type, (byte)(sum / ammount)); //по среднем значению
                    }

            if (!Descending) quality = quality.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            return quality;
        }

        /// <summary>
        /// Sorts by days, when false returns most breakable days
        /// </summary>
        /// <param name="Descending">When false => most breakable days </param>
        /// <returns></returns>
        public Dictionary<int,int> LeastBreakableDays(bool Descending = true)
        {
            Dictionary<int, int> breakable = new(); //дни где меньше всего поломок
            foreach (var item in array.Distinct().OrderByDescending(x => x.Status))
                if (!breakable.ContainsKey(item.Date.Day))
                    breakable.Add(item.Date.Day, array.Where(x => x.Date.Day == item.Date.Day).Sum(x => x.Status));

            if (!Descending) breakable = breakable.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            return breakable;
        }

        private readonly LinkedList<Flight> array;
    }

    class Probe
    {
        public int Day { get; set; }
        public double Chance { get; set; }
        public PlaneType Type { get; set; }

        public override string ToString() => $"{Type.ToString()}, {Day.ToString()}, {Chance.ToString()}";
    }

    class Program
    {
        static void Main()
        {

            #region Parsing
            LinkedList<Flight> flights = new();

            Flight f1 = new();
            f1.Id = "ASP-23";
            f1.Type = (PlaneType)5;
            f1.Date = new DateTime(2012, 5, 16);
            f1.Status = 3;
            f1.Passangers = 230;
            f1.Deaths = 0;

            Flight f3 = new();
            f3.Id = "ASP2-21";
            f3.Type = (PlaneType)5;
            f3.Date = new DateTime(2020, 7, 12);
            f3.Status = 2;
            f3.Passangers = 133;
            f3.Deaths = 30;

            Flight f4 = new();
            f4.Id = "ASP2-21";
            f4.Type = (PlaneType)5;
            f4.Date = new DateTime(2020, 7, 12);
            f4.Status = 9;
            f4.Passangers = 133;
            f4.Deaths = 30;


            Flight f2 = new();
            f2.Id = "ASP2-21";
            f2.Type = (PlaneType)2;
            f2.Date = new DateTime(2020, 7, 16);
            f2.Status = 4;
            f2.Passangers = 133;
            f2.Deaths = 7;


            flights.AddLast(f1);
            flights.AddLast(f3);
            flights.AddLast(f4);
            flights.AddLast(f2);
            #endregion

            Dictionary<int, Probe> pairs = new(); //day
            int index = -1;
            foreach (var item in flights.Distinct().OrderByDescending(x => x.Date.Day))
            {
                index++;                
                int ammount = flights.Where(x => x.Type == item.Type && x.Date.Day == item.Date.Day).Count();
                int normal = flights.Where(x => x.Type == item.Type && x.Date.Day == item.Date.Day && x.Status > 6).Count(); //кол-во не сломанных

                if (!pairs.ContainsKey(index))              
                    pairs.Add(index, new Probe() { Type = item.Type, Day = item.Date.Day, Chance = (double)normal / ammount }); //вероятность, что попадется нормальный самолет            
            }

            foreach (var item in pairs)
            {
                if (item.Value.Chance < 0.5)
                    Console.WriteLine("For day {0} we should buy some of type {1} planes! Because: {2}", item.Value.Day, item.Value.Type, item.Value.Chance);
                else Console.WriteLine("OK Because: {0}", item);
            }

            foreach (var item in pairs)
                Console.WriteLine(item);
            

            foreach (var item in flights)
                Console.WriteLine(item);

            Filter filter = new(flights);

            var a = filter.DeadliestDays(false);
            foreach (var item in a)
                Console.WriteLine(item.Key);

            var b = filter.BestModels(false);
            foreach (var item in b)
                Console.WriteLine(item.Key);


            var c = filter.LeastBreakableDays();
            foreach (var item in c)
                Console.WriteLine(item.Key);         
        }
    }
}