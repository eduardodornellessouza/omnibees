using System;
namespace OB.DL.Common.QueryResultObjects
{
    public class ExternalMarkupRule
    {
        public long ChannelUid { get; set; }

        /// <summary>
        /// Tpi Id
        /// </summary>
        public long PosCode { get; set; }

        /// <summary>
        /// --RuleType: 0 - Default, 1 - Pais, 2 - Cidade, 3 -Hotel
        /// </summary>
        public int RuleType { get; set; }

        /// <summary>
        /// 1-Nets;2-Comiisionaveis;3-Ambas
        /// </summary>
        public ExternalRatesTypeTarget RatesTypeTarget { get; set; }

        /// <summary>
        /// 1-Defenir;2-Diminuir;3-Aumentar
        /// </summary>
        public ExternalMarkupCommissionType MarkupType { get; set; }

        public decimal Markup { get; set; }

        /// <summary>
        /// 1-Defenir;2-Diminuir;3-Aumentar
        /// </summary>
        public ExternalMarkupCommissionType CommissionType { get; set; }

        public decimal Commission { get; set; }

        public long Currency_UID { get; set; }

        public bool CommissionValueType { get; set; }
    }

    public enum ExternalMarkupCommissionType : byte
    {
        Define = 1,
        Down = 2,
        Up = 3
    }

    public enum ExternalRatesTypeTarget : byte
    {
        Net = 1,
        Comm = 2,
        Both = 3
    }
}