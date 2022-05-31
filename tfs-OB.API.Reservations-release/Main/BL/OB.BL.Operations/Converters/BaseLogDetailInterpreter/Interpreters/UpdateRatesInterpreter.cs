using OB.BL.Contracts.Data.BaseLogDetails;
using OB.Events.Contracts;
using OB.Events.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OB.Events.Contracts.Enums;
using System.Collections;
using Newtonsoft.Json;
using OB.BL.Contracts.Data.Rates;

namespace OB.BL.Operations.Converters.BaseLogDetailInterpreter.Interpreters
{
    public class UpdateRatesInterpreter : DefaultInterpreter
    {
        public UpdateRatesInterpreter(NotificationBase notification)
            : base(notification)
        {
     
        }

        enum Weekdays
        {
            Monday = 0,
            Tuesday = 1,
            Wednesday = 2,
            Thursday = 3,
            Friday = 4,
            Saturday = 5,
            Sunday = 6
        }

        public override Dictionary<string, int> GetOrder()
        {
            var defaultOrder = base.GetOrder();

            // Organize the way you want

            return defaultOrder;
        }

        protected override void HandleGroup(Events.Contracts.EntityDelta delta)
        {
            switch (delta.EntityType)
            {
                //case OB.Events.Contracts.Enums.EntityEnum.RatePeriod.ToString():
                case "RatePeriod":

                    var ratePeriodGroup = GetDeltaGroup(delta, !HasDeltaGroup(delta));

                    if (!ratePeriodGroup.ChangeGroups.Any()) ratePeriodGroup.ChangeGroups.Add(new ChangeGroup());

                    if (ratePeriodGroup.ChangeGroups.FirstOrDefault().Changes.Count == 0)
                    {
                        Change ratePeriod = new Change("Intervalo de datas", QueryEntityProperty(delta, EntityPropertyEnum.DateFrom).CurrentValue.ToString() 
                            + " - "
                            + QueryEntityProperty(delta, EntityPropertyEnum.DateTo).CurrentValue.ToString(), null);

                        ratePeriodGroup.ChangeGroups.FirstOrDefault().Changes.Add(ratePeriod);

                    }
                    else
                    {
                        var changeToUpdate = ratePeriodGroup.ChangeGroups.FirstOrDefault().Changes.FirstOrDefault(c => c.Field == "Intervalo de datas");
                        changeToUpdate.NewValue = changeToUpdate.NewValue 
                                                  + "; " + 
                                                  QueryEntityProperty(delta, EntityPropertyEnum.DateFrom).CurrentValue.ToString() 
                                                  + " - " + 
                                                  QueryEntityProperty(delta, EntityPropertyEnum.DateTo).CurrentValue.ToString();

                    }

                    break;
                
                case "RateRoomDetail":

                    var RateRoomDetailsGroup = GetDeltaGroup(delta, !HasDeltaGroup(delta));

                    if (!RateRoomDetailsGroup.AddedEntityNames.Any(q => q == QueryEntityProperty(delta, Events.Contracts.Enums.EntityPropertyEnum.RateName).CurrentValue.ToString()))
                        RateRoomDetailsGroup.AddedEntityNames.Add(QueryEntityProperty(delta, Events.Contracts.Enums.EntityPropertyEnum.RateName).CurrentValue.ToString());


                    EntityGroup rate;
                    string rateName = QueryEntityProperty(delta, Events.Contracts.Enums.EntityPropertyEnum.RateName).CurrentValue.ToString();

                    if (!RateRoomDetailsGroup.EntitiesGroups.Any(q => q.EntityType == EntityEnum.Rate.ToString() && q.Name == rateName))
                    {
                        rate = new EntityGroup(EntityEnum.Rate.ToString());
                        rate.Name = rateName;
                    }
                    else
                        rate = RateRoomDetailsGroup.EntitiesGroups.FirstOrDefault(q => q.EntityType == EntityEnum.Rate.ToString() && q.Name == rateName);

                    
                    // Add Room Entity to Rate EntityGroup
                    Entity room = new Entity();
                    room.Name = QueryEntityProperty(delta, EntityPropertyEnum.RoomName).CurrentValue.ToString();

                    // Add RoomName to AddedEntityNames in Rate EntityGroup 
                    if(!rate.AddedEntityNames.Any(q => q == room.Name))
                        rate.AddedEntityNames.Add(room.Name);

                    var rateCurrencySymbol = " CURRENCY SYMBOL BUG"; //QueryEntityProperty(delta, Events.Contracts.Enums.EntityPropertyEnum.CurrencySymbol).CurrentValue.ToString();

                    var pricesChangeGroup = new ChangeGroup();

                    // PriceWeekDays
                    List<bool> priceWeekDays = new List<bool>();
                    var priceWeekDaysBools = QueryEntityProperty(delta, EntityPropertyEnum.PriceWeekDays).CurrentValue.ToString()
                                            .Replace("\r\n", "").Replace("  ", "").Replace("[", "").Replace("]", "")
                                            .Split(',').ToList();

                    foreach (var p in priceWeekDaysBools)
                        priceWeekDays.Add(Convert.ToBoolean(p));
                        
                    List<string> priceWeekDaysList = new List<string>();
                    string priceWeekDaysString = string.Empty;

                    for (int i = 0; i < priceWeekDays.Count; i++)
                    {
                        if (priceWeekDays[i] == true)
                        {
                            priceWeekDaysList.Add(Enum.GetName(typeof(Weekdays), i));
                        }
                    }

                    priceWeekDaysString = String.Join("; ", priceWeekDaysList);
                    pricesChangeGroup.Changes.Add(new Change(EntityPropertyEnum.PriceWeekDays.ToString(), priceWeekDaysString, null));



                    var prices = JsonConvert.DeserializeObject<List<OB.Events.Contracts.Data.UpdateRates.Prices>>(QueryEntityProperty(delta, EntityPropertyEnum.Prices).CurrentValue.ToJSON());

                    
                    if (prices != null)
                        foreach (OB.Events.Contracts.Data.UpdateRates.Prices price in prices)
                        {
                            Change priceChange = new Change();

                            if (price.IsAdult)
                            {
                                priceChange.Field = price.PersonNr + " Pax";
                            }
                            else
                            {
                                if (price.IsExtraBed)
                                    priceChange.Field = "Extra Bed";
                                else
                                    priceChange.Field = price.PersonNr + " Child";
                            }


                            if (price.IsPercentage)
                            {
                                priceChange.NewValue = (price.IsPriceDecreased ? "-" : "+") + price.Price + "%";
                            }
                            else
                            {
                                if (!price.IsPriceDecreased && !price.IsPriceDecreased)
                                    priceChange.NewValue = price.Price + rateCurrencySymbol;
                                else
                                    priceChange.NewValue = (price.IsPriceDecreased ? "-" : "+") + price.Price + rateCurrencySymbol;
                            }


                            pricesChangeGroup.Changes.Add(priceChange);
                        }

                    // Allotment
                    int allotment = Int16.Parse(QueryEntityProperty(delta, EntityPropertyEnum.Allotment).CurrentValue.ToString());
                    if (allotment > -1)
                        pricesChangeGroup.Changes.Add(new Change(EntityPropertyEnum.Allotment.ToString(), allotment.ToString(), null));

                    if (pricesChangeGroup.Changes.Count(q => q.Field != EntityPropertyEnum.PriceWeekDays.ToString()) > 0)
                        room.ChangeGroups.Add(pricesChangeGroup);
                                                            
                    if (!rate.Entities.Any(q => q.Name == room.Name && q.ChangeGroups.Any(c => c.Changes.Any(h => h.Field == EntityPropertyEnum.PriceWeekDays.ToString()
                                          && h.NewValue == room.ChangeGroups.FirstOrDefault().Changes.FirstOrDefault(i => i.Field == EntityPropertyEnum.PriceWeekDays.ToString()).NewValue))))
                        rate.Entities.Add(room);


                    

                    if(!RateRoomDetailsGroup.EntitiesGroups.Any(q => q.AddedEntityNames == rate.AddedEntityNames))
                        RateRoomDetailsGroup.EntitiesGroups.Add(rate);

                    ChangeGroup rrdChangeGroup = new ChangeGroup();

                    //string maxDays = QueryEntityProperty(delta, EntityPropertyEnum.MaxDays).CurrentValue.ToString();
                    //if (maxDays != null)
                    //    RateRoomDetailsGroup.ChangeGroups.FirstOrDefault().Changes.Add(new Change(EntityPropertyEnum.MaxDays.ToString(), maxDays, null));
                   /*
                    string minDays = QueryEntityProperty(delta, EntityPropertyEnum.MinDays).CurrentValue.ToString();
                    if (minDays != "")
                        rrdChangeGroup.Changes.Add(new Change(EntityPropertyEnum.MinDays.ToString(), minDays, null));

                    RateRoomDetailsGroup.ChangeGroups.Add(rrdChangeGroup);
                    */
                    break;

                //default:
                //    base.HandleGroup(delta);
                //    break;
            }
        }

