using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.BL.Contracts.Data.Rates
{
    [DataContract]
    public class RateRoomDetailsLightDataWithChildRecordsCustom : ContractBase
    {
        public RateRoomDetailsLightDataWithChildRecordsCustom()
        {
        }

        ///// <summary>UID do RateRoom a atualizar</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public long RateRoomId { get; set; }

        /// <summary>UID da tarifa a atualizar</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RateId { get; set; }

        /// <summary>UID do quarto a atualizar</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long RoomTypeId { get; set; }


        /// <summary>Data de inicio</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime RRDFromDate { get; set; }

        /// <summary>Data de fim</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime RRDToDate { get; set; }


        /// <summary>Currency Code ISO 4217</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string CurrencyISO { get; set; }


        /// <summary>Especifica que o update afeta as segundas-feiras</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsMonday { get; set; }

        /// <summary>Especifica que o update afeta as terças-feiras</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsTuesday { get; set; }

        /// <summary>Especifica que o update afeta as quartas-feiras</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsWednesday { get; set; }

        /// <summary>Especifica que o update afeta as quintas-feiras</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsThursday { get; set; }

        /// <summary>Especifica que o update afeta as sextas-feiras</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsFriday { get; set; }

        /// <summary>Especifica que o update afeta as sabados</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsSaturday { get; set; }

        /// <summary>Especifica que o update afeta as domingos</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool IsSunday { get; set; }



        /// <summary>Allotment a atualizar (igual para todos os dias)</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? Allotment { get; set; }



        /// <summary>Lista com número de adultos ex: '1,2,3,4'</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> NoOfAdultList { get; set; }

        /// <summary>Lista com preços de adultos ex:'25.00,35.00,12.00'</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<decimal> AdultPriceList { get; set; }



        /// <summary>Lista com número de filhos ex: '1,2,3,4'</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<int> NoOfChildsList { get; set; }

        /// <summary>Preços dos filhos ex:'25.00,35.00,12.00'</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<decimal> ChildPriceList { get; set; }




        /// <summary>Restrição mínimo de dias</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MinDays { get; set; }

        /// <summary>Restrição máximo de dias</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? MaxDays { get; set; }

        /// <summary>Restrição de número de dias a reservar</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? StayThrough { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int? ReleaseDays { get; set; }



        /// <summary>Restrição fechado à chegada</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsClosedOnArr { get; set; }

        /// <summary>Restrição fechado à partida</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsClosedOnDep { get; set; }



        /// <summary>Restrição bloquear vendas</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IsBlocked { get; set; }

        /// <summary>Lista de canais a aplicar os FECHOS</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> SelectedChannelList { get; set; }


        /// <summary>UserUID que gerou a atualização</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public long CreatedBy { get; set; }


        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string UpdateId { get; set; }


        #region Excluded Fields
        ///// <summary>Preço em modelo por quarto</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public decimal Price { get; set; }


        ///// <summary>Lista de booleanos a indicar se a variação é para cima ou para baixo em cada adulto</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> AdultPriceVariationIsValueDecrease { get; set; }

        ///// <summary>Lista de booleanos a indicar se a variação é percentagem ou valor para cada adulto</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> AdultPriceVariationIsPercentage { get; set; }

        ///// <summary>Lista de variações de preço por adulto</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<decimal> AdultPriceVariationValue { get; set; }

        ///// <summary>Lista de booleanos a indicar se a variação é para cima ou para baixo em cada criança</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> ChildPriceVariationIsValueDecrease { get; set; }

        ///// <summary>Lista de booleanos a indicar se a variação é percentagem ou valor para cada criança</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> ChildPriceVariationIsPercentage { get; set; }

        ///// <summary>Lista de variações de preço por criança</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<decimal> ChildPriceVariationValue { get; set; }

        ///// <summary>Indica se o update é por variação de preço ou preço directo</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool IsPriceVariation { get; set; }

        ///// <summary>Indica se a variação de preço é ascendente ou descendente</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool PriceVariationIsValueDecrease { get; set; }

        ///// <summary>Indica se a variação de preço é por percentagem</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool PriceVariationIsPercentage { get; set; }

        ///// <summary>Valor da variação de preço</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public decimal PriceVariationValue { get; set; }


        ///// <summary>Fechado à partida por dia de semana - Lista com 7 dias da semana em boleanos '1,1,1,1,1,1,1' Ex o primeiro '1' aplica restrição às 2ªs feiras no intervalo de datas</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> arrClosedDays { get; set; }

        ///// <summary>Fechado à saida por dia de semana - Lista com 7 dias da semana em boleanos Ex '1,1,1,1,1,1,1' Ex o primeiro '1' aplica restrição às 2ªs feiras no intervalo de datas</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> depClosedDays { get; set; }

        ///// <summary>Fecho de vendas por semana - Lista dias da semana a fechar vendas EX '1,0,0,0,0,0,0' Só fecha 2ªs feiras</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public List<bool> closedDays { get; set; }


        ///// <summary>Se a variação de preço de cama extra é ascendente ou descendente</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool PriceVariationIsExtraBedValueDecrease { get; set; }

        ///// <summary>Se a variação de preço de cama extra é ascendente ou descendente</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool PriceVariationIsExtraBedPercentage { get; set; }

        ///// <summary>Variação de preço da cama extra</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public decimal PriceVariationExtraBedValue { get; set; }


        ///// <summary>Indica se muda preços</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsPriceChanged { get; set; }

        ///// <summary>Indica se muda cama extra</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsExtraBedPriceChanged { get; set; }

        ///// <summary>Indica se muda allotment</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsAllotmentChanged { get; set; }

        ///// <summary>Indica se muda políticas de deposito</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsDepositPolicyChanged { get; set; }

        ///// <summary>Indica se muda políticas de cancelamento</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsCancellationPolicyChanged { get; set; }

        ///// <summary>Indica se muda restrição minimo de dias</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsMinDaysChanged { get; set; }

        ///// <summary>Indica se muda restrição maximo de dias</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsMaxDaysChanged { get; set; }

        ///// <summary>Indica se muda restrição stay through</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsStayThroughChanged { get; set; }

        ///// <summary>Indica se muda restrição release days</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsReleaseDaysChanged { get; set; }

        ///// <summary>Indica se muda restrição de fechado a saida</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsClosedOnArrivalChanged { get; set; }

        ///// <summary>Indica se muda restrição de fechado a chegada</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsClosedOnDepartureChanged { get; set; }

        ///// <summary>Indica se muda restrição de fecho de vendas</summary>
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public bool RateRoomDetailChangedCustom_IsStoppedSaleChanged { get; set; }


        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public int? GeneratedBy { get; set; }


        ///// <summary>Preço cama extra</summary>
        //[System.Obsolete]
        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public decimal? ExtraBedPrice { get; set; }


        //[DataMember(IsRequired = false, EmitDefaultValue = false)]
        //public int? Allocation { get; set; }


        #endregion  Excluded Fields

    }
}