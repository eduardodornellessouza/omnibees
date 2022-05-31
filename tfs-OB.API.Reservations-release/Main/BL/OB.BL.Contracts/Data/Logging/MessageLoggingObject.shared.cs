namespace OB.Reservation.BL.Contracts.Data.Payments
{
    public enum Operation
    {
        Insert,
        Update,
        Delete
    };

    //add messages
    public enum TypeMessage
    {
        RateLoggingMessage,
        ReservationMessage,
        TourOperatorMessage,
        RoomTypeMessage
    };
}