        public override GridLineDetail GetGridLine(NotificationBase notification)
        {
            var gridLine = base.GetGridLine(notification);

            gridLine.Rates = GetRateNames(notification);
            gridLine.Rooms = GetRoomNames(notification);
            gridLine.Dates = GetDatePeriods(notification);

            return gridLine;
        }

        private List<UpdatePeriod> GetDatePeriods(NotificationBase notification)
        {
            var datePeriods = new List<UpdatePeriod>();

            foreach(var datePeriod in QueryEntityDeltas(Events.Contracts.Enums.EntityEnum.RatePeriod))
            {
                var newUpdatePeriod = new UpdatePeriod();

                var dateFromString = QueryEntityProperty(datePeriod, Events.Contracts.Enums.EntityPropertyEnum.DateFrom).CurrentValue.ToString();
                var dateToString = QueryEntityProperty(datePeriod, Events.Contracts.Enums.EntityPropertyEnum.DateTo).CurrentValue.ToString();

                newUpdatePeriod.DateFrom = DateTime.Parse(dateFromString);
                newUpdatePeriod.DateTo = DateTime.Parse(dateToString);

                datePeriods.Add(newUpdatePeriod);
            }

            return datePeriods;
        }
       
        private string GetRoomNames(NotificationBase notification)
        {
            var roomsProperties = QueryEntityProperties(EntityEnum.RateRoomDetail, EntityPropertyEnum.RoomName);

            return String.Join(";", roomsProperties.Select(q => q.CurrentValue).Distinct().ToList());
        }

        private string GetRateNames(NotificationBase notification)
        {
            var ratesProperties = QueryEntityProperties(EntityEnum.RateRoomDetail, EntityPropertyEnum.RateName);

            return String.Join(";", ratesProperties.Select(q => q.CurrentValue).Distinct().ToList());
        }
        
    }
}
