using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.Domain.Reservations
{
    public class MasterTimeZone
    {
        public MasterTimeZone(int uid, int code, string timeZoneName, string displayName)
        {
            this.UID = uid;
            this.Code = code;
            this.TimeZoneName = timeZoneName;
            this.DisplayName = displayName;
        }

        public int UID { get; private set; }
        public int Code {get; private set;}
        public string TimeZoneName {get; private set;}
        public string DisplayName {get; private set;}
        public string StandardTime {get;private set;}
        public string Regoin {get;private set;}
        public string Value {get;private set;}
    }


    public class MasterTimeZones
    {
        //UID,  Code,   TimeZone,   DisplayName
        public static readonly MasterTimeZone DateLine                      = new MasterTimeZone(1,0,       "Dateline Standard Time","(UTC-12:00) International Date Line West"                                       );
        public static readonly MasterTimeZone UTC_11                        = new MasterTimeZone(2,110,     "UTC-11","(UTC-11:00) Coordinated Universal Time -11"                                                   );
        public static readonly MasterTimeZone HawaiianStandardTime          = new MasterTimeZone(3,200,     "Hawaiian Standard Time","(UTC-10:00) Hawaii"                                                           );
        public static readonly MasterTimeZone AlaskanStandardTime           = new MasterTimeZone(4, 300,    "Alaskan Standard Time", "(UTC-09:00) Alaska");
        public static readonly MasterTimeZone PacificStandardTime           = new MasterTimeZone(5, 400,    "Pacific Standard Time", "(UTC-08:00) Pacific Time (US and Canada)");
        public static readonly MasterTimeZone MexicoPacificStandardTime     = new MasterTimeZone(6, 410,    "Pacific Standard Time (Mexico)", "(UTC-08:00)Baja California");
        public static readonly MasterTimeZone MountainStandardTime          = new MasterTimeZone(7,500,     "Mountain Standard Time","(UTC-07:00) Mountain Time (US and Canada)"                                    );
        public static readonly MasterTimeZone MexicoMountainStandardTime    = new MasterTimeZone(8, 510,    "Mountain Standard Time (Mexico)", "(UTC-07:00) Chihuahua, La Paz, Mazatlan");
        public static readonly MasterTimeZone USMountainStandardTime        = new MasterTimeZone(9,520,     "US Mountain Standard Time","(UTC-07:00) Arizona"                                                       );
        public static readonly MasterTimeZone CanadaCentralStandardTime     = new MasterTimeZone(10,600,    "Canada Central Standard Time","(UTC-06:00) Saskatchewan"                                              );
        public static readonly MasterTimeZone CentralAmericaStandardTime    = new MasterTimeZone(11,10,     "Central America Standard Time","(UTC-06:00) Central America"                                           );
        public static readonly MasterTimeZone CentralStandardTime           = new MasterTimeZone(12,620,    "Central Standard Time","(UTC-06:00) Central Time (US and Canada)"                                     );
        public static readonly MasterTimeZone CentralStandardTimeMexico     = new MasterTimeZone(13,630,    "Central Standard Time (Mexico)","((UTC-06:00) Guadalajara, Mexico City, Monterrey"                );
        public static readonly MasterTimeZone EasternStandardTime           = new MasterTimeZone(14,700,    "Eastern Standard Time","(UTC-05:00) Eastern Time (US and Canada)"                                     );
        public static readonly MasterTimeZone SAPacificStandardTime         = new MasterTimeZone(15,710,    "SA Pacific Standard Time","(UTC-05:00) Bogota, Lima, Quito"                                       );
        public static readonly MasterTimeZone USEasternStandardTime         = new MasterTimeZone(16,720,    "US Eastern Standard Time","(UTC-05:00) Indiana (East)"                                                );
        public static readonly MasterTimeZone VenezuelaStandardTime         = new MasterTimeZone(17,840,    "Venezuela Standard Time","(UTC-04:30) Caracas"                                                        );
        public static readonly MasterTimeZone AtlanticStandardTime          = new MasterTimeZone(18,800,    "Atlantic Standard Time","(UTC-04:00) Atlantic Time (Canada)"                                          );
        public static readonly MasterTimeZone CentralBrazilianStandardTime  = new MasterTimeZone(19,810,    "Central Brazilian Standard Time","(UTC-04:00) Cuiaba"                                                 );
        public static readonly MasterTimeZone PacificSAStandardTime         = new MasterTimeZone(20,820,    "Pacific SA Standard Time","(UTC-04:00) Santiago"                                                      );
        public static readonly MasterTimeZone SAWesternStandardTime         = new MasterTimeZone(21,830,    "SA Western Standard Time","(UTC-04:00) Georgetown, La Paz, Manaus, San Juan"                      );
        public static readonly MasterTimeZone ParaguayStandardTime          = new MasterTimeZone(22,850,    "Paraguay Standard Time","(UTC-04:00) Asuncion"                                                        );
        public static readonly MasterTimeZone NewfoundlandStandardTime      = new MasterTimeZone(23,900,    "Newfoundland Standard Time","(UTC-03:30) Newfoundland"                                                );
        public static readonly MasterTimeZone ESouthAmericaStandardTime     = new MasterTimeZone(24,910,    "E. South America Standard Time","(UTC-03:00) Brasilia"                                                );
        public static readonly MasterTimeZone GreenlandStandardTime         = new MasterTimeZone(25,920,    "Greenland Standard Time","(UTC-03:00) Greenland"                                                      );
        public static readonly MasterTimeZone MontevideoStandardTime        = new MasterTimeZone(26,930,    "Montevideo Standard Time","(UTC-03:00) Montevideo"                                                    );
        public static readonly MasterTimeZone SAEasternStandardTime         = new MasterTimeZone(27,940,    "SA Eastern Standard Time","(UTC-03:00) Cayenne, Fortaleza"                                        );
        public static readonly MasterTimeZone ArgentinaStandardTime         = new MasterTimeZone(28,950,    "Argentina Standard Time","(UTC-03:00) Buenos Aires"                                                   );
        public static readonly MasterTimeZone Mid_AtlanticStandardTime      = new MasterTimeZone(29,1000,   "Mid-Atlantic Standard Time","(UTC-02:00) Mid-Atlantic"                                               );
        public static readonly MasterTimeZone UTC_02                        = new MasterTimeZone(30,1010,   "UTC-02","(UTC-02:00) Coordinated Universal Time -02"                                                 );
        public static readonly MasterTimeZone AzoresStandardTime            = new MasterTimeZone(31,1100,   "Azores Standard Time","(UTC-01:00) Azores"                                                           );
        public static readonly MasterTimeZone CapeVerdeStandardTime         = new MasterTimeZone(32,1110,   "Cape Verde Standard Time","(UTC-01:00) Cape Verde Is."                                               );
        public static readonly MasterTimeZone GMTStandardTime               = new MasterTimeZone(33,1200,   "GMT Standard Time","(UTC) Dublin, Edinburgh, Lisbon, London"                                     );
        public static readonly MasterTimeZone GreenwichStandardTime         = new MasterTimeZone(34,1210,   "Greenwich Standard Time","(UTC) Monrovia, Reykjavik"                                             );
        public static readonly MasterTimeZone MoroccoStandardTime           = new MasterTimeZone(35,1220,   "Morocco Standard Time","(UTC) Casablanca"                                                            );
        public static readonly MasterTimeZone UTC                           = new MasterTimeZone(36,1230,   "UTC","(UTC) Coordinated Universal Time"                                                              );
        public static readonly MasterTimeZone CentralEuropeStandardTime     = new MasterTimeZone(37,1300,   "Central Europe Standard Time","(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague"    );
        public static readonly MasterTimeZone CentralEuropeanStandardTime   = new MasterTimeZone(38,1310,   "Central European Standard Time","(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb"                   );
        public static readonly MasterTimeZone RomanceStandardTime           = new MasterTimeZone(39,1320,   "Romance Standard Time","(UTC+01:00) Brussels, Copenhagen, Madrid, Paris"                         );
        public static readonly MasterTimeZone WCentralAfricaStandardTime    = new MasterTimeZone(40,1330,   "W. Central Africa Standard Time","(UTC+01:00) West Central Africa"                                   );
        public static readonly MasterTimeZone WEuropeStandardTime           = new MasterTimeZone(41,1340,   "W. Europe Standard Time","(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"          );
        public static readonly MasterTimeZone NamibiaStandardTime           = new MasterTimeZone(42,1350,   "Namibia Standard Time","(UTC+01:00) Windhoek"                                                        );
        public static readonly MasterTimeZone EEuropeStandardTime           = new MasterTimeZone(43,1400,   "E. Europe Standard Time","(UTC+02:00) Minsk"                                                         );
        public static readonly MasterTimeZone EgyptStandardTime             = new MasterTimeZone(44,1410,   "Egypt Standard Time","(UTC+02:00) Cairo"                                                             );
        public static readonly MasterTimeZone FLEStandardTime               = new MasterTimeZone(45,1420,   "FLE Standard Time","(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius"                   );
        public static readonly MasterTimeZone GTBStandardTime               = new MasterTimeZone(46,1430,   "GTB Standard Time","(UTC+02:00) Athens, Bucharest"                                               );
        public static readonly MasterTimeZone IsraelStandardTime            = new MasterTimeZone(47,1440,   "Israel Standard Time","(UTC+02:00) Jerusalem"                                                        );
        public static readonly MasterTimeZone JordanStandardTime            = new MasterTimeZone(48,1450,   "Jordan Standard Time","(UTC+02:00) Amman"                                                            );
        public static readonly MasterTimeZone MiddleEastStandardTime        = new MasterTimeZone(49,1460,   "Middle East Standard Time","(UTC+02:00) Beirut"                                                      );
        public static readonly MasterTimeZone SouthAfricaStandardTime       = new MasterTimeZone(50,1470,   "South Africa Standard Time","(UTC+02:00) Harare, Pretoria"                                       );
        public static readonly MasterTimeZone SyriaStandardTime             = new MasterTimeZone(51,1480,   "Syria Standard Time","(UTC+02:00) Damascus"                                                          );
        public static readonly MasterTimeZone TurkeyStandardTime            = new MasterTimeZone(52,1490,   "Turkey Standard Time","(UTC+02:00) Istanbul"                                                         );
        public static readonly MasterTimeZone ArabStandardTime              = new MasterTimeZone(53,1500,   "Arab Standard Time","(UTC+03:00) Kuwait, Riyadh"                                                 );
        public static readonly MasterTimeZone ArabicStandardTime            = new MasterTimeZone(54,1510,   "Arabic Standard Time","(UTC+03:00) Baghdad"                                                          );
        public static readonly MasterTimeZone EAfricaStandardTime           = new MasterTimeZone(55,1520,   "E. Africa Standard Time","(UTC+03:00) Nairobi"                                                       );
        public static readonly MasterTimeZone KaliningradStandardTime       = new MasterTimeZone(56,1530,   "Kaliningrad Standard Time","(UTC+03:00) Kaliningrad"                                                 );
        public static readonly MasterTimeZone IranStandardTime              = new MasterTimeZone(57,1550,   "Iran Standard Time","(UTC+03:30) Tehran"                                                             );
        public static readonly MasterTimeZone RussianStandardTime           = new MasterTimeZone(58,1540,   "Russian Standard Time","(UTC+04:00) Moscow, St. Petersburg, Volgograd"                           );
        public static readonly MasterTimeZone ArabianStandardTime           = new MasterTimeZone(59,1600,   "Arabian Standard Time","(UTC+04:00) Abu Dhabi, Muscat"                                           );
        public static readonly MasterTimeZone AzerbaijanStandardTime        = new MasterTimeZone(60,1610,   "Azerbaijan Standard Time","(UTC+04:00) Baku"                                                         );
        public static readonly MasterTimeZone CaucasusStandardTime          = new MasterTimeZone(61,1620,   "Caucasus Standard Time","(UTC+04:00) Yerevan"                                                        );
        public static readonly MasterTimeZone GeorgianStandardTime          = new MasterTimeZone(62,1640,   "Georgian Standard Time","(UTC+04:00) Tbilisi"                                                        );
        public static readonly MasterTimeZone MauritiusStandardTime         = new MasterTimeZone(63,1650,   "Mauritius Standard Time","(UTC+04:00) Port Louis"                                                    );
        public static readonly MasterTimeZone AfghanistanStandardTime       = new MasterTimeZone(64,1630,   "Afghanistan Standard Time","(UTC+04:30) Kabul"                                                       );
        public static readonly MasterTimeZone WestAsiaStandardTime          = new MasterTimeZone(65,1710,   "West Asia Standard Time","(UTC+05:00) Tashkent"                                                      );
        public static readonly MasterTimeZone PakistanStandardTime          = new MasterTimeZone(66,1750,   "Pakistan Standard Time","(UTC+05:00) Islamabad, Karachi"                                         );
        public static readonly MasterTimeZone IndiaStandardTime             = new MasterTimeZone(67,1720,   "India Standard Time","(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi"                           );
        public static readonly MasterTimeZone SriLankaStandardTime          = new MasterTimeZone(68,1730,   "Sri Lanka Standard Time","(UTC+05:30) Sri Jayawardenepura"                                           );
        public static readonly MasterTimeZone NepalStandardTime             = new MasterTimeZone(69,1740,   "Nepal Standard Time","(UTC+05:45) Kathmandu"                                                         );
        public static readonly MasterTimeZone EkaterinburgStandardTime      = new MasterTimeZone(70,1700,   "Ekaterinburg Standard Time","(UTC+06:00) Ekaterinburg"                                               );
        public static readonly MasterTimeZone CentralAsiaStandardTime       = new MasterTimeZone(71,1800,   "Central Asia Standard Time","(UTC+06:00) Astana"                                                     );
        public static readonly MasterTimeZone BangladeshStandardTime        = new MasterTimeZone(72,1830,   "Bangladesh Standard Time","(UTC+06:00) Dhaka"                                                        );
        public static readonly MasterTimeZone MyanmarStandardTime           = new MasterTimeZone(73,1820,   "Myanmar Standard Time","(UTC+06:30) Yangon (Rangoon)"                                                );
        public static readonly MasterTimeZone NCentralAsiaStandardTime      = new MasterTimeZone(74,1810,   "N. Central Asia Standard Time","(UTC+07:00) Novosibirsk"                                             );
        public static readonly MasterTimeZone SEAsiaStandardTime            = new MasterTimeZone(75,1910,   "SE Asia Standard Time","(UTC+07:00) Bangkok, Hanoi, Jakarta"                                   );
        public static readonly MasterTimeZone NorthAsiaStandardTime         = new MasterTimeZone(76,1900,   "North Asia Standard Time","(UTC+08:00) Krasnoyarsk"                                                  );
        public static readonly MasterTimeZone ChinaStandardTime             = new MasterTimeZone(77,2000,   "China Standard Time","(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"                         );
        public static readonly MasterTimeZone SingaporeStandardTime         = new MasterTimeZone(78,2020,   "Singapore Standard Time","(UTC+08:00) Kuala Lumpur, Singapore"                                   );
        public static readonly MasterTimeZone TaipeiStandardTime            = new MasterTimeZone(79,2030,   "Taipei Standard Time","(UTC+08:00) Taipei"                                                           );
        public static readonly MasterTimeZone WAustraliaStandardTime        = new MasterTimeZone(80,2040,   "W. Australia Standard Time","(UTC+08:00) Perth"                                                      );
        public static readonly MasterTimeZone UlaanbaatarStandardTime       = new MasterTimeZone(81,2050,   "Ulaanbaatar Standard Time","(UTC+08:00) Ulaanbaatar"                                                 );
        public static readonly MasterTimeZone NorthAsiaEastStandardTime     = new MasterTimeZone(82,2010,   "North Asia East Standard Time","(UTC+09:00) Irkutsk"                                                 );
        public static readonly MasterTimeZone KoreaStandardTime             = new MasterTimeZone(83,2100,   "Korea Standard Time","(UTC+09:00) Seoul"                                                             );
        public static readonly MasterTimeZone TokyoStandardTime             = new MasterTimeZone(84,2110,   "Tokyo Standard Time","(UTC+09:00) Osaka, Sapporo, Tokyo"                                         );
        public static readonly MasterTimeZone AUSCentralStandardTime        = new MasterTimeZone(85,2130,   "AUS Central Standard Time","(UTC+09:30) Darwin"                                                      );
        public static readonly MasterTimeZone CenAustraliaStandardTime      = new MasterTimeZone(86,2140,   "Cen. Australia Standard Time","(UTC+09:30) Adelaide"                                                 );
        public static readonly MasterTimeZone YakutskStandardTime           = new MasterTimeZone(87,2120,   "Yakutsk Standard Time","(UTC+10:00) Yakutsk"                                                         );
        public static readonly MasterTimeZone AUSEasternStandardTime        = new MasterTimeZone(88,2200,   "AUS Eastern Standard Time","(UTC+10:00) Canberra, Melbourne, Sydney"                            );
        public static readonly MasterTimeZone EAustraliaStandardTime        = new MasterTimeZone(89,2210,   "E. Australia Standard Time","(UTC+10:00) Brisbane"                                                   );
        public static readonly MasterTimeZone TasmaniaStandardTime          = new MasterTimeZone(90,2220,   "Tasmania Standard Time","(UTC+10:00) Hobart"                                                         );
        public static readonly MasterTimeZone WestPacificStandardTime       = new MasterTimeZone(91,2240,   "West Pacific Standard Time","(UTC+10:00) Guam, Port Moresby"                                     );
        public static readonly MasterTimeZone VladivostokStandardTime       = new MasterTimeZone(92,2230,   "Vladivostok Standard Time","(UTC+11:00) Vladivostok"                                                 );
        public static readonly MasterTimeZone CentralPacificStandardTime    = new MasterTimeZone(93,2300,   "Central Pacific Standard Time","(UTC+11:00) Solomon Is., New Caledonia"                          );
        public static readonly MasterTimeZone MagadanStandardTime           = new MasterTimeZone(94,2310,   "Magadan Standard Time","(UTC+12:00) Magadan"                                                         );
        public static readonly MasterTimeZone FijiStandardTime              = new MasterTimeZone(95,2400,   "Fiji Standard Time","(UTC+12:00) Fiji"                                                               );
        public static readonly MasterTimeZone NewZealandStandardTime        = new MasterTimeZone(96,2410,   "New Zealand Standard Time","(UTC+12:00) Auckland, Wellington"                                    );
        public static readonly MasterTimeZone UTC_12                        = new MasterTimeZone(97,2430,   "UTC+12","(UTC+12:00) Coordinated Universal Time +12"                                                 );
        public static readonly MasterTimeZone TongaStandardTime             = new MasterTimeZone(98,2500,   "Tonga Standard Time","(UTC+13:00) Nuku'alofa"                                                        );
        public static readonly MasterTimeZone SamoaStandardTime             = new MasterTimeZone(99,2510,   "Samoa Standard Time","(UTC-11:00)Samoa"                                                              );


        public static readonly Dictionary<long, MasterTimeZone> AllTimeZones = new Dictionary<long, MasterTimeZone>()
                                                                    {
                                                                      {DateLine.UID, DateLine}
                                                                      , {UTC_11.UID,UTC_11                             }       
                                                                      , {HawaiianStandardTime.UID,HawaiianStandardTime               }
                                                                      , {AlaskanStandardTime.UID,AlaskanStandardTime                }
                                                                      , {PacificStandardTime.UID,PacificStandardTime                }
                                                                      , {MexicoPacificStandardTime.UID,MexicoPacificStandardTime          }
                                                                      , {MountainStandardTime.UID,MountainStandardTime               }
                                                                      , {MexicoMountainStandardTime.UID,MexicoMountainStandardTime         }
                                                                      , {USMountainStandardTime.UID,USMountainStandardTime             }
                                                                      , {CanadaCentralStandardTime.UID,CanadaCentralStandardTime          }
                                                                      , {CentralAmericaStandardTime.UID,CentralAmericaStandardTime         }
                                                                      , {CentralStandardTime.UID,CentralStandardTime                }
                                                                      , {CentralStandardTimeMexico.UID,CentralStandardTimeMexico          }
                                                                      , {EasternStandardTime.UID,EasternStandardTime                }
                                                                      , {SAPacificStandardTime.UID,SAPacificStandardTime              }
                                                                      , {USEasternStandardTime.UID,USEasternStandardTime              }
                                                                      , {VenezuelaStandardTime.UID,VenezuelaStandardTime              }
                                                                      , {AtlanticStandardTime.UID,AtlanticStandardTime               }
                                                                      , {CentralBrazilianStandardTime.UID,CentralBrazilianStandardTime       }
                                                                      , {PacificSAStandardTime.UID,PacificSAStandardTime              }
                                                                      , {SAWesternStandardTime.UID,SAWesternStandardTime              }
                                                                      , {ParaguayStandardTime.UID,ParaguayStandardTime               }
                                                                      , {NewfoundlandStandardTime.UID,NewfoundlandStandardTime           }
                                                                      , {ESouthAmericaStandardTime.UID,ESouthAmericaStandardTime          }
                                                                      , {GreenlandStandardTime.UID,GreenlandStandardTime              }
                                                                      , {MontevideoStandardTime.UID,MontevideoStandardTime             }
                                                                      , {SAEasternStandardTime.UID,SAEasternStandardTime              }
                                                                      , {ArgentinaStandardTime.UID,ArgentinaStandardTime              }
                                                                      , {Mid_AtlanticStandardTime.UID,Mid_AtlanticStandardTime           }
                                                                      , {UTC_02.UID,UTC_02                             }
                                                                      , {AzoresStandardTime.UID,AzoresStandardTime                 }
                                                                      , {CapeVerdeStandardTime.UID,CapeVerdeStandardTime              }
                                                                      , {GMTStandardTime.UID,GMTStandardTime                    }
                                                                      , {GreenwichStandardTime.UID,GreenwichStandardTime              }
                                                                      , {MoroccoStandardTime.UID,MoroccoStandardTime                }
                                                                      , {UTC.UID,UTC                                }
                                                                      , {CentralEuropeStandardTime.UID,CentralEuropeStandardTime          }
                                                                      , {CentralEuropeanStandardTime.UID,CentralEuropeanStandardTime        }
                                                                      , {RomanceStandardTime.UID,RomanceStandardTime                }
                                                                      , {WCentralAfricaStandardTime.UID,WCentralAfricaStandardTime         }
                                                                      , {WEuropeStandardTime.UID,WEuropeStandardTime                }
                                                                      , {NamibiaStandardTime.UID,NamibiaStandardTime                }
                                                                      , {EEuropeStandardTime.UID,EEuropeStandardTime                }
                                                                      , {EgyptStandardTime.UID,EgyptStandardTime                  }
                                                                      , {FLEStandardTime.UID,FLEStandardTime                    }
                                                                      , {GTBStandardTime.UID,GTBStandardTime                    }
                                                                      , {IsraelStandardTime.UID,IsraelStandardTime                 }
                                                                      , {JordanStandardTime.UID,JordanStandardTime                 }
                                                                      , {MiddleEastStandardTime.UID,MiddleEastStandardTime             }
                                                                      , {SouthAfricaStandardTime.UID,SouthAfricaStandardTime            }
                                                                      , {SyriaStandardTime.UID,SyriaStandardTime                  }
                                                                      , {TurkeyStandardTime.UID,TurkeyStandardTime                 }
                                                                      , {ArabStandardTime.UID,ArabStandardTime                   }
                                                                      , {ArabicStandardTime.UID,ArabicStandardTime                 }
                                                                      , {EAfricaStandardTime.UID,EAfricaStandardTime                }
                                                                      , {KaliningradStandardTime.UID,KaliningradStandardTime            }
                                                                      , {IranStandardTime.UID,IranStandardTime                   }
                                                                      , {RussianStandardTime.UID,RussianStandardTime                }
                                                                      , {ArabianStandardTime.UID,ArabianStandardTime                }
                                                                      , {AzerbaijanStandardTime.UID,AzerbaijanStandardTime             }
                                                                      , {CaucasusStandardTime.UID,CaucasusStandardTime               }
                                                                      , {GeorgianStandardTime.UID,GeorgianStandardTime               }
                                                                      , {MauritiusStandardTime.UID,MauritiusStandardTime              }
                                                                      , {AfghanistanStandardTime.UID,AfghanistanStandardTime            }
                                                                      , {WestAsiaStandardTime.UID,WestAsiaStandardTime               }
                                                                      , {PakistanStandardTime.UID,PakistanStandardTime               }
                                                                      , {IndiaStandardTime.UID,IndiaStandardTime                  }
                                                                      , {SriLankaStandardTime.UID,SriLankaStandardTime               }
                                                                      , {NepalStandardTime.UID,NepalStandardTime                  }
                                                                      , {EkaterinburgStandardTime.UID,EkaterinburgStandardTime           }
                                                                      , {CentralAsiaStandardTime.UID,CentralAsiaStandardTime            }
                                                                      , {BangladeshStandardTime.UID,BangladeshStandardTime             }
                                                                      , {MyanmarStandardTime.UID,MyanmarStandardTime                }
                                                                      , {NCentralAsiaStandardTime.UID,NCentralAsiaStandardTime           }
                                                                      , {SEAsiaStandardTime.UID,SEAsiaStandardTime                 }
                                                                      , {  NorthAsiaStandardTime.UID,NorthAsiaStandardTime              }
                                                                      , {ChinaStandardTime.UID,ChinaStandardTime                  }
                                                                      , {SingaporeStandardTime.UID,SingaporeStandardTime              }
                                                                      , {  TaipeiStandardTime.UID,TaipeiStandardTime                 }
                                                                      , {WAustraliaStandardTime.UID,WAustraliaStandardTime             }
                                                                      , {UlaanbaatarStandardTime.UID,UlaanbaatarStandardTime            }
                                                                      , {NorthAsiaEastStandardTime.UID,NorthAsiaEastStandardTime          }
                                                                      , {KoreaStandardTime.UID,KoreaStandardTime                  }
                                                                      , {TokyoStandardTime.UID,TokyoStandardTime                  }
                                                                      , {AUSCentralStandardTime.UID,AUSCentralStandardTime             }
                                                                      , {CenAustraliaStandardTime.UID,CenAustraliaStandardTime           }
                                                                      , {YakutskStandardTime.UID,YakutskStandardTime                }
                                                                      , {AUSEasternStandardTime.UID,AUSEasternStandardTime             }
                                                                      , {EAustraliaStandardTime.UID,EAustraliaStandardTime             }
                                                                      , {TasmaniaStandardTime.UID,TasmaniaStandardTime               }
                                                                      , {WestPacificStandardTime.UID,WestPacificStandardTime            }
                                                                      , {VladivostokStandardTime.UID,VladivostokStandardTime            }
                                                                      , {CentralPacificStandardTime.UID,CentralPacificStandardTime         }
                                                                      , {MagadanStandardTime.UID,MagadanStandardTime                }
                                                                      , {FijiStandardTime.UID,FijiStandardTime                   }
                                                                      , {NewZealandStandardTime.UID,NewZealandStandardTime             }
                                                                      , {UTC_12.UID,UTC_12                             }
                                                                      , {TongaStandardTime.UID,TongaStandardTime                  }
                                                                      , {SamoaStandardTime.UID,SamoaStandardTime                  }
                                                                    };

        public static MasterTimeZone FindByUID(long uid)
        {
            MasterTimeZone timezone = null;
            AllTimeZones.TryGetValue(uid, out timezone);
            return timezone;
        }

    }
}
    