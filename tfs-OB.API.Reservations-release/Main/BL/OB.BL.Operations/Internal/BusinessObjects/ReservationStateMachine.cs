using OB.Reservation.BL.Contracts.Responses;
using OB.BL.Operations.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using enums = OB.Reservation.BL.Constants;
using OB.BL.Operations.Extensions;
using System.Diagnostics;


namespace OB.BL.Operations.Internal.BusinessObjects
{
    public static class ReservationStateMachine
    {
        private static Dictionary<ReservationStateTransaction, enums.ReservationTransactionStatus> reservationTransactionStatus =
            new Dictionary<ReservationStateTransaction, enums.ReservationTransactionStatus>
            {
                #region MODIFY RESERVATION

                #region INITIATE

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Commited), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Ignored), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Cancelled), enums.ReservationTransactionStatus.Pending },

                //{ new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                //    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.None), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Commited), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Ignored), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Initiate, enums.ReservationTransactionStatus.Cancelled), enums.ReservationTransactionStatus.Pending },

                #endregion

                #region MODIFY

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Modify, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Pending },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Modify, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Pending },

                #endregion

                #region COMMIT

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Commit, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Commited },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Commit, enums.ReservationTransactionStatus.CommitedOnRequest), enums.ReservationTransactionStatus.OnRequestAccepted },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Commit, enums.ReservationTransactionStatus.OnRequestAccepted), enums.ReservationTransactionStatus.Commited },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Commit, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Commited },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Commit, enums.ReservationTransactionStatus.Commited), enums.ReservationTransactionStatus.Commited },

                #endregion

                #region IGNORE

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Ignore, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Ignored },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.A, 
                    enums.ReservationTransactionAction.Ignore, enums.ReservationTransactionStatus.CommitedOnRequest), enums.ReservationTransactionStatus.RefusedOnRequest },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Ignore, enums.ReservationTransactionStatus.Pending), enums.ReservationTransactionStatus.Ignored },

                { new ReservationStateTransaction(enums.ReservationAction.Modify, enums.ReservationTransactionType.B, 
                    enums.ReservationTransactionAction.Ignore, enums.ReservationTransactionStatus.Ignored), enums.ReservationTransactionStatus.Ignored },

                #endregion

                #endregion
            };

        private static Dictionary<ReservationStateTransaction, enums.ReservationStatus> reservationStatus =
            new Dictionary<ReservationStateTransaction, enums.ReservationStatus>
            {
                #region INSERT

                { new ReservationStateTransaction(enums.ReservationAction.Insert, null, null, enums.ReservationTransactionStatus.Commited), enums.ReservationStatus.Booked },

                #endregion

                #region MODIFY

                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.Commited), enums.ReservationStatus.Modified },
                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.Ignored), enums.ReservationStatus.Modified },
                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.Cancelled), enums.ReservationStatus.Modified },
                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.Pending), enums.ReservationStatus.Pending },
                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.RefusedOnRequest), enums.ReservationStatus.RefusedOnRequest },
                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.CancelledOnRequest), enums.ReservationStatus.CancelledOnRequest },
                { new ReservationStateTransaction(enums.ReservationAction.Modify, null, null, enums.ReservationTransactionStatus.OnRequestAccepted), enums.ReservationStatus.OnRequestAccepted },

                #endregion

                #region UPDATE

                { new ReservationStateTransaction(enums.ReservationAction.Update, null, null, enums.ReservationTransactionStatus.Commited), enums.ReservationStatus.Booked },

                #endregion
            };

        public static enums.ReservationTransactionStatus GetNextReservationTransactionState(enums.ReservationAction reservationAction, enums.ReservationTransactionType transactionType,
            enums.ReservationTransactionAction? transactionAction, enums.ReservationTransactionStatus currentTransactionState)
        {
            ReservationStateTransaction transition = new ReservationStateTransaction(reservationAction, transactionType, transactionAction, currentTransactionState);
            enums.ReservationTransactionStatus nextState;
            if (!ReservationStateMachine.reservationTransactionStatus.TryGetValue(transition, out nextState))
                throw Reservation.BL.Contracts.Responses.Errors.InvalidReservationTransactionStatus.ToBusinessLayerException();

            return nextState;
        }

        public static enums.ReservationStatus GetNextReservationState(enums.ReservationAction reservationAction, enums.ReservationTransactionStatus currentTransactionState)
        {
            ReservationStateTransaction transition = new ReservationStateTransaction(reservationAction, null, null, currentTransactionState);
            enums.ReservationStatus nextState;
            if (!ReservationStateMachine.reservationStatus.TryGetValue(transition, out nextState))
                throw Reservation.BL.Contracts.Responses.Errors.InvalidReservationStatus.ToBusinessLayerException();

            return nextState;
        }
    }

    public class ReservationStateTransaction
    {
        readonly enums.ReservationAction _reservationAction;
        readonly enums.ReservationTransactionType? _transactionType;
        readonly enums.ReservationTransactionAction? _transactionAction;
        readonly enums.ReservationTransactionStatus _transactionStatus;

        public enums.ReservationAction ResevationAction
        {
            get { return _reservationAction; }
        }

        public enums.ReservationTransactionType? TransactionType
        {
            get { return _transactionType; }
        }

        public enums.ReservationTransactionAction? TransactionAction
        {
            get { return _transactionAction; }
        }

        public enums.ReservationTransactionStatus TransactionStatus
        {
            get { return _transactionStatus; }
        }  

        public ReservationStateTransaction(enums.ReservationAction reservationAction, enums.ReservationTransactionType? transactionType, enums.ReservationTransactionAction? transactionAction,
            enums.ReservationTransactionStatus currentTransactionStatus)
        {
            _reservationAction = reservationAction;
            _transactionType = transactionType;
            _transactionAction = transactionAction;
            _transactionStatus = currentTransactionStatus;
        }

        public override int GetHashCode()
        {
            return _reservationAction.GetHashCode() + _transactionType.GetHashCode() + _transactionAction.GetHashCode() + _transactionStatus.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ReservationStateTransaction other = obj as ReservationStateTransaction;
            return other != null && this._reservationAction == other.ResevationAction && this._transactionType == other.TransactionType && this._transactionAction == other.TransactionAction 
                && this._transactionStatus == other.TransactionStatus;
        }
    }
}
