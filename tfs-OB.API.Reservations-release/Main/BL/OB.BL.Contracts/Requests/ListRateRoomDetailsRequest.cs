using OB.Reservation.BL.Contracts.Data.Rates;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OB.Reservation.BL.Contracts.Requests
{
    /// <summary>
    /// Class that stores the arguments for retrieving room prices.
    /// Object that contains all the required objects
    /// </summary>
    [DataContract]
    public class ListRateRoomDetailsRequest : PagedRequestBase
    {
        /// <summary>Lita de UIDs a pesquisar</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> UIDs { get; set; }

        /// <summary>UID do RateRoom a pesquisar</summary>
        [DataMember(IsRequired = true, EmitDefaultValue = false)]
        public long RateRoom_UID { get; set; }

        /// <summary>Data de inicio da pesquisa, ou data actual senão for indicada</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? RRDFromDate { get; set; }

        /// <summary>Data de fim da pesquisa, ou futuro senão for indicada</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime? RRDToDate { get; set; }

        /// <summary>Booleano para considerar somente fechos de vendas na resposta</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? IncludeOnlyCloseSales { get; set; }

        /// <summary>Canais que devem estar incluídos na resposta</summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<long> SelectedChannelList { get; set; }

        /// <summary>Booleano para excluir detalhes da resposta e retornar somente número total de registos encontrados e booleano que indica se existem RRD para todos os dias pesquisados </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? ExcludeDetails { get; set; }

    }
